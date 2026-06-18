// #define DEBUGGING

using Config.ConfigCells.Character;
using Config;
using GameData.Domains.Item;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GameData.Utilities;
using System.Reflection;

namespace FeaturesBoundToFuyu
{
    internal class DataConfigAppenderHelpers
    {
        private static List<SpecialEffectItem> _specialEffectItems = new List<SpecialEffectItem>();
        public static List<SpecialEffectItem> SpecialEffectItems => _specialEffectItems;
        private static List<CombatSkillItem> _combatSkillItems = new List<CombatSkillItem>();
        public static List<CombatSkillItem> CombatSkillItems => _combatSkillItems;
        private static List<SkillBookItem> _skillBookItems = new List<SkillBookItem>();
        public static List<SkillBookItem> SkillBookItems => _skillBookItems;


        public static void AddSkillBreakGridList(CombatSkillItem Item)
        {
            var Grade = Item.Grade;
            var Type = Item.Type;
            var fieldInfo = typeof(Config.CombatSkill).GetField("_dataArray", BindingFlags.NonPublic | BindingFlags.Instance);
            List<CombatSkillItem> ItemList = (List<CombatSkillItem>)fieldInfo.GetValue(Config.CombatSkill.Instance);
            var fieldInfo1 = typeof(Config.SkillBreakGridList).GetField("_dataArray", BindingFlags.NonPublic | BindingFlags.Instance);
            List<SkillBreakGridListItem> SkillBreakGridListList = (List<SkillBreakGridListItem>)fieldInfo1.GetValue(Config.SkillBreakGridList.Instance);
            CombatSkillItem ReferenceSkill = ItemList.First(item => item.Grade == Grade && item.Type == Type);
            SkillBreakGridListItem ReferenceSkillBreakGridListItem = SkillBreakGridListList.First(item => item.TemplateId == ReferenceSkill.TemplateId);
            SkillBreakGridListItem CopiedGradeList = new SkillBreakGridListItem(Item.TemplateId, ReferenceSkillBreakGridListItem.BreakGridListJust,
                ReferenceSkillBreakGridListItem.BreakGridListKind, ReferenceSkillBreakGridListItem.BreakGridListEven,
                ReferenceSkillBreakGridListItem.BreakGridListRebel, ReferenceSkillBreakGridListItem.BreakGridListEgoistic);
            SkillBreakGridList.Instance.AddExtraItem((Item.TemplateId).ToString(), Item.Name, CopiedGradeList);

        }
        private static void OverwriteInDataArray(System.Type configType, object configInstance, short templateId, object newItem)
        {
            var fieldInfo = configType.GetField("_dataArray", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
                return;

            var list = fieldInfo.GetValue(configInstance) as System.Collections.IList;
            if (list == null || templateId >= list.Count)
                return;

            list[templateId] = newItem;
        }

        public static void AddSpecialEffectItemToConfig(string TemplateId, string Name, object Item)
        {
            short tid = short.Parse(TemplateId);
            if (tid < SpecialEffect.Instance.Count)
            {
                OverwriteInDataArray(typeof(Config.SpecialEffect), SpecialEffect.Instance, tid, Item);
            }
            else
            {
                SpecialEffect.Instance.AddExtraItem(TemplateId, Name, Item);
            }
            _specialEffectItems.Add((SpecialEffectItem)Item);
        }
        public static void AddCombatSkillItemToConfig(string TemplateId, string Name, object Item)
        {
            short tid = short.Parse(TemplateId);
            if (tid < CombatSkill.Instance.Count)
            {
                OverwriteInDataArray(typeof(Config.CombatSkill), CombatSkill.Instance, tid, Item);
            }
            else
            {
                CombatSkill.Instance.AddExtraItem(TemplateId, Name, Item);
                AddSkillBreakGridList((CombatSkillItem)Item);
            }
            _combatSkillItems.Add((CombatSkillItem)Item);
        }
        public static void AddSkillBookToConfig(string TemplateId, string Name, object Item)
        {
            short tid = short.Parse(TemplateId);
            if (tid < SkillBook.Instance.Count)
            {
                OverwriteInDataArray(typeof(Config.SkillBook), SkillBook.Instance, tid, Item);
            }
            else
            {
                SkillBook.Instance.AddExtraItem(TemplateId, Name, Item);
            }
            _skillBookItems.Add((SkillBookItem)Item);
        }
    }
}
