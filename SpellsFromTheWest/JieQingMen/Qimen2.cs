
using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Linq;
using static GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen2 : CombatSkillEffectBase
    {
        static sbyte sha = 19;
        static sbyte wu = 20;
        // 正 敌人每有一个无，我方每有一个杀式，伤害+10%，发挥十成威力时，敌人杀式和我方无式互换。当敌方获得无式时，若敌方的无和我方杀式大于等于5个，且此功法可以命中敌方，立即无消耗地从50%释放该功法并封禁该功法3秒。
        // 逆：敌人每有一个无或杀式，伤害+10%，发挥十成威力时，敌人杀式和我方无式互换。当敌方获得无式时，若敌方的无和杀式大于等于8个，且此功法可以命中敌方，立即无消耗地从50%释放该功法并封禁该功法3秒。
        public Qimen2()
        {
        }

        public Qimen2(CombatSkillKey skillKey)
            : base(skillKey, 54107)
        {
        }



        private void OnGetShaTrick(DataContext context, int charId, bool isAlly, bool real)
        {
            var enemyChar = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly);
            if (IsDirect && charId != base.CombatChar.GetId())
            {
                return;
            }
            if (!IsDirect && charId != enemyChar.GetId())
            {
                return;
            }

            var tricks = enemyChar.GetTricks();
            var myTricks = base.CombatChar.GetTricks();
            int count;
            if (IsDirect)
            {
                count = myTricks.Tricks.Sum((p) => (p.Value == sha ? 1 : 0));
                count += tricks.Tricks.Sum((p) => (p.Value == wu ? 1 : 0));
            }
            else
            {
                count = tricks.Tricks.Sum((p)=>(p.Value == wu || p.Value == sha? 1 : 0));
            }
            if ((IsDirect && count >= 8) || (!IsDirect && count >= 8))
            {
                _delaying = true;

            }
        }




        private bool _checking;

        private bool _delaying;

        private bool _affecting;


        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            CreateAffectedData(69, EDataModifyType.AddPercent, -1);
            _affecting = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_GetShaTrick(OnGetShaTrick);

        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_PrepareSkillBegin(OnPrepareSkillBegin);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_GetShaTrick(OnGetShaTrick);
            base.OnDisable(context);

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
            // idle: CombatCharacterStateType.Idle 
            _checking = true;
            if (!checking)
            {
                return;
            }

            if (DomainManager.Combat.CanCastSkill(base.CombatChar, base.SkillTemplateId, costFree: true, checkRange: true))
            {
                _delaying = false;
                _affecting = true;
                if (IsDirect)
                {
                    DomainManager.Combat.RemoveTrick(context, CombatChar, sha, (byte)1, true);
                }
                else
                {
                    DomainManager.Combat.RemoveTrick(context, EnemyChar, sha, (byte)2, false);
                }
                DomainManager.Combat.CastSkillFree(context, base.CombatChar, base.SkillTemplateId);

            }
            else
            {
                _delaying = false;
            }

        }

        private void OnPrepareSkillBegin(DataContext context, int charId, bool isAlly, short skillId)
        {
            if (charId == base.CharacterId && skillId == base.SkillTemplateId && _affecting)
            {
                DomainManager.Combat.ChangeSkillPrepareProgress(base.CombatChar, base.CombatChar.SkillPrepareTotalProgress * 50 / 100);
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            if (charId == base.CharacterId && skillId == base.SkillTemplateId && _affecting)
            {
                DomainManager.Combat.SilenceSkill(context, base.CombatChar, base.SkillTemplateId, 180, 100);
                _delaying = false;
                _affecting = false;
            }
            if (PowerMatchAffectRequire(power))
            {
                
                int enemySha = MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(EnemyChar, sha);
                int enemyWu = MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(EnemyChar, wu);
                int mySha = MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(this.CombatChar, sha);
                if (IsDirect)
                {
                    DomainManager.Combat.RemoveTrick(context, CombatChar, sha, (byte)mySha, true);
                    DomainManager.Combat.RemoveTrick(context, EnemyChar, wu, (byte)enemyWu, false);
                    DomainManager.Combat.AddTrick(context, EnemyChar, wu, (byte)mySha, false);
                    DomainManager.Combat.AddTrick(context, CombatChar, sha, (byte)enemyWu, false);
                }
                else
                {
                    DomainManager.Combat.RemoveTrick(context, EnemyChar, sha, (byte)enemySha, false);
                    DomainManager.Combat.RemoveTrick(context, EnemyChar, wu, (byte)enemyWu, false);
                    DomainManager.Combat.AddTrick(context, EnemyChar, wu, (byte)enemySha, false);
                    DomainManager.Combat.AddTrick(context, EnemyChar, sha, (byte)enemyWu, false);

                }

            }
            else
            {
            }
        }


        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            if (dataKey.CharId == base.CharacterId && dataKey.FieldId == 69 && _affecting && dataKey.CombatSkillId == base.SkillTemplateId && dataKey.CustomParam0 == ((!base.IsDirect) ? 1 : 0))
            {
                ShowSpecialEffectTips(1);
                return MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(EnemyChar, sha) + (IsDirect ? MoreFactionCombatSkillsBackend.Helpers.Helpers.CountTricks(EnemyChar, wu) : 0);
            }
            return 0;
        }

    }
}
