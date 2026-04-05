using GameData.Domains.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreFactionCombatSkillsBackend.Helpers
{
    internal class Helpers
    {
        public static int CountTricks(CombatCharacter combatChar, sbyte trick)
        {
            var tricks = combatChar.GetTricks();
            return tricks.Tricks.Sum((p) => (p.Value == trick ? 1 : 0));
        }
    }
}
