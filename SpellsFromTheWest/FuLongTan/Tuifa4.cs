using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan

{
    // 潜龙出渊式
    //正练：该功法在自身解除封禁时自动释放。发挥十成威力时，若运用者醉酒，自身每有一个仍在封禁中的功法，向敌人添加一个破绽标记。
    //逆练：该功法在敌人的功法在解除封禁时自动释放。发挥十成威力时，若运用者醉酒，敌人每有一个仍在封禁中的功法，向敌人添加一个破绽标记。
    internal class Tuifa4 : CombatSkillEffectBase
    {
        private const sbyte FullPower = 100;

        private const sbyte AddFlawLevel = 1;

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        public Tuifa4()
        {
        }

        public Tuifa4(CombatSkillKey skillKey)
            : base(skillKey, 54123)
        {
        }

        public override void OnEnable(DataContext context)
        {
            _checking = false;
            _delaying = false;
            _affecting = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_SkillSilenceEnd(OnSkillSilenceEnd);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_SkillSilenceEnd(OnSkillSilenceEnd);
        }

        private void OnSkillSilenceEnd(DataContext context, CombatSkillKey skillKey)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            int triggerCharId = base.IsDirect ? base.CharacterId : base.CurrEnemyChar.GetId();
            if (skillKey.CharId != triggerCharId)
            {
                return;
            }
            if (skillKey.SkillTemplateId != base.SkillTemplateId)
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
                ShowSpecialEffectTips(0);
            }
            else
            {
                _delaying = false;
            }
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }

            _affecting = false;

            if (interrupted || power < FullPower)
            {
                return;
            }

            bool isDrunk = base.CombatChar.GetCharacter().GetEatingItems().ContainsWine();
            if (!isDrunk)
            {
                return;
            }

            CombatCharacter sourceChar = base.IsDirect ? base.CombatChar : base.CurrEnemyChar;
            int silencedSkillCount = sourceChar.GetSilenceData().CombatSkill.Count;
            if (silencedSkillCount <= 0)
            {
                return;
            }

            for (int i = 0; i < silencedSkillCount; i++)
            {
                DomainManager.Combat.AddFlaw(context, base.CurrEnemyChar, AddFlawLevel, SkillKey, -1);
            }

            ShowSpecialEffectTips(1);
        }
    }
}
