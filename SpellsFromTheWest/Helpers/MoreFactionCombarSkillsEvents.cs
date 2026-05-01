using GameData.Common;
using GameData.Domains.Combat;
using GameData.Utilities;
using System;

namespace MoreFactionCombatSkillsBackend.Helpers
{
    public delegate void OnCastSkillFree(DataContext context, CombatCharacter character, short skillId, ECombatCastFreePriority priority);

    internal static class MoreFactionCombarSkillsEvents
    {
        private static OnCastSkillFree _handlersCastSkillFree;

        public static void RegisterHandler_CastSkillFree(OnCastSkillFree handler)
        {
            _handlersCastSkillFree = (OnCastSkillFree)Delegate.Combine(_handlersCastSkillFree, handler);
        }

        public static void UnRegisterHandler_CastSkillFree(OnCastSkillFree handler)
        {
            _handlersCastSkillFree = (OnCastSkillFree)Delegate.Remove(_handlersCastSkillFree, handler);
        }

        public static void RaiseCastSkillFree(DataContext context, CombatCharacter character, short skillId, ECombatCastFreePriority priority)
        {
            _handlersCastSkillFree?.Invoke(context, character, skillId, priority);
        }
    }
}
