using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreFactionCombatSkillsBackend.Helpers
{
    internal class Helpers
    {
        public static void Break()
        {
                        throw new NotImplementedException();
        }
        public static int CountTricks(CombatCharacter combatChar, sbyte trick)
        {
            var tricks = combatChar.GetTricks();
            return tricks.Tricks.Sum((p) => (p.Value == trick ? 1 : 0));
        }

        public static OuterAndInnerInts Multiply(OuterAndInnerInts baseInts, double multiplier)
        {
            return new OuterAndInnerInts
            {
                Outer = (int)(baseInts.Outer * multiplier),
                Inner = (int)(baseInts.Inner * multiplier)
            };
        }

        public static HitOrAvoidInts Multiply(HitOrAvoidInts baseInts, double multiplier)
        {
            return new HitOrAvoidInts
            (
                new int[4]
                {
                    (int)(baseInts[0] * multiplier),
                    (int)(baseInts[1] * multiplier),
                    (int)(baseInts[2] * multiplier),
                    (int)(baseInts[3] * multiplier)
                }
            );
        }
    }
}
