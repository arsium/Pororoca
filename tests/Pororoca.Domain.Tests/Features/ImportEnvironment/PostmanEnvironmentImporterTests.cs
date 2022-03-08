using Xunit;
using System;
using System.IO;
using System.Text;
using Pororoca.Domain.Features.Entities.Postman;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.ImportEnvironment.PostmanEnvironmentImporter;

namespace Pororoca.Domain.Tests.Features.ImportEnvironment;

public static class PostmanEnvironmentImporterTests
{
    private static readonly Guid testEnvId = Guid.Parse("8b34e2c4-3384-4ebd-996e-24c0e63ee256");
    private const string testEnvName = "TestEnvironment";
    private static readonly DateTimeOffset testEnvExportedDateTime = DateTimeOffset.Parse("2021-04-01T00:57:06.703Z");


    [Fact]
    public static void Should_import_valid_postman_environment_correctly()
    {  
        string json = GetTestEnvironmentFileJson();
        Assert.True(TryImportPostmanEnvironment(json, out PororocaEnvironment? env));
        AssertConvertedEnvironment(env!);
    }

    [Fact]
    public static void Should_convert_valid_postman_environment_correctly()
    {  
        PostmanEnvironment postmanEnvironment = CreateTestPostmanEnvironment();
        Assert.True(TryConvertPostmanEnvironment(postmanEnvironment, out PororocaEnvironment? env));
        AssertConvertedEnvironment(env!);
    }

    [Fact]
    public static void Should_not_convert_invalid_postman_environment()
    {  
        string json = "{\"id\": \"8b34e2c4-3384-4ebd-996e-24c0e63ee256\"}";
        
        Assert.False(TryImportPostmanEnvironment(json, out PororocaEnvironment? env));
        Assert.Null(env);
    }

    private static void AssertConvertedEnvironment(PororocaEnvironment env)
    {
        Assert.NotNull(env);
        Assert.NotEqual(testEnvId, env.Id);
        Assert.Equal(testEnvName, env.Name);
        Assert.Equal(testEnvExportedDateTime, env.CreatedAt);
        Assert.False(env.IsCurrent);
        Assert.Equal(2, env.Variables.Count);

        PororocaVariable var1 = env.Variables[0];
        Assert.True(var1.Enabled);
        Assert.False(var1.IsSecret);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);

        PororocaVariable var2 = env.Variables[1];
        Assert.False(var2.Enabled);
        Assert.False(var2.IsSecret);
        Assert.Equal("Key2", var2.Key);
        Assert.Equal("Value2", var2.Value);
    }

    private static string GetTestEnvironmentFileJson()
    {
        DirectoryInfo testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        string jsonFileInfoPath = Path.Combine(testDataDirInfo.FullName, "TestData", "TestEnvironment.postman_environment.json");
        return File.ReadAllText(jsonFileInfoPath, Encoding.UTF8);
    }

    private static PostmanEnvironment CreateTestPostmanEnvironment() =>
        new()
        {
            Id = testEnvId,
            Name = testEnvName,
            Scope = "environment",
            ExportedAt = testEnvExportedDateTime,
            ExportedUsing = "Postman/8.0.10",
            Values = new PostmanEnvironmentVariable[]
            {
                new()
                {
                    Key = "Key1",
                    Value = "Value1",
                    Enabled = true
                },
                new()
                {
                    Key = "Key2",
                    Value = "Value2",
                    Enabled = false
                }
            }
        };
}