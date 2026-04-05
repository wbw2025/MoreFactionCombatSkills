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

namespace FeaturesBoundToFuyu
{
    internal class DataConfigAppender
    {

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

                    changes[field.Key] = field.Value;
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

                    changes[field.Key] = field.Value;
                }

                changes["NewTemplateId"] = newTemplateId;
                CreateAndAppendCombatSkillItemFromStringsNew(sourceTemplateId, changes);
            }

            AdaptableLog.Info($"Loaded {items.Count} special effect item(s) from {yamlPath}");
        }

        public static void CreateAndAppendSpecialEffectItemFromStringsNew(int templateID, Dictionary<string, object> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            int targetTemplateId = GetRequiredNewTemplateId(changes);

            SpecialEffectItem sourceItem = SpecialEffect.Instance[(short)templateID];
            if (sourceItem == null)
                throw new InvalidOperationException($"Source SpecialEffectItem {templateID} does not exist.");

            EnsureExtraTemplateId(targetTemplateId, SpecialEffect.Instance.Count, nameof(SpecialEffectItem));

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

            EnsureExtraTemplateId(targetTemplateId, CombatSkill.Instance.Count, nameof(CombatSkillItem));

            CombatSkillItem copiedItem = sourceItem.Duplicate(targetTemplateId);
            ApplyChanges(copiedItem, changes, "TemplateId", "NewTemplateId");
            SaveCombatSkillItem(copiedItem);
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

        private static void EnsureExtraTemplateId(int templateId, int baseConfigCount, string itemTypeName)
        {
            if (templateId < baseConfigCount)
                throw new ArgumentOutOfRangeException($"{itemTypeName} template id {templateId} is inside the base config range. Use a new id reserved for modded items.");
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

        private static void ApplyChanges<TItem>(TItem configItem, Dictionary<string, object> changes, params string[] ignoredKeys)
        {
            var ignoredKeySet = new HashSet<string>(ignoredKeys, StringComparer.OrdinalIgnoreCase);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
            Type targetType = configItem.GetType();

            foreach (var change in changes)
            {
                if (ignoredKeySet.Contains(change.Key))
                    continue;

                FieldInfo targetField = targetType.GetField(change.Key, flags);
                if (targetField != null)
                {
                    object convertedValue = ConvertChangeValue(change.Value, targetField.FieldType);
                    targetField.SetValue(configItem, convertedValue);
                    continue;
                }

                PropertyInfo targetProperty = targetType.GetProperty(change.Key, flags);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    object convertedValue = ConvertChangeValue(change.Value, targetProperty.PropertyType);
                    targetProperty.SetValue(configItem, convertedValue);
                    continue;
                }

                // throw new ArgumentOutOfRangeException($"Unknown config field/property: {change.Key}");
                AdaptableLog.Info($"Applying for {changes["NewTemplateId"]}: Unknown config field/property: {change.Key}");
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
