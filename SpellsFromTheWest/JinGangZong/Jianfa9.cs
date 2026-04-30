
using GameData.Combat.Math;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa9 : CombatSkillEffectBase
    {
        public static bool SkillIsFoWang(short skillId)
        {
            return skillId == Jianfa9TId || skillId == Jianfa1.Jianfa1TId;
        }


        public static readonly short Jianfa9TId = 4109;

        // on enable does nothing


        public Jianfa9()
        {
        }

        public Jianfa9(CombatSkillKey skillKey)
            : base(skillKey, Jianfa9TId)
        {
        }



    }
}
