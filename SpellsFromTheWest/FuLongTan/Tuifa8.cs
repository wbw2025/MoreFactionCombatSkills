using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    internal class Tuifa8 : CombatSkillEffectBase
    {
        // 赤龙翻
        //正：该功法在敌人胸背或腰腹被添加伤势标记时自动释放.若自动释放，释放结束之后会封禁该功法2秒。
        //逆：该功法在自己胸背或腰腹被添加伤势标记时自动释放.若自动释放，释放结束之后会封禁该功法2秒。
        private const sbyte ChestBodyPart = 0;

        private const sbyte WaistBodyPart = 1;

        private const int AutoCastSilenceFrames = 120;

        public Tuifa8()
        {
        }

        public Tuifa8(CombatSkillKey skillKey)
            : base(skillKey, 54119)
        {
        }

        private bool _checking;

        private bool _delaying;

        private bool _affecting;


        public override void OnEnable(DataContext context)
        {
            _checking = false;
            _delaying = false;
            _affecting = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_AddInjury(OnAddInjury);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_AddInjury(OnAddInjury);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        private void OnAddInjury(DataContext context, CombatCharacter character, sbyte bodyPart, bool isInner, sbyte value, bool changeToOld)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            if (!IsLegBodyPart(bodyPart) || value <= 0)
            {
                return;
            }
            int triggerTargetId = base.IsDirect ? base.EnemyChar.GetId() : base.CharacterId;
            if (character.GetId() != triggerTargetId)
            {
                return;
            }

            _delaying = true;
        }

        private static bool IsLegBodyPart(sbyte bodyPart)
        {
            return bodyPart == ChestBodyPart || bodyPart == WaistBodyPart;
        }

        private void OnCombatStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            if (combatChar.GetId() != base.CharacterId)
            {
                return;
            }

            bool checking = _checking;
            _checking = false;
            if (combatChar.NeedUseSkillFreeId >= 0 || !_delaying || combatChar.StateMachine.GetCurrentStateType() != CombatCharacterStateType.Idle)
            {
                return;
            }
            _checking = true;
            if (checking)
            {
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
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            if (_affecting)
            {
                _affecting = false;
                _delaying = false;
                DomainManager.Combat.SilenceSkill(context, base.CombatChar, base.SkillTemplateId, AutoCastSilenceFrames * (IsDirect ? 2 : 1), 100);
                return;
            }
            _affecting = false;
        }
    }
}
