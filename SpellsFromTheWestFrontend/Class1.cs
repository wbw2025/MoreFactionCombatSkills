using Config;
using GameData.Domains.Combat;
using GameData.Domains.Mod;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            int langSettings = 0;
            ModManager.GetSetting(base.ModIdStr, "Language", ref langSettings);
            bool dontLoadSelf = false;
            ModManager.GetSetting(base.ModIdStr, "DontLoadSelf", ref dontLoadSelf);
            if (langSettings == 1)
            {
                LanguageKey = 44;
            }
            else
            {
                LanguageKey = 86;
            }

            ModInfoList modInfoList = ModManager.GetLoadedModInfoList();
            ModInfo myModInfo = ModManager.GetModInfo(this.ModIdStr);
            foreach (ModInfo mod in modInfoList.Items)
            {
                if(mod.DirectoryName == myModInfo.DirectoryName && dontLoadSelf)
                {
                    continue;
                }

                string directory = mod.DirectoryName;
                if (File.Exists(Path.Combine(directory, "CombatSkills.yml")) || File.Exists(Path.Combine(directory, "SpecialEffects.yml")) || File.Exists(Path.Combine(directory, "SkillBooks.yml")))
                {
                    try
                    {
                        DataConfigAppender.LoadCombatSkillsFromYamlFile(Path.Combine(directory, "CombatSkills.yml"));
                        DataConfigAppender.LoadSpecialEffectsFromYamlFile(Path.Combine(directory, "SpecialEffects.yml"));
                        DataConfigAppender.LoadSkillBooksFromYamlFile(Path.Combine(directory, "SkillBooks.yml"));
                    }
                    catch (Exception ex)
                    {
                        AdaptableLog.Info($"Failed to load YAML files from mod directory {directory}: {ex.Message}");
                    }
                }
            }

            DumpConfigToCsv(CombatSkill.Instance, @".\CombatSkills.csv");
            DumpConfigToCsv(SpecialEffect.Instance, @".\SpecialEffects.csv");
            DumpConfigToCsv(SkillBook.Instance, @".\SkillBooks.csv");
            AdaptableLog.Info($"Dumping game data. Dump Directory {Directory.GetCurrentDirectory()}");

            AdaptableLog.Info($"SpellsFromTheWest Frontend initialized. LanguageKey: {LanguageKey}.");


        }

        private static void DumpConfigToCsv<T>(IEnumerable<T> items, string filePath)
        {
            if (items == null) return;

            var list = items.ToList();
            if (list.Count == 0) return;

            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.IsStatic)
                .ToArray();

            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", fields.Select(f => EscapeCsvField(f.Name))));

            foreach (var item in list)
            {
                var values = fields.Select(f =>
                {
                    var val = f.GetValue(item);
                    return EscapeCsvField(FormatValue(val));
                });
                sb.AppendLine(string.Join(",", values));
            }

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                AdaptableLog.Info($"Dumped {list.Count} {typeof(T).Name} items to {filePath}");
            }
            catch (Exception ex)
            {
                AdaptableLog.Error($"Failed to write to {filePath}: {ex.Message}");
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            return field;
        }

        private static string FormatValue(object val)
        {
            if (val == null) return "";
            if (val is string s) return s;
            if (val is IEnumerable enumerable)
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                    items.Add(FormatValue(item));
                return "[" + string.Join("; ", items) + "]";
            }
            return val.ToString();
        }
    }
}
