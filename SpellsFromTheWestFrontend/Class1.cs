using GameData.Domains.Combat;
using GameData.Domains.Mod;
using GameData.Utilities;
using HarmonyLib;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;

namespace FeaturesBoundToFuyu
{
    [PluginConfig("SpellsFromTheWestFrontendPlugin", "wilhelm", "1.0")]
    public class SpellsFromTheWestFrontendPlugin : TaiwuRemakePlugin
    {
	    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        Harmony harmony;
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

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
            
            DataConfigAppender.LoadSpecialEffectsFromYamlFile(Path.Combine(directory, "SpecialEffects.yml"));
            DataConfigAppender.LoadCombatSkillsFromYamlFile(Path.Combine(directory, "CombatSkills.yml"));
            

        }

    }
}
