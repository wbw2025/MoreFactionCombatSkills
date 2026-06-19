using Config;
using GameData.Combat.Math;

using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Linq;
using static GameData.DomainEvents.Events;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen

{
    internal class Qimen3 : CombatSkillEffectBase
    {
        // Constants
        private const int HealthCostPerSha = 4;
        private const int AbsorbBreathStancePercentDirect = 25;
        private const int AbsorbBreathStancePercentInDirect = 20;
        private const sbyte ShaTrick = 19;

        public Qimen3()
        {
        }

        public Qimen3(CombatSkillKey skillKey)
            : base(skillKey, 54106)
        {
        }

        // 正：发挥十成威力时：每有一个杀式，消耗4+1%健康，吸取敌人提气和架势，等同于25%释放该功法时的消耗的提气和架势。
        // 逆：发挥十成威力时：敌人每有一个杀式，我方消耗4+1%健康，吸取敌人提气和架势，等同于20%释放该功法的消耗的提气和架势。
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            Events.RegisterHandler_CastSkillEnd(OnCastSkillEnd);
        }
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastSkillEnd(OnCastSkillEnd);
            base.OnDisable(context);
        }

        private int CountShaTricks(CombatCharacter combatChar)
        {
            var tricks = combatChar.GetTricks();
            return tricks.Tricks.Sum((p) => (p.Value == ShaTrick ? 1 : 0));
        }

        private unsafe void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            if (charId != base.CharacterId || skillId != base.SkillTemplateId)
            {
                return;
            }

            if (!PowerMatchAffectRequire(power))
            {
                return;
            }

            // Determine which char's sha count to use
            int shaCount = base.IsDirect ? CountShaTricks(base.CombatChar) : CountShaTricks(base.CurrEnemyChar);

            // Get skill cost
            GameData.Domains.CombatSkill.CombatSkill skill = DomainManager.CombatSkill.GetElement_CombatSkills(SkillKey);
            OuterAndInnerInts costBreathStance = DomainManager.Combat.GetSkillCostBreathStance(base.CharacterId, skill);
            double costStance = costBreathStance.Outer;
            double costBreath = costBreathStance.Inner;

            var AbsorbBreathStancePercent = IsDirect ? AbsorbBreathStancePercentDirect : AbsorbBreathStancePercentInDirect;

            double absorbBreathPerSha = costBreath * AbsorbBreathStancePercent / 100.0;
            double absorbStancePerSha = costStance * AbsorbBreathStancePercent / 100.0;
            double totalAbsorbBreath = absorbBreathPerSha * shaCount;
            double totalAbsorbStance = absorbStancePerSha * shaCount;

            // Apply health consumption on self
            GameData.Domains.Character.Character selfChar = base.CombatChar.GetCharacter();
            int totalHealthCost = HealthCostPerSha * shaCount + selfChar.GetHealth() / 100;

            selfChar.ChangeHealth(context, -totalHealthCost);

            // Absorb breath/stance from enemy
            if (totalAbsorbBreath > 0)
            {
                AbsorbBreathValue(context, base.CurrEnemyChar, (CValuePercent)totalAbsorbBreath);
            }
            if (totalAbsorbStance > 0)
            {
                AbsorbStanceValue(context, base.CurrEnemyChar, (CValuePercent)totalAbsorbStance);
            }

            ShowSpecialEffectTips(0);
        }
    }
}
