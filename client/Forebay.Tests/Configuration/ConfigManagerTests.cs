using Forebay.Core.Configuration;
using Xunit;

namespace Forebay.Tests.Configuration;

public class ConfigManagerTests : IDisposable
{
    private readonly string _testConfigDir;
    private readonly string _originalConfigFile;

    public ConfigManagerTests()
    {
        // Create a temporary test directory
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"forebay-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testConfigDir);

        // Save original config file path for cleanup
        _originalConfigFile = ConfigManager.GetConfigFilePath();
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }
    }

    [Fact]
    public void GetConfigDirectory_Returns_ValidPath()
    {
        var dir = ConfigManager.GetConfigDirectory();

        Assert.NotNull(dir);
        Assert.NotEmpty(dir);
        Assert.Contains("forebay", dir.ToLower());
    }

    [Fact]
    public void GetConfigFilePath_Returns_ValidPath()
    {
        var path = ConfigManager.GetConfigFilePath();

        Assert.NotNull(path);
        Assert.NotEmpty(path);
        Assert.EndsWith("config.toml", path);
    }

    [Fact]
    public void Load_Returns_Null_When_File_Does_Not_Exist()
    {
        // Ensure config file doesn't exist
        if (ConfigManager.Exists())
        {
            ConfigManager.Delete();
        }

        var config = ConfigManager.Load();

        Assert.Null(config);
    }

    [Fact]
    public void Save_And_Load_Roundtrip_Works()
    {
        try
        {
            var config = new ForebayConfig
            {
                WorkerUrl = "https://forebay.example.com",
                SessionToken = "test-token-123",
                ExpiresAt = 1609459200000,
                Email = "test@example.com"
            };

            ConfigManager.Save(config);
            var loaded = ConfigManager.Load();

            Assert.NotNull(loaded);
            Assert.Equal(config.WorkerUrl, loaded.WorkerUrl);
            Assert.Equal(config.SessionToken, loaded.SessionToken);
            Assert.Equal(config.ExpiresAt, loaded.ExpiresAt);
            Assert.Equal(config.Email, loaded.Email);
        }
        finally
        {
            ConfigManager.Delete();
        }
    }

    [Fact]
    public void Exists_Returns_True_When_File_Exists()
    {
        try
        {
            var config = new ForebayConfig { WorkerUrl = "https://test.com" };
            ConfigManager.Save(config);

            Assert.True(ConfigManager.Exists());
        }
        finally
        {
            ConfigManager.Delete();
        }
    }

    [Fact]
    public void Exists_Returns_False_When_File_Does_Not_Exist()
    {
        if (ConfigManager.Exists())
        {
            ConfigManager.Delete();
        }

        Assert.False(ConfigManager.Exists());
    }

    [Fact]
    public void Delete_Removes_Config_File()
    {
        try
        {
            var config = new ForebayConfig { WorkerUrl = "https://test.com" };
            ConfigManager.Save(config);

            Assert.True(ConfigManager.Exists());

            ConfigManager.Delete();

            Assert.False(ConfigManager.Exists());
        }
        finally
        {
            ConfigManager.Delete();
        }
    }

    [Fact]
    public void IsSessionValid_Returns_True_When_Session_Not_Expired()
    {
        var futureTime = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeMilliseconds();
        var config = new ForebayConfig
        {
            SessionToken = "test-token",
            ExpiresAt = futureTime
        };

        Assert.True(config.IsSessionValid());
    }

    [Fact]
    public void IsSessionValid_Returns_False_When_Session_Expired()
    {
        var pastTime = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();
        var config = new ForebayConfig
        {
            SessionToken = "test-token",
            ExpiresAt = pastTime
        };

        Assert.False(config.IsSessionValid());
    }

    [Fact]
    public void IsSessionValid_Returns_False_When_Token_Missing()
    {
        var config = new ForebayConfig
        {
            SessionToken = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeMilliseconds()
        };

        Assert.False(config.IsSessionValid());
    }

    [Fact]
    public void IsSessionValid_Returns_False_When_ExpiresAt_Missing()
    {
        var config = new ForebayConfig
        {
            SessionToken = "test-token",
            ExpiresAt = null
        };

        Assert.False(config.IsSessionValid());
    }
}
