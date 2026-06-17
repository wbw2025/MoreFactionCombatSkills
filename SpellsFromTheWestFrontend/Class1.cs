using GameData.Domains.Combat;
using GameData.Domains.Mod;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;
using YamlDotNet.Core.Tokens;

namespace FeaturesBoundToFuyu
{
    [PluginConfig("SpellsFromTheWestFrontendPlugin", "wilhelm", "1.0")]
    public class SpellsFromTheWestFrontendPlugin : TaiwuRemakePlugin
    {

        Harmony harmony;
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
        public static int LanguageKey { get; private set; }

        public override void Initialize()
        {
            AdaptableLog.Info($"Load SpellsFromTheWest Frontend. Current Directory {Directory.GetCurrentDirectory()}");
            harmony = Harmony.CreateAndPatchAll(typeof(SpellsFromTheWestFrontendPlugin));

            /*Thread.Sleep(500);
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }*/
            // removed due to bug
            //AddCharacterFeature.DoAdd();
            ModInfo modInfo = ModManager.GetModInfo(this.ModIdStr);
            string directory = modInfo.DirectoryName;

            int langSettings = 0;
            ModManager.GetSetting(base.ModIdStr, "Language", ref langSettings);
            if (langSettings == 1)
            {
                LanguageKey = 44;
            }
            else
            {
                LanguageKey = 86;
            }
            DataConfigAppender.LoadCombatSkillsFromYamlFile(Path.Combine(directory, "CombatSkills.yml"));
            DataConfigAppender.LoadSpecialEffectsFromYamlFile(Path.Combine(directory, "SpecialEffects.yml"));

            AdaptableLog.Info($"SpellsFromTheWest Frontend initialized. LanguageKey: {LanguageKey}.");


        }
    }
}
