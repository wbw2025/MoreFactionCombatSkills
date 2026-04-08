using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong
{
    internal class Jianfa8 : CombatSkillEffectBase
    {
        // 正 该功法增加20%威力。发挥五成威力时，在本次战斗中将此增益增加到明王之剑上。
        // 逆 该功法增加50%威力。发挥五成威力时，在本次战斗中将此增益增加到明王之剑上。该功法造成的20%伤害会反震到运用者身上。
        public Jianfa8(CombatSkillKey skillKey) : base(skillKey, 4109)
        {
        }
    }


}
