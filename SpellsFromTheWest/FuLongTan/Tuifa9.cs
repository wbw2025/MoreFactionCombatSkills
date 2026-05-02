using Config;
using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using AutoCastEvents = MoreFactionCombatSkillsBackend.Helpers.MoreFactionCombarSkillsEvents;
using System.Collections.Generic;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    // 衔尾踢
    //正练 - 该功法在其他腿法自动释放后自动释放。发挥一成威力后，若运用者醉酒，则向敌人添加一个随机破绽。
    //逆练 - 该功法在其他腿法自动释放后自动释放。发挥一成威力后，若运用者醉酒，则为自己添加一个随机破绽并使得该功法威力+10%,持续到战斗结束.
    internal class Tuifa9 : CombatSkillEffectBase
    {
        private const sbyte MinPower = 10;

        private const sbyte ReverseAddFlawLevel = 1;

        private const sbyte DirectAddFlawLevel = 1;

        private int ReversePowerBonus = 0;

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        private static readonly HashSet<int> _queuedCharIds = new HashSet<int>();

        public Tuifa9()
        {
        }

        public Tuifa9(CombatSkillKey skillKey)
            : base(skillKey, 4118)
        {
        }

        public override void OnEnable(DataContext context)
        {
            _checking = false;
            _delaying = false;
            _affecting = false;
            // Register power modifier for reverse practice (field ID 199 = power/威力)
            CreateAffectedData(199, EDataModifyType.Add, base.SkillTemplateId);
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            AutoCastEvents.RegisterHandler_CastSkillFree(OnAutoCastSkillFree);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            AutoCastEvents.UnRegisterHandler_CastSkillFree(OnAutoCastSkillFree);
            _queuedCharIds.Remove(base.CharacterId);
        }

        /// <summary>
        /// Returns true if Tuifa9 is currently queued for autocast.
        /// Other Tuifas should yield if this returns true to allow Tuifa9 highest priority.
        /// </summary>
        public static bool IsQueued(int charId)
        {
            return _queuedCharIds.Contains(charId);
        }

        private void OnCombatStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            if (combatChar.GetId() != base.CharacterId)
            {
                return;
            }

            bool checking = _checking;
            _checking = false;
            if (combatChar.NeedUseSkillFreeId >= 0 || !_delaying || _affecting || combatChar.StateMachine.GetCurrentStateType() != CombatCharacterStateType.Idle)
            {
                return;
            }

            _checking = true;
            if (!checking)
            {
                return;
            }

            if (DomainManager.Combat.CanCastSkill(base.CombatChar, base.SkillTemplateId, costFree: true, checkRange: true))
            {
                _delaying = false;
                _affecting = true;
                // _queuedCharIds entry is kept until the cast fully ends (see OnCastSkillEnd),
                // so that IsQueued() stays true while Tuifa9 is casting and Tuifa1 keeps yielding.
                ShowSpecialEffectTips(0);
                DomainManager.Combat.CastSkillFree(context, base.CombatChar, base.SkillTemplateId);
            }
            else
            {
                _delaying = false;
                _queuedCharIds.Remove(base.CharacterId);
            }
        }

        private void OnAutoCastSkillFree(DataContext context, CombatCharacter character, short skillId, ECombatCastFreePriority priority)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            if (character.GetId() != base.CharacterId || skillId == base.SkillTemplateId)
            {
                return;
            }

            if (!IsAttackSkill(skillId))
            {
                return;
            }

            _delaying = true;
            _queuedCharIds.Add(base.CharacterId);
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            // Handle completion of Tuifa9 itself
            if (charId == base.CharacterId && skillId == base.SkillTemplateId)
            {
                _queuedCharIds.Remove(base.CharacterId);
                _affecting = false;

                if (interrupted || power < MinPower)
                {
                    return;
                }

                // Check if caster is drunk
                var casterChar = base.CombatChar.GetCharacter();
                bool isDrunk = casterChar.GetEatingItems().ContainsWine();

                if (!isDrunk)
                {
                    return;
                }

                if (base.IsDirect)
                {
                    // 正练: Add random flaw to enemy
                    DomainManager.Combat.AddFlaw(context, base.CurrEnemyChar, DirectAddFlawLevel, SkillKey, -1);
                    ShowSpecialEffectTips(1);
                }
                else
                {
                    // 逆练: Add random flaw to self; power bonus is applied via GetModifyValue
                    DomainManager.Combat.AddFlaw(context, base.CombatChar, ReverseAddFlawLevel, SkillKey, -1);
                    ReversePowerBonus += 10;
                    DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 199);
                    ShowSpecialEffectTips(1);
                }
                return;
            }

            // Trigger scheduling now comes from OnAutoCastSkillFree only.
        }

        private static bool IsAttackSkill(short skillId)
        {
            return Config.CombatSkill.Instance[skillId].EquipType == 1;
        }

        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            // Apply +10 power bonus only for reverse practice (IsDirect == false)
            if (dataKey.CharId == base.CharacterId && dataKey.FieldId == 199 && 
                dataKey.CombatSkillId == base.SkillTemplateId && !base.IsDirect)
            {
                return ReversePowerBonus;
            }
            return 0;
        }
    }
}
