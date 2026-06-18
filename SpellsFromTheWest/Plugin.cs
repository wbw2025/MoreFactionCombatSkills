using Config;
using Config.ConfigCells.Character;
using GameData;
using GameData.ArchiveData;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Combat;
using GameData.Domains.Combat.Ai;
using GameData.Domains.CombatSkill;
using GameData.Domains.Global;
using GameData.Domains.Item;
using GameData.Domains.Mod;
using GameData.Domains.SpecialEffect;
using GameData.Domains.Taiwu;
using GameData.Domains.Taiwu.VillagerRole;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using TaiwuModdingLib.Core.Plugin;

namespace FeaturesBoundToFuyu
{

    [PluginConfig("FeaturesBoundToFuyuPlugin", "wilhelm", "1.0")]
    public class FeaturesBoundToFuyuPlugin : TaiwuRemakePlugin
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
        public static bool LearnAll { get; private set; } = false;

        private string thisModIdStr;
        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(FeaturesBoundToFuyuPlugin));
            thisModIdStr = base.ModIdStr;

            EnsureTypesArePreserved();

            int langSettings = 0;
            DomainManager.Mod.GetSetting(base.ModIdStr, "Language", ref langSettings);
            bool learnAll = false;
            DomainManager.Mod.GetSetting(base.ModIdStr, "LearnAll", ref learnAll);
            LearnAll = learnAll;
            if (langSettings == 1)
            {
                LanguageKey = 44;
            }
            else
            {
                LanguageKey = 86;
            }

            List<ModId> loadedModIds = ModDomain.GetLoadedModIds();
            foreach (ModId modId in loadedModIds)
            {
                string directory = DomainManager.Mod.GetModDirectory(modId.ToString());
                if (File.Exists(Path.Combine(directory, "CombatSkills.yml")) || File.Exists(Path.Combine(directory, "SpecialEffects.yml")) || File.Exists(Path.Combine(directory, "SkillBooks.yml")))
                {
                    try
                    {
                        DataConfigAppender.LoadSpecialEffectsFromYamlFile(Path.Combine(directory, "SpecialEffects.yml"));
                        DataConfigAppender.LoadCombatSkillsFromYamlFile(Path.Combine(directory, "CombatSkills.yml"));
                        DataConfigAppender.LoadSkillBooksFromYamlFile(Path.Combine(directory, "SkillBooks.yml"));
                    }
                    catch (Exception ex)
                    {
                        AdaptableLog.Info($"Failed to load YAML files from mod directory {directory}: {ex.Message}");
                    }
                }
            }

            AdaptableLog.Info($"SpellsFromTheWest Frontend initialized. LanguageKey: {LanguageKey}.");
        }
        public void AdvanceFinishHandler(DataContext context)
        {
            Install();
        }
        private static void EnsureTypesArePreserved()
        {
            // Prevent tree-shaking/trimming from removing types loaded via reflection
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa1);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa2);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa3);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa4);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa5);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa6);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa7);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa8);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.FuLongTan.Tuifa9);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen1);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen2);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen3);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen4);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen5);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen6);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen7);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen8);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JieQingMen.Qimen9);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa1);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa2);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa3);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa4);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa5);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa6);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa7);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa7Depr);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa8);
            _ = typeof(GameData.Domains.SpecialEffect.MoreFactionCombatSkills.JinGangZong.Jianfa9);
        }
        public override void OnEnterNewWorld()
        {
            Events.UnRegisterHandler_AdvanceMonthFinish(AdvanceFinishHandler);
            Events.RegisterHandler_AdvanceMonthFinish(AdvanceFinishHandler);
        }
        public override void OnLoadedArchiveData()
        {
            Events.UnRegisterHandler_AdvanceMonthFinish(AdvanceFinishHandler);
            Events.RegisterHandler_AdvanceMonthFinish(AdvanceFinishHandler);

        }
        private void learn(DataContext context, short TemplateId)
        {
            if (!DomainManager.Taiwu.TryGetElement_CombatSkills(TemplateId, out var skillInfo))
            {
                DomainManager.Taiwu.TaiwuLearnCombatSkill(context, TemplateId, ushort.MaxValue);
            }
            //else
            //{
            //    for (byte i = 0; i < 15; i++)
            //    {
            //        skillInfo.SetBookPageReadingProgress(i, 100);

            //    }
            //}
        }
        private void Install()
        {
            foreach (var skill in DataConfigAppenderHelpers.CombatSkillItems)
            {
                var taiwu = DomainManager.Taiwu.GetTaiwu();
                DataContext context = DataContextManager.GetCurrentThreadDataContext();
                learn(context, skill.TemplateId);
            }
            if (LearnAll)
            {
                for (int i = 0; i < 722; i++)
                {
                    learn(DataContextManager.GetCurrentThreadDataContext(), (short)i);
                }
            
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatSkillDomain), "InitializeOnInitializeGameDataModule")]
        public static bool InitializeOnInitializeGameDataModule_Hijack()
        {
            int capacity = short.MaxValue + 1;
            CombatSkillDomain.EquipAddPropertyDict = new short[capacity][];

            foreach (CombatSkillItem skillCfg in (IEnumerable<CombatSkillItem>)Config.CombatSkill.Instance)
            {
                if (skillCfg == null)
                {
                    continue;
                }

                short skillId = skillCfg.TemplateId;
                if (skillId < 0)
                {
                    continue;
                }

                List<PropertyAndValue> addPropertyList = skillCfg.PropertyAddList;
                if (addPropertyList == null || addPropertyList.Count == 0)
                {
                    continue;
                }

                short[] addValueList = new short[161];
                Array.Clear(addValueList, 0, addValueList.Length);
                foreach (PropertyAndValue addProperty in addPropertyList)
                {
                    if (addProperty.PropertyId >= 0 && addProperty.PropertyId < addValueList.Length)
                    {
                        addValueList[addProperty.PropertyId] = addProperty.Value;
                    }
                }

                CombatSkillDomain.EquipAddPropertyDict[skillId] = addValueList;
            }

            AccessTools.Method(typeof(CombatSkillDomain), "InitializeLearnableCombatSkillTemplateIds")?.Invoke(null, null);
            PlayerCastBossSkills.Initialize();

            return false;
        }


	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpecialEffectDomain), "Add", new Type[]
	{
		typeof(DataContext),
		typeof(int),
		typeof(string)
	})]
    public static bool FixAdd(DataContext context, int charId, string effectName, SpecialEffectDomain __instance, out long __result)
    {
        string fullTypeName = "GameData.Domains.SpecialEffect." + effectName;
        Type specialEffectType = Type.GetType(fullTypeName) ?? AccessTools.TypeByName(fullTypeName);
        if (specialEffectType == null)
        {
            throw new Exception("Cannot find type '" + fullTypeName + "'.");
        }

        SpecialEffectBase effect = (SpecialEffectBase)Activator.CreateInstance(specialEffectType, charId);
        __instance.Add(context, effect);
        __result = effect.Id;
        return false;
    }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpecialEffectDomain), "Add", new Type[]
	{
		typeof(DataContext),
		typeof(int),
		typeof(short),
		typeof(sbyte),
		typeof(sbyte)
	})]
    public static bool FixAdd(DataContext context, int charId, short skillTemplateId, sbyte effectActiveType, sbyte direction, SpecialEffectDomain __instance)
    {
        CombatSkillKey skillKey = new CombatSkillKey(charId, skillTemplateId);
        GameData.Domains.CombatSkill.CombatSkill skill = DomainManager.CombatSkill.GetElement_CombatSkills(skillKey);
        CombatSkillItem skillConfig = Config.CombatSkill.Instance[skillTemplateId];

        if (direction < 0)
        {
            direction = skill.GetDirection();
        }

        short effectTemplateId = (short)(direction switch
        {
            1 => skillConfig.ReverseEffectID,
            0 => skillConfig.DirectEffectID,
            _ => -1
        });

        if (effectTemplateId < 0)
        {
            return false;
        }

        SpecialEffectItem effectConfig = Config.SpecialEffect.Instance[effectTemplateId];
        if (effectConfig.EffectActiveType != effectActiveType || string.IsNullOrEmpty(effectConfig.ClassName))
        {
            return false;
        }

        string fullTypeName = "GameData.Domains.SpecialEffect." + effectConfig.ClassName;
        Type specialEffectType = Type.GetType(fullTypeName) ?? AccessTools.TypeByName(fullTypeName);
        if (specialEffectType == null)
        {
            throw new Exception("Cannot find type '" + fullTypeName + "'.");
        }

        SpecialEffectBase effect = effectActiveType == 3
            ? (SpecialEffectBase)Activator.CreateInstance(specialEffectType, skillKey, direction)
            : (SpecialEffectBase)Activator.CreateInstance(specialEffectType, skillKey);

        __instance.Add(context, effect);

        if (effectActiveType == 3 || effectActiveType == 2 || effectActiveType == 1)
        {
            skill.SetSpecialEffectId(effect.Id, context);
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CombatDomain), "CastSkillFree", new Type[]
    {
        typeof(DataContext),
        typeof(CombatCharacter),
        typeof(short),
        typeof(ECombatCastFreePriority)
    })]
    public static void OnCastSkillFreePatched(DataContext context, CombatCharacter character, short skillId, ECombatCastFreePriority priority)
    {
        MoreFactionCombatSkillsBackend.Helpers.MoreFactionCombarSkillsEvents.RaiseCastSkillFree(context, character, skillId, priority);
    }


        //static void ClampMin(ref short value, short min)
        //{
        //    if (value < min)
        //        value = min;
        //}
        //static int savedData = 0;
        //static bool[] forceSet = new bool[30];
        //private bool isSet(int bitLoc)
        //{
        //    if ((savedData & (2 >> bitLoc)) != 0)
        //    {
        //        return true;
        //    }
        //    return forceSet[bitLoc];
        //}
        //public override void OnLoadedArchiveData()
        //{
        //    DataContext context = DataContextManager.GetCurrentThreadDataContext();
        //    base.OnEnterNewWorld();
        //    if (!DomainManager.Mod.TryGet(FeaturesBoundToFuyuPlugin.GetModIdStr(), "ProtaFeatures", true, out savedData))
        //    {

        //    }

        //    // main attributes
        //    var taiwu = DomainManager.Taiwu.GetTaiwu();
        //    var taiwuMainAttr = taiwu.GetBaseMainAttributes();
        //    for (sbyte i = 0; i < 6; i++)
        //    {
        //        if (isSet(i))
        //        {
        //            ClampMin(ref taiwuMainAttr[i], (short)70);
        //        }
        //    }
        //    taiwu.SetBaseMainAttributes(taiwuMainAttr, context);

        //    // features
        //    var featureIds = taiwu.GetFeatureIds();
        //    List<Tuple<int, short>> features = new List<Tuple<int, short>>()
        //    {
        //        new Tuple<int, short>(7, (short)203), // 梦境中人 203
        //        new Tuple<int, short>(8, (short)203), // 蛇 256
        //        new Tuple<int, short>(9, (short)203), // 玉 335
        //        new Tuple<int, short>(24, (short)201), // 璞玉韬光 201
        //        new Tuple<int, short>(25, (short)202), // 神锋敛彩 202 
        //    };
        //    foreach (var feature in features)
        //    {
        //        if (isSet(feature.Item1))
        //        {
        //            if (!featureIds.Contains(feature.Item2))
        //            {
        //                taiwu.AddFeature(context, feature.Item2);
        //            }
        //        }
        //    }

        //    // combat skill attributes
        //    // TODO yifu attributes, need to hook dialogs
        //    if (isSet(28))
        //    {
        //        var combatQuali = taiwu.GetCombatSkillQualifications();
        //        for (int i = 0; i < 3; i++)
        //        {
        //            ClampMin(ref combatQuali[i], 90);
        //        }
        //    }

        //    // life skill attributes
        //    var lifeQuali = taiwu.GetLifeSkillQualifications();
        //    List<Tuple<int, List<short>>> lifeAttribs = new List<Tuple<int, List<short>>>()
        //    {
        //        new Tuple<int, List<short>>(29, new List<short>() { 0, 1, 2, 3 }), // 琴棋书画
        //        new Tuple<int, List<short>>(30, new List<short>() { 13, 12, 5, 14 }), // 一任自然
        //        new Tuple<int, List<short>>(31, new List<short>() { 8, 9, 4, 15 }), // 意向性卜
        //        new Tuple<int, List<short>>(32, new List<short>() { 6, 7, 11, 10 }), // 天工开物
        //    };
        //    foreach (var feature in lifeAttribs)
        //    {
        //        if (isSet(feature.Item1))
        //        {
        //            foreach (var attrib in feature.Item2)
        //            {
        //                ClampMin(ref lifeQuali[feature.Item2[attrib]], 70);
        //            }
        //        }
        //    }

        //}

        //static string thisModIdStr;
        //public static string GetModIdStr()
        //{
        //    return thisModIdStr;
        //}


        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(GameData.Domains.Character.Character), "OfflineCreateProtagonist")]
        //public static void InitializeOnInitializeGameDataModule_Post(short templateId, short orgMemberId, ProtagonistCreationInfo info, DataContext context)
        //{
        //    int result = 0;
        //    foreach (short featId in info.ProtagonistFeatureIds)
        //    {
        //        result &= 2 >> featId;
        //    }
        //    DomainManager.Mod.SetInt(context, FeaturesBoundToFuyuPlugin.GetModIdStr(), "ProtaFeatures", true, result);
        //}
    }
}
