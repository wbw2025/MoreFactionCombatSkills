
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Combat.Math;
using GameData.Domains.Item;
using GameData.Domains.Organization;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    internal class Qimen7 : CombatSkillEffectBase
    {
        static sbyte fuyuan = 5;
        // 当运用者的冷静，勇壮，坚毅高于敌人时，本功法施加600点生毒烈毒，赤毒，腐毒。当运用者的福源高于敌人时本功法的毒性提高至剧毒。
        // 当运用者的热情，聪颖，合道高于敌人时，本功法施加600点生毒郁毒，寒毒，幻毒。当运用者的福源高于敌人时本功法的毒性提高至剧毒。
        public Qimen7()
        {
        }

        public Qimen7(CombatSkillKey skillKey)
            : base(skillKey, 4102)
        {
        }

        static List<KeyValuePair<sbyte, sbyte>> personalityToPoisonLUTIndirect = new List<KeyValuePair<sbyte, sbyte>>
        {
            new KeyValuePair<sbyte, sbyte>( 2, 1 ),
            new KeyValuePair<sbyte, sbyte>( 1, 3 ),
            new KeyValuePair<sbyte, sbyte>( 6, 5 ),
        };
        static List<KeyValuePair<sbyte, sbyte>> personalityToPoisonLUTDirect = new List<KeyValuePair<sbyte, sbyte>>
        {
            new KeyValuePair<sbyte, sbyte>( 0, 0 ),
            new KeyValuePair<sbyte, sbyte>( 1, 2 ),
            new KeyValuePair<sbyte, sbyte>( 4, 4 ),
        };
        public override void OnEnable(DataContext context)
        {
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }

        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }
            applyPoison(context);
        }


        public void applyPoison(DataContext context)
        {
            var power = base.SkillInstance.GetPower();
            var allyChar = DomainManager.Combat.GetCombatCharacter(base.CombatChar.IsAlly);
            var enemyChar = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly);
            sbyte poisonLevel = 0;
            if (allyChar.GetPersonalityValue(fuyuan) > enemyChar.GetPersonalityValue(fuyuan))
            {
                poisonLevel += 1;
            }
            foreach (var p in IsDirect ? personalityToPoisonLUTDirect : personalityToPoisonLUTIndirect)
            {
                var enemyPersonality = enemyChar.GetPersonalityValue(p.Key);
                var allyPersonality = allyChar.GetPersonalityValue(p.Key);
                var poisonType = p.Value;
                int poisonValue = 600;
                if (allyPersonality > enemyPersonality)
                {
                    DomainManager.Combat.AddPoison(context, base.CombatChar, base.CurrEnemyChar, poisonType, poisonLevel, poisonValue, base.SkillTemplateId, applySpecialEffect: true, canBounce: true, default(ItemKey), isDirectPoison: true);
                }
            }


        }
    }
}
