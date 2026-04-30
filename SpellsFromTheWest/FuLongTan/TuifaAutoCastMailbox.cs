using System.Collections.Generic;

namespace GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan
{
    internal static class TuifaAutoCastMailbox
    {
        private static readonly Dictionary<int, int> _forceAutoCastTokens = new Dictionary<int, int>();

        public static void NotifyForceAutoCast(int charId)
        {
            if (charId <= 0)
            {
                return;
            }

            _forceAutoCastTokens.TryGetValue(charId, out int token);
            _forceAutoCastTokens[charId] = token + 1;
        }

        public static int GetToken(int charId)
        {
            if (charId <= 0)
            {
                return 0;
            }

            return _forceAutoCastTokens.TryGetValue(charId, out int token) ? token : 0;
        }
    }
}