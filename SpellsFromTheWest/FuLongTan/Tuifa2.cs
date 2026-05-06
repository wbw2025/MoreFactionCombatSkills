using Config;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.Item;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    // 空杯掷月式
    //正练 - 该功法在自身饮酒时自动释放。发挥至少一成威力时，若运用者醉酒，提高敌人的内息紊乱，相当于自身内息紊乱的十分之一。
    //逆练 - 该功法在敌人使用任何物品时自动释放。发挥至少一成威力时，若运用者醉酒，降低自身的10%的新增内息紊乱。
    internal class Tuifa2 : CombatSkillEffectBase
    {
        private const sbyte MinPower = 10;

        private const int WineItemType = 9;

        private const short WineSubType = 901;

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        public Tuifa2()
        {
        }

        public Tuifa2(CombatSkillKey skillKey)
            : base(skillKey, 54125)
        {
        }

        public override void OnEnable(DataContext context)
        {
            _checking = false;
            _delaying = false;
            _affecting = false;
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_EatingItem(OnEatingItem);
            Events.RegisterHandler_UsedMedicine(OnUsedMedicine);
            Events.RegisterHandler_UsedCustomItem(OnUsedCustomItem);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_EatingItem(OnEatingItem);
            Events.UnRegisterHandler_UsedMedicine(OnUsedMedicine);
            Events.UnRegisterHandler_UsedCustomItem(OnUsedCustomItem);
        }

        private void OnEatingItem(DataContext context, GameData.Domains.Character.Character character, ItemKey itemKey)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            if (!base.IsDirect)
            {
                return;
            }

            if (character.GetId() != base.CharacterId || !IsWine(itemKey))
            {
                return;
            }

            _delaying = true;
        }

        private void OnUsedMedicine(DataContext context, int charId, ItemKey itemKey)
        {
            TryDelayByEnemyUseItem(charId);
        }

        private void OnUsedCustomItem(DataContext context, int charId, ItemKey itemKey)
        {
            TryDelayByEnemyUseItem(charId);
        }

        private void TryDelayByEnemyUseItem(int charId)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            if (base.IsDirect)
            {
                return;
            }

            if (charId != base.CurrEnemyChar.GetId())
            {
                return;
            }

            _delaying = true;
        }

        private static bool IsWine(ItemKey itemKey)
        {
            return itemKey.ItemType == WineItemType
                && Config.TeaWine.Instance[itemKey.TemplateId].ItemSubType == WineSubType;
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

            if (interrupted || power < MinPower)
            {
                return;
            }

            bool isDrunk = base.CombatChar.GetCharacter().GetEatingItems().ContainsWine();
            if (!isDrunk)
            {
                return;
            }

            if (base.IsDirect)
            {
                int selfQiDisorder = base.CombatChar.GetCharacter().GetDisorderOfQi();
                int addValue = selfQiDisorder / 7;
                if (addValue > 0)
                {
                    DomainManager.Combat.ChangeDisorderOfQiRandomRecovery(context, base.CurrEnemyChar, addValue);
                    ShowSpecialEffectTips(1);
                }
                return;
            }

            int newQiDisorder = base.CombatChar.GetCharacter().GetDisorderOfQi() - base.CombatChar.GetOldDisorderOfQi();
            int reduceValue = newQiDisorder / 10;
            if (reduceValue > 0)
            {
                DomainManager.Combat.ChangeDisorderOfQiRandomRecovery(context, base.CombatChar, -reduceValue);
                ShowSpecialEffectTips(1);
            }
        }
    }
}
