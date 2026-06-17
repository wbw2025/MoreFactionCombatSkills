using System.Collections.Generic;
using Xunit;

namespace FeaturesBoundToFuyu.Tests;

public class DataConfigAppenderLocalizationTests
{
    [Fact]
    public void ApplyChangesForTesting_UsesLocalizedFields_WhenLanguageMatches()
    {
        var yaml = """
            4122:
              TemplateId: 473
              Name: "Base name"
              44Name: "Localized name"
              Desc: "Base desc"
              44Desc: "Localized desc"
              Grade: 4
            """;

        var parsedItem = ParseSingleYamlItem(yaml);
        var item = new TestConfigItem
        {
            Name = "Source name",
            Desc = "Source desc",
            Grade = 1
        };

        DataConfigAppender.ApplyChangesForTesting(item, parsedItem, 44, "TemplateId", "NewTemplateId");

        Assert.Equal("Localized name", item.Name);
        Assert.Equal("Localized desc", item.Desc);
        Assert.Equal(4, item.Grade);
    }

    [Fact]
    public void ApplyChangesForTesting_FallsBackToBaseFields_WhenLanguageDoesNotMatch()
    {
        var yaml = """
            4122:
              TemplateId: 473
              Name: "Base name"
              44Name: "Localized name"
              Desc: "Base desc"
              44Desc: "Localized desc"
            """;

        var parsedItem = ParseSingleYamlItem(yaml);
        var item = new TestConfigItem
        {
            Name = "Source name",
            Desc = "Source desc"
        };

        DataConfigAppender.ApplyChangesForTesting(item, parsedItem, 55, "TemplateId", "NewTemplateId");

        Assert.Equal("Base name", item.Name);
        Assert.Equal("Base desc", item.Desc);
    }

    [Fact]
    public void ApplyChangesForTesting_LeavesOriginalValue_WhenOnlyOtherLanguageFieldExists()
    {
        var yaml = """
            4122:
              TemplateId: 473
              44Name: "Localized name"
              44Desc: "Localized desc"
            """;

        var parsedItem = ParseSingleYamlItem(yaml);
        var item = new TestConfigItem
        {
            Name = "Source name",
            Desc = "Source desc"
        };

        DataConfigAppender.ApplyChangesForTesting(item, parsedItem, 55, "TemplateId", "NewTemplateId");

        Assert.Equal("Source name", item.Name);
        Assert.Equal("Source desc", item.Desc);
    }

    [Fact]
    public void ParseYamlTopLevelObjectsForTesting_PreservesPrefixedLocalizationKeys()
    {
        var yaml = """
            4122:
              TemplateId: 473
              44Name: "Localized name"
              Name: "Base name"
            """;

        Dictionary<string, object> parsedItem = ParseSingleYamlItem(yaml);

        Assert.Equal("Localized name", parsedItem["44Name"]);
        Assert.Equal("Base name", parsedItem["Name"]);
    }

    private static Dictionary<string, object> ParseSingleYamlItem(string yaml)
    {
        var parsed = DataConfigAppender.ParseYamlTopLevelObjectsForTesting(yaml);
        var item = Assert.Single(parsed);
        return item.Value;
    }

    private sealed class TestConfigItem
    {
        public string Name { get; set; } = string.Empty;

        public string Desc { get; set; } = string.Empty;

        public int Grade { get; set; }
    }
}