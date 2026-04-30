using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa1 : CombatSkillEffectBase
    {
        public static readonly short Jianfa1TId = 4117;

        public Jianfa1()
        {
        }

        public Jianfa1(CombatSkillKey skillKey)
            : base(skillKey, Jianfa1TId)
        {
        }

    }
}
