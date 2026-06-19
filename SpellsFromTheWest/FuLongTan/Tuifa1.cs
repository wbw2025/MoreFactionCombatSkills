using System;
using System.Collections.Generic;
using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Utilities;
using MoreFactionCombatSkillsBackend.Helpers;
using Events = GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    // 万龙破天步
    // 正练 - 发挥十成威力时，若已装备的其他腿法可以自动释放，则视为其满足了自动释放条件。
    // 逆练 - 开始施展此功法时，封禁所有其他已装备的且未封禁的腿法10秒。本功法在此次释放时获得基础穿透和命中，数值等于各个以此方法封禁的腿法的面板穿透和命中之和。
    internal class Tuifa1 : CombatSkillEffectBase
    {
        private const sbyte FullPower = 100;

        private const int SilenceFrames = 600;

        private const sbyte AttackSkillEquipType = 1;

        private const sbyte LegSkillType = 5;

        private bool _bonusActive;

        private HitOrAvoidInts _bonusHits;

        private OuterAndInnerInts _bonusPenetrations;

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        private static readonly int[] ExternalSkillIds = { 4118, 4119, 4120, 4122, 4123, 4124, 4125, 469, 471, 475, 482, };

        private List<short> _queuedSkills;

        public Tuifa1()
        {
        }

        public Tuifa1(CombatSkillKey skillKey)
            : base(skillKey, 54126)
        {
        }

        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            _bonusActive = false;
            _bonusHits = default;
            _bonusPenetrations = default;
            _checking = false;
            _delaying = false;
            _affecting = false;
            _queuedSkills = new List<short>();
            // 32-35: 命中, 44-45: 穿透 (all scoped to this skill template)
            for (ushort fieldId = 32; fieldId <= 35; fieldId++)
            {
                CreateAffectedData(fieldId, EDataModifyType.Add, base.SkillTemplateId);
            }
            CreateAffectedData(44, EDataModifyType.Add, base.SkillTemplateId);
            CreateAffectedData(45, EDataModifyType.Add, base.SkillTemplateId);

            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            base.OnDisable(context);
        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            if (base.IsDirect || charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }

            HitOrAvoidInts totalHits = default;
            OuterAndInnerInts totalPenetrations = default;
            bool anySilenced = false;

            IReadOnlyList<short> attackSkills = base.CombatChar.GetCombatSkillList(AttackSkillEquipType);
            for (int i = 0; i < attackSkills.Count; i++)
            {
                short otherSkillId = attackSkills[i];
                if (otherSkillId < 0 || otherSkillId == base.SkillTemplateId)
                {
                    continue;
                }

                if (DomainManager.CombatSkill.GetSkillType(base.CharacterId, otherSkillId) != LegSkillType)
                {
                    continue;
                }

                if (!DomainManager.Combat.TryGetCombatSkillData(base.CharacterId, otherSkillId, out CombatSkillData skillData)
                    || skillData.GetLeftCdFrame() != 0)
                {
                    continue;
                }

                if (DomainManager.CombatSkill.TryGetElement_CombatSkills(new CombatSkillKey(base.CharacterId, otherSkillId), out GameData.Domains.CombatSkill.CombatSkill otherSkill))
                {
                    totalHits += otherSkill.GetHitValue();
                    totalPenetrations += otherSkill.GetPenetrations();
                }
                else
                {
                    continue;
                }
                DomainManager.Combat.SilenceSkill(context, base.CombatChar, otherSkillId, SilenceFrames, 100);



                anySilenced = true;
            }

            _bonusHits = totalHits;
            _bonusPenetrations = totalPenetrations;
            _bonusActive = anySilenced;

            InvalidateBonusCache(context);

            if (anySilenced)
            {
                ShowSpecialEffectTips(1);
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId)
            {
                return;
            }

            // External queued skill finished: release lock and schedule next queued skill.
            if (_affecting && IsExternalSkill(skillId))
            {
                _affecting = false;
                _checking = false;
                _delaying = _queuedSkills.Count > 0;
                return;
            }

            if (skillId != base.SkillTemplateId)
            {
                return;
            }

            bool hadBonus = _bonusActive;
            _bonusActive = false;
            _bonusHits = default;
            _bonusPenetrations = default;
            if (hadBonus)
            {
                InvalidateBonusCache(context);
            }

            if (interrupted || power < FullPower)
            {
                return;
            }

            if (base.IsDirect)
            {
                // Populate queue with equipped external skills
                _queuedSkills.Clear();
                IReadOnlyList<short> attackSkills = base.CombatChar.GetCombatSkillList(AttackSkillEquipType);
                for (int i = 0; i < ExternalSkillIds.Length; i++)
                {
                    short externalSkillId = (short)ExternalSkillIds[i];
                    for (int j = 0; j < attackSkills.Count; j++)
                    {
                        if (attackSkills[j] == externalSkillId)
                        {
                            _queuedSkills.Add(externalSkillId);
                            break;
                        }
                    }
                }
                
                _delaying = true;
                ShowSpecialEffectTips(0);
            }
            else
            {
                _bonusHits = new HitOrAvoidInts();
                _bonusPenetrations = new OuterAndInnerInts();
            }
        }

        private void OnCombatStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            if (combatChar.GetId() != base.CharacterId)
            {
                return;
            }

            bool checking = _checking;
            _checking = false;
            if (!_delaying || _affecting || combatChar.StateMachine.GetCurrentStateType() != CombatCharacterStateType.Idle)
            {
                return;
            }

            _checking = true;
            if (!checking)
            {
                return;
            }

            // Two-frame confirmation complete - now autocast exactly one queued external skill.
            _delaying = false;

            if (_queuedSkills.Count <= 0)
            {
                _affecting = false;
                return;
            }

            // Yield to Tuifa9 priority and retry next frame.
            if (Tuifa9.IsQueued(base.CharacterId))
            {
                _delaying = true;
                return;
            }

            short nextSkillId = _queuedSkills[0];
            _queuedSkills.RemoveAt(0);

            if (DomainManager.Combat.CanCastSkill(base.CombatChar, nextSkillId, costFree: true, checkRange: true))
            {
                _affecting = true;
                ShowSpecialEffectTips(0);
                DomainManager.Combat.CastSkillFree(context, base.CombatChar, nextSkillId);
                return; // One skill at a time.
            }

            // Not castable this time; continue queue next frame if any remain.
            _affecting = false;
            _delaying = _queuedSkills.Count > 0;
        }

        private static bool IsExternalSkill(short skillId)
        {
            for (int i = 0; i < ExternalSkillIds.Length; i++)
            {
                if (ExternalSkillIds[i] == skillId)
                {
                    return true;
                }
            }

            return false;
        }

        private void InvalidateBonusCache(DataContext context)
        {
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 32);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 33);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 34);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 35);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 44);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 45);
        }

        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            if (!_bonusActive || dataKey.CharId != base.CharacterId) // do not check for templateid
            {
                return 0;
            }

            switch (dataKey.FieldId)
            {
                case 32:
                    return _bonusHits[0];
                case 33:
                    return _bonusHits[1];
                case 34:
                    return _bonusHits[2];
                case 35:
                    return _bonusHits[3];
                case 44:
                    return _bonusPenetrations.Outer;
                case 45:
                    return _bonusPenetrations.Inner;
                default:
                    return 0;
            }
        }
    }
}