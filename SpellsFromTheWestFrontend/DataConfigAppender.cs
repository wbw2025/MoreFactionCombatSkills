using Config;
using GameData.Utilities;
using GameData.Domains.Item;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using System.Text;
using System.Linq;

namespace FeaturesBoundToFuyu
{
    internal class DataConfigAppender
    {
        internal static Dictionary<int, Dictionary<string, object>> ParseYamlTopLevelObjectsForTesting(string yaml)
        {
            return ParseYamlTopLevelObjects(yaml);
        }

        internal static void ApplyChangesForTesting<TItem>(TItem configItem, Dictionary<string, object> changes, int languageKey, params string[] ignoredKeys)
        {
            ApplyChanges(configItem, changes, languageKey, ignoredKeys);
        }

        public static void LoadSpecialEffectsFromYamlFile(string yamlPath)
        {
            AdaptableLog.Info($"Loading special effect items from YAML file: {yamlPath}");
            if (string.IsNullOrWhiteSpace(yamlPath))
                throw new ArgumentOutOfRangeException("yamlPath is empty.", nameof(yamlPath));

            if (!File.Exists(yamlPath))
            {
                AdaptableLog.Info($"YAML file not found: {yamlPath}");
                return;
            }

            string yaml = File.ReadAllText(yamlPath, Encoding.UTF8);
            var items = ParseYamlTopLevelObjects(yaml);

            foreach (var pair in items)
            {
                int newTemplateId = pair.Key;
                Dictionary<string, object> yamlItem = pair.Value;

                if (!TryGetValueIgnoreCase(yamlItem, "TemplateId", out object sourceTemplateValue))
                    throw new ArgumentOutOfRangeException($"YAML item {newTemplateId} must include TemplateId (the source id to copy from).");

                int sourceTemplateId = Convert.ToInt32(sourceTemplateValue, CultureInfo.InvariantCulture);

                var changes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in yamlItem)
                {
                    if (string.Equals(field.Key, "TemplateId", StringComparison.OrdinalIgnoreCase))
                        continue;

                    changes[field.Key] = ResolveBangReference(field.Key, field.Value, SpecialEffect.Instance);
                }

                changes["NewTemplateId"] = newTemplateId;
                CreateAndAppendSpecialEffectItemFromStringsNew(sourceTemplateId, changes);
            }

            AdaptableLog.Info($"Loaded {items.Count} special effect item(s) from {yamlPath}");
        }

        public static void LoadCombatSkillsFromYamlFile(string yamlPath)
        {
            if (string.IsNullOrWhiteSpace(yamlPath))
                throw new ArgumentOutOfRangeException("yamlPath is empty.", nameof(yamlPath));

            if (!File.Exists(yamlPath))
            {
                AdaptableLog.Info($"YAML file not found: {yamlPath}");
                return;
            }

            string yaml = File.ReadAllText(yamlPath, Encoding.UTF8);
            var items = ParseYamlTopLevelObjects(yaml);

            foreach (var pair in items)
            {
                int newTemplateId = pair.Key;
                Dictionary<string, object> yamlItem = pair.Value;

                if (!TryGetValueIgnoreCase(yamlItem, "TemplateId", out object sourceTemplateValue))
                    throw new ArgumentOutOfRangeException($"YAML item {newTemplateId} must include TemplateId (the source id to copy from).");

                int sourceTemplateId = Convert.ToInt32(sourceTemplateValue, CultureInfo.InvariantCulture);

                var changes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in yamlItem)
                {
                    if (string.Equals(field.Key, "TemplateId", StringComparison.OrdinalIgnoreCase))
                        continue;

                    changes[field.Key] = ResolveBangReference(field.Key, field.Value, CombatSkill.Instance);
                }

                changes["NewTemplateId"] = newTemplateId;
                CreateAndAppendCombatSkillItemFromStringsNew(sourceTemplateId, changes);
            }

            AdaptableLog.Info($"Loaded {items.Count} special effect item(s) from {yamlPath}");
        }

        public static void LoadSkillBooksFromYamlFile(string yamlPath)
        {
            AdaptableLog.Info($"Loading skill book items from YAML file: {yamlPath}");
            if (string.IsNullOrWhiteSpace(yamlPath))
                throw new ArgumentOutOfRangeException("yamlPath is empty.", nameof(yamlPath));

            if (!File.Exists(yamlPath))
            {
                AdaptableLog.Info($"YAML file not found: {yamlPath}");
                return;
            }

            string yaml = File.ReadAllText(yamlPath, Encoding.UTF8);
            var items = ParseYamlTopLevelObjects(yaml);

            foreach (var pair in items)
            {
                int newTemplateId = pair.Key;
                Dictionary<string, object> yamlItem = pair.Value;

                if (!TryGetValueIgnoreCase(yamlItem, "TemplateId", out object sourceTemplateValue))
                    throw new ArgumentOutOfRangeException($"YAML item {newTemplateId} must include TemplateId (the source id to copy from).");

                int sourceTemplateId = Convert.ToInt32(sourceTemplateValue, CultureInfo.InvariantCulture);

                var changes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in yamlItem)
                {
                    if (string.Equals(field.Key, "TemplateId", StringComparison.OrdinalIgnoreCase))
                        continue;

                    changes[field.Key] = ResolveBangReference(field.Key, field.Value, SkillBook.Instance);
                }

                changes["NewTemplateId"] = newTemplateId;
                CreateAndAppendSkillBookItemFromStringsNew(sourceTemplateId, changes);
            }

            AdaptableLog.Info($"Loaded {items.Count} skill book item(s) from {yamlPath}");
        }

        public static void CreateAndAppendSpecialEffectItemFromStringsNew(int templateID, Dictionary<string, object> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            int targetTemplateId = GetRequiredNewTemplateId(changes);

            SpecialEffectItem sourceItem = SpecialEffect.Instance[(short)templateID];
            if (sourceItem == null)
                throw new InvalidOperationException($"Source SpecialEffectItem {templateID} does not exist.");

            SpecialEffectItem copiedItem = sourceItem.Duplicate(targetTemplateId);
            ApplyChanges(copiedItem, changes, "TemplateId", "NewTemplateId");
            SaveSpecialEffectItem(copiedItem);
        }

        public static void CreateAndAppendCombatSkillItemFromStringsNew(int templateID, Dictionary<string, object> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            int targetTemplateId = GetRequiredNewTemplateId(changes);

            CombatSkillItem sourceItem = CombatSkill.Instance[(short)templateID];
            if (sourceItem == null)
                throw new InvalidOperationException($"Source CombatSkillItem {templateID} does not exist.");

            CombatSkillItem copiedItem = sourceItem.Duplicate(targetTemplateId);
            ApplyChanges(copiedItem, changes, "TemplateId", "NewTemplateId");
            SaveCombatSkillItem(copiedItem);
        }

        public static void CreateAndAppendSkillBookItemFromStringsNew(int templateID, Dictionary<string, object> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            int targetTemplateId = GetRequiredNewTemplateId(changes);

            SkillBookItem sourceItem = SkillBook.Instance[(short)templateID];
            if (sourceItem == null)
                throw new InvalidOperationException($"Source SkillBookItem {templateID} does not exist.");

            SkillBookItem copiedItem = sourceItem.Duplicate(targetTemplateId);
            ApplyChanges(copiedItem, changes, "TemplateId", "NewTemplateId");
            SaveSkillBookItem(copiedItem);
        }

        private static void SaveCombatSkillItem(CombatSkillItem item)
        {
            string refName = BuildRefName(item.Name, item.TemplateId);
            DataConfigAppenderHelpers.AddCombatSkillItemToConfig(item.TemplateId.ToString(), refName, item);
        }

        private static void SaveSpecialEffectItem(SpecialEffectItem item)
        {
            string refName = BuildRefName(item.Name, item.TemplateId);
            DataConfigAppenderHelpers.AddSpecialEffectItemToConfig(item.TemplateId.ToString(), refName, item);
        }

        private static void SaveSkillBookItem(SkillBookItem item)
        {
            string refName = BuildRefName(item.Name, item.TemplateId);
            DataConfigAppenderHelpers.AddSkillBookToConfig(item.TemplateId.ToString(), refName, item);
        }

        private static string BuildRefName(string name, int templateId)
        {
            string safeName = string.IsNullOrWhiteSpace(name) ? "CustomConfigItem" : name;
            return $"{safeName}_{templateId}";
        }

        private static int GetRequiredNewTemplateId(Dictionary<string, object> changes)
        {
            if (TryGetValueIgnoreCase(changes, "NewTemplateId", out object newTemplateId))
                return Convert.ToInt32(newTemplateId);

            if (TryGetValueIgnoreCase(changes, "TemplateId", out object templateId))
                return Convert.ToInt32(templateId);

            throw new ArgumentOutOfRangeException("changes must contain NewTemplateId or TemplateId for the new modded item.");
        }

        private static object ResolveBangReference(string key, object value, object configInstance)
        {
            string text = value as string;
            if (string.IsNullOrEmpty(text) || !text.StartsWith("!!!"))
                return value;

            string numberPart = text.Substring(3);
            if (!short.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out short templateId))
                return value;

            Type configType = configInstance.GetType();
            PropertyInfo indexer = configType.GetProperty("Item", new[] { typeof(short) });
            if (indexer == null)
                return value;

            object referencedItem;
            try
            {
                referencedItem = indexer.GetValue(configInstance, new object[] { templateId });
            }
            catch
            {
                return value;
            }

            if (referencedItem == null)
                return value;

            Type itemType = referencedItem.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

            FieldInfo targetField = itemType.GetField(key, flags);
            if (targetField != null)
                return targetField.GetValue(referencedItem);

            PropertyInfo targetProperty = itemType.GetProperty(key, flags);
            if (targetProperty != null && targetProperty.CanRead)
                return targetProperty.GetValue(referencedItem);

            int prefixLength = 0;
            while (prefixLength < key.Length && char.IsDigit(key[prefixLength]))
                prefixLength++;

            if (prefixLength > 0 && prefixLength < key.Length)
            {
                string strippedKey = key.Substring(prefixLength);
                targetField = itemType.GetField(strippedKey, flags);
                if (targetField != null)
                    return targetField.GetValue(referencedItem);

                targetProperty = itemType.GetProperty(strippedKey, flags);
                if (targetProperty != null && targetProperty.CanRead)
                    return targetProperty.GetValue(referencedItem);
            }

            return value;
        }

        private static bool TryGetValueIgnoreCase(Dictionary<string, object> changes, string key, out object value)
        {
            foreach (var pair in changes)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private sealed class PendingChange
        {
            public PendingChange(string rawKey, string memberName, object value)
            {
                RawKey = rawKey;
                MemberName = memberName;
                Value = value;
            }

            public string RawKey { get; }

            public string MemberName { get; }

            public object Value { get; }
        }

        private sealed class PendingLocalizedChangeSet
        {
            public PendingChange BaseChange { get; set; }

            public Dictionary<int, PendingChange> LocalizedChanges { get; } = new Dictionary<int, PendingChange>();
        }

        private static Dictionary<string, PendingLocalizedChangeSet> BuildPendingChanges(Dictionary<string, object> changes, HashSet<string> ignoredKeySet)
        {
            var pendingChanges = new Dictionary<string, PendingLocalizedChangeSet>(StringComparer.OrdinalIgnoreCase);

            foreach (var change in changes)
            {
                if (ignoredKeySet.Contains(change.Key))
                    continue;

                string memberName = change.Key;
                bool isLocalized = TryParseLocalizedMemberKey(change.Key, out int languageKey, out memberName);

                if (!pendingChanges.TryGetValue(memberName, out PendingLocalizedChangeSet changeSet))
                {
                    changeSet = new PendingLocalizedChangeSet();
                    pendingChanges[memberName] = changeSet;

                }

                var pendingChange = new PendingChange(change.Key, memberName, change.Value);
                if (isLocalized)
                {
                    changeSet.LocalizedChanges[languageKey] = pendingChange;
                    continue;
                }


                changeSet.BaseChange = pendingChange;
            }

            return pendingChanges;
        }

        private static bool TryParseLocalizedMemberKey(string rawKey, out int languageKey, out string memberName)
        {
            languageKey = 0;
            memberName = rawKey;
            if (string.IsNullOrEmpty(rawKey))
                return false;

            int prefixLength = 0;
            while (prefixLength < rawKey.Length && char.IsDigit(rawKey[prefixLength]))
            {
                prefixLength++;
            }

            if (prefixLength == 0 || prefixLength == rawKey.Length)
                return false;

            if (!int.TryParse(rawKey.Substring(0, prefixLength), NumberStyles.None, CultureInfo.InvariantCulture, out languageKey))
                return false;

            memberName = rawKey.Substring(prefixLength);
            return !string.IsNullOrWhiteSpace(memberName);
        }

        private static PendingChange SelectPendingChange(PendingLocalizedChangeSet changeSet, int languageKey)
        {
            if (changeSet.LocalizedChanges.TryGetValue(languageKey, out PendingChange localizedChange))
                return localizedChange;

            return changeSet.BaseChange;
        }

        private static string GetUnknownChangeKey(PendingLocalizedChangeSet changeSet, int languageKey)
        {
            if (changeSet.LocalizedChanges.TryGetValue(languageKey, out PendingChange localizedChange))
                return localizedChange.RawKey;

            if (changeSet.BaseChange != null)
                return changeSet.BaseChange.RawKey;

            foreach (var pair in changeSet.LocalizedChanges)
            {
                return pair.Value.RawKey;
            }

            return string.Empty;
        }

        private static void ApplyChanges<TItem>(TItem configItem, Dictionary<string, object> changes, params string[] ignoredKeys)
        {
            ApplyChanges(configItem, changes, SpellsFromTheWestFrontendPlugin.LanguageKey, ignoredKeys);
        }

        private static void ApplyChanges<TItem>(TItem configItem, Dictionary<string, object> changes, int languageKey, params string[] ignoredKeys)
        {
            var ignoredKeySet = new HashSet<string>(ignoredKeys, StringComparer.OrdinalIgnoreCase);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
            Type targetType = configItem.GetType();
            Dictionary<string, PendingLocalizedChangeSet> pendingChanges = BuildPendingChanges(changes, ignoredKeySet);

            foreach (var pair in pendingChanges)
            {
                string memberName = pair.Key;
                PendingLocalizedChangeSet changeSet = pair.Value;

                FieldInfo targetField = targetType.GetField(memberName, flags);
                PropertyInfo targetProperty = targetType.GetProperty(memberName, flags);

                if (targetField == null && (targetProperty == null || !targetProperty.CanWrite))
                {
                    AdaptableLog.Info($"Applying for {changes["NewTemplateId"]}: Unknown config field/property: {GetUnknownChangeKey(changeSet, languageKey)}");
                    continue;
                }

                PendingChange change = SelectPendingChange(changeSet, languageKey);
                if (change == null)
                    continue;

                if (targetField != null)
                {
                    object convertedValue = ConvertChangeValue(change.Value, targetField.FieldType);
                    targetField.SetValue(configItem, convertedValue);
                    continue;
                }

                if (targetProperty != null && targetProperty.CanWrite)
                {
                    object convertedValue = ConvertChangeValue(change.Value, targetProperty.PropertyType);
                    targetProperty.SetValue(configItem, convertedValue);
                    continue;
                }
            }
        }

        private static object ConvertChangeValue(object value, Type targetType)
        {
            if (value == null)
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                    return null;

                return Activator.CreateInstance(targetType);
            }

            Type sourceType = value.GetType();
            if (targetType.IsAssignableFrom(sourceType))
                return value;

            if (value is List<object> yamlList)
                return ConvertYamlListValue(yamlList, targetType);

            Type nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
                targetType = nullableType;

            if (targetType.IsEnum)
            {
                if (value is string enumText)
                    return Enum.Parse(targetType, enumText, true);

                object enumValue = Convert.ChangeType(value, Enum.GetUnderlyingType(targetType));
                return Enum.ToObject(targetType, enumValue);
            }

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(object))
                return value;

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static object ConvertYamlListValue(List<object> list, Type targetType)
        {
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();
                Array array = Array.CreateInstance(elementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    object convertedElement = ConvertChangeValue(list[i], elementType);
                    array.SetValue(convertedElement, i);
                }

                return array;
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];
                IList typedList = (IList)Activator.CreateInstance(targetType);
                for (int i = 0; i < list.Count; i++)
                {
                    typedList.Add(ConvertChangeValue(list[i], elementType));
                }

                return typedList;
            }

            if (targetType == typeof(object))
            {
                object[] dynamicArray = new object[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    dynamicArray[i] = list[i] is List<object> nested ? ConvertYamlListValue(nested, typeof(object[])) : list[i];
                }

                return dynamicArray;
            }

            ConstructorInfo[] ctors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (ConstructorInfo ctor in ctors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length != 1)
                    continue;

                Type pType = parameters[0].ParameterType;
                if (!pType.IsArray)
                    continue;

                try
                {
                    object convertedArray = ConvertYamlListValue(list, pType);
                    return ctor.Invoke(new object[] { convertedArray });
                }
                catch
                {
                }
            }

            foreach (ConstructorInfo ctor in ctors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length != list.Count)
                    continue;

                object[] args = new object[list.Count];
                bool ok = true;

                for (int i = 0; i < list.Count; i++)
                {
                    try
                    {
                        args[i] = ConvertChangeValue(list[i], parameters[i].ParameterType);
                    }
                    catch
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok)
                    continue;

                try
                {
                    return ctor.Invoke(args);
                }
                catch
                {
                }
            }

            throw new ArgumentOutOfRangeException($"Cannot construct type {targetType.FullName} from YAML list value.");
        }

        private static Dictionary<int, Dictionary<string, object>> ParseYamlTopLevelObjects(string yaml)
        {
            var stream = new YamlStream();
            using (var reader = new StringReader(yaml))
            {
                stream.Load(reader);
            }

            if (stream.Documents.Count == 0)
                return new Dictionary<int, Dictionary<string, object>>();

            if (!(stream.Documents[0].RootNode is YamlMappingNode rootMap))
                throw new FormatException("YAML root must be a mapping of newTemplateId -> object.");

            var result = new Dictionary<int, Dictionary<string, object>>();
            foreach (var pair in rootMap.Children)
            {
                object keyValue = ParseYamlNode(pair.Key);
                int newTemplateId = Convert.ToInt32(keyValue, CultureInfo.InvariantCulture);

                if (!(pair.Value is YamlMappingNode itemMap))
                    throw new FormatException($"YAML item {newTemplateId} must be a mapping object.");

                var item = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in itemMap.Children)
                {
                    if (!(field.Key is YamlScalarNode keyNode))
                        throw new FormatException($"YAML item {newTemplateId} contains a non-scalar field key.");

                    string key = keyNode.Value ?? string.Empty;
                    item[key] = ParseYamlNode(field.Value);
                }

                result[newTemplateId] = item;
            }

            return result;
        }

        private static object ParseYamlNode(YamlNode node)
        {
            if (node is YamlScalarNode scalar)
                return ParseYamlScalar(scalar);

            if (node is YamlSequenceNode sequence)
            {
                var list = new List<object>();
                foreach (YamlNode child in sequence.Children)
                {
                    list.Add(ParseYamlNode(child));
                }

                return list;
            }

            if (node is YamlMappingNode map)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in map.Children)
                {
                    if (!(pair.Key is YamlScalarNode keyNode))
                        throw new FormatException("YAML mapping key must be scalar.");

                    dict[keyNode.Value ?? string.Empty] = ParseYamlNode(pair.Value);
                }

                return dict;
            }

            throw new FormatException($"Unsupported YAML node type: {node.GetType().Name}");
        }

        private static object ParseYamlScalar(YamlScalarNode scalar)
        {
            string text = scalar.Value;
            if (text == null)
                return null;

            if (string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) || text == "~")
                return null;

            if (bool.TryParse(text, out bool boolValue))
                return boolValue;

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                return intValue;

            if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longValue))
                return longValue;

            if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;

            return text;
        }
    }

}
