using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill.Common.Assist;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen
{
    // 正 此功法每段命中：我方的一个杀式转化为敌人随机旧伤。若暴击则不需要消耗杀式。
    // 逆 此功法每段命中：敌方的一个杀式转化为敌人随机旧伤。若暴击则不需要消耗杀式。

    internal class Qimen8 : CombatSkillEffectBase
    {
        private const sbyte ShaTrick = 19;
        public Qimen8()
        {
        }

        public Qimen8(CombatSkillKey skillKey)
            : base(skillKey, 54101)
        {
        }
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            Events.RegisterHandler_AttackSkillAttackHit(OnAttackSkillAttackHit);
        }

        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_AttackSkillAttackHit(OnAttackSkillAttackHit);
            base.OnDisable(context);
        }

        private void OnAttackSkillAttackHit(DataContext context, CombatCharacter attacker, CombatCharacter defender, short skillId, int index, bool critical)
        {
            if (!SkillKey.IsMatch(attacker.GetId(), skillId))
            {
                return;
            }

            CombatCharacter trickChar = base.IsDirect ? base.CombatChar : base.CurrEnemyChar;

            if (!critical)
            {
                bool consumed = DomainManager.Combat.RemoveTrick(context, trickChar, ShaTrick, 1, removedByAlly: base.IsDirect);
                if (!consumed)
                {
                    return;
                }
            }

            bool addInner = context.Random.Next(50) < 100;
            defender.AddRandomInjury(context, addInner, 1, changeToOld: true);
            ShowSpecialEffectTips(0);
        }
    }
}
