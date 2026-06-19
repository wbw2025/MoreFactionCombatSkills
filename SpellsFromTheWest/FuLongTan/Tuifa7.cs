using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    internal class Tuifa7 : CombatSkillEffectBase
    {
        //捣虚劲
        //正练 该功法在敌人一个功法未造成十成威力时自动释放。若自动释放，释放结束之后会封禁该功法2秒。
        //逆练 该功法在我方一个功法未造成十成威力时自动释放。若自动释放，释放结束之后会封禁该功法4秒。

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        public Tuifa7()
        {
        }

        public Tuifa7(CombatSkillKey skillKey)
            : base(skillKey, 54120)
        {
        }

        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            _checking = false;
            _delaying = false;
            _affecting = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            base.OnDisable(context);
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId == base.CharacterId && skillId == base.SkillTemplateId && _affecting)
            {
                _affecting = false;
                _delaying = false;
                int SilenceFrames = IsDirect ? 120 : 240;

        DomainManager.Combat.SilenceSkill(context, base.CombatChar, base.SkillTemplateId, SilenceFrames, 100);
                return;
            }

            if (_affecting || interrupted || skillId < 0 || skillId == base.SkillTemplateId || power >= 100 || !IsAttackSkill(skillId))
            {
                return;
            }

            int triggerCharId = base.IsDirect ? base.EnemyChar.GetId() : base.CharacterId;
            if (charId != triggerCharId)
            {
                return;
            }

            _delaying = true;
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

            // Use a two-tick confirm so queued auto-cast can wait until the actor is truly idle.
            _checking = true;
            if (!checking)
            {
                return;
            }

            if (DomainManager.Combat.CanCastSkill(base.CombatChar, base.SkillTemplateId, costFree: true, checkRange: true))
            {
                if (Tuifa9.IsQueued(base.CharacterId))
                {
                    return;
                }

                _delaying = false;
                _affecting = true;
                DomainManager.Combat.CastSkillFree(context, base.CombatChar, base.SkillTemplateId);
            }
            else
            {
                _delaying = false;
            }
        }

        private static bool IsAttackSkill(short skillId)
        {
            return Config.CombatSkill.Instance[skillId].EquipType == 1;
        }
    }
}
