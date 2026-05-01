using System.Collections.Generic;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.Item;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    // 踏残甲
    //正练-该功法在敌人护甲失去共计3次耐久度时自动释放。若为自动释放，之后封禁自身2秒。发挥一成威力时，敌人的一件随机护甲失去2耐久度。
    //逆练-该功法在自身护甲失去共计3次耐久度时自动释放.若为自动释放，之后封禁自身2秒。发挥一成威力时，敌人的一件随机护甲失去2耐久度。
    internal class Tuifa5 : CombatSkillEffectBase
    {
        private const int TriggerDurabilityLossCount = 3;

        private const int AutoCastSilenceFrames = 120;

        private const sbyte MinPower = 10;

        private const int ArmorItemType = 1;

        private const int DurabilityLossPerHit = 2;

        private bool _checking;

        private bool _delaying;

        private bool _affecting;

        private int _durabilityLossCount;

        private int _lastMailboxToken;

        public Tuifa5()
        {
        }

        public Tuifa5(CombatSkillKey skillKey)
            : base(skillKey, 4122)
        {
        }

        public override void OnEnable(DataContext context)
        {
            _checking = false;
            _delaying = false;
            _affecting = false;
            _durabilityLossCount = 0;
            _lastMailboxToken = TuifaAutoCastMailbox.GetToken(base.CharacterId);
            Events.RegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.RegisterHandler_CombatChangeDurability(OnCombatChangeDurability);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(OnCombatStateMachineUpdateEnd);
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            Events.UnRegisterHandler_CombatChangeDurability(OnCombatChangeDurability);
        }

        private void OnCombatChangeDurability(DataContext context, CombatCharacter character, ItemKey itemKey, int delta)
        {
            if (_affecting || _delaying)
            {
                return;
            }

            if (delta >= 0 || itemKey.ItemType != ArmorItemType)
            {
                return;
            }

            int triggerCharId = base.IsDirect ? base.CurrEnemyChar.GetId() : base.CharacterId;
            if (character.GetId() != triggerCharId)
            {
                return;
            }

            _durabilityLossCount++;
            if (_durabilityLossCount < TriggerDurabilityLossCount)
            {
                return;
            }

            _durabilityLossCount = 0;
            _delaying = true;
        }

        private void OnCombatStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            if (combatChar.GetId() != base.CharacterId)
            {
                return;
            }

            TryScheduleMailboxAutoCast();

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
            }
            else
            {
                _delaying = false;
            }
        }

        private void TryScheduleMailboxAutoCast()
        {
            if (_affecting || _delaying)
            {
                return;
            }

            int token = TuifaAutoCastMailbox.GetToken(base.CharacterId);
            if (token == _lastMailboxToken)
            {
                return;
            }

            _lastMailboxToken = token;
            if (DomainManager.Combat.CanCastSkill(base.CombatChar, base.SkillTemplateId, costFree: true, checkRange: true))
            {
                _delaying = true;
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
                DomainManager.Combat.SilenceSkill(context, base.CombatChar, base.SkillTemplateId, AutoCastSilenceFrames, 100);
            }

            if (interrupted || power < MinPower)
            {
                return;
            }

            int affectCount = power / MinPower;
            bool anyChanged = false;
            for (int i = 0; i < affectCount; i++)
            {
                if (!TryReduceRandomEnemyArmorDurability(context))
                {
                    break;
                }
                anyChanged = true;
            }

            if (anyChanged)
            {
                ShowSpecialEffectTips(0);
            }
        }

        private bool TryReduceRandomEnemyArmorDurability(DataContext context)
        {
            List<ItemKey> availableArmorKeys = new List<ItemKey>(4);
            for (int i = 0; i < base.CurrEnemyChar.Armors.Length; i++)
            {
                ItemKey armorKey = base.CurrEnemyChar.Armors[i];
                if (!armorKey.IsValid() || armorKey.ItemType != ArmorItemType)
                {
                    continue;
                }

                Armor armor = DomainManager.Item.GetElement_Armors(armorKey.Id);
                if (armor != null && armor.GetCurrDurability() > 0)
                {
                    availableArmorKeys.Add(armorKey);
                }
            }

            if (availableArmorKeys.Count <= 0)
            {
                return false;
            }

            int selectedIndex = context.Random.Next(availableArmorKeys.Count);
            ChangeDurability(context, base.CurrEnemyChar, availableArmorKeys[selectedIndex], -DurabilityLossPerHit);
            return true;
        }
    }
}
