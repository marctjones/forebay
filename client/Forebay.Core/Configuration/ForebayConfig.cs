using System.Runtime.InteropServices;
using Tomlyn;

namespace Forebay.Core.Configuration;

/// <summary>
/// Represents the Forebay CLI configuration stored in TOML format.
/// </summary>
public class ForebayConfig
{
    public string? WorkerUrl { get; set; }
    public string? ApiKey { get; set; }
}

/// <summary>
/// Manages reading and writing the Forebay configuration file.
/// </summary>
public class ConfigManager
{
    private static readonly string ConfigDirectoryPath = GetConfigDirectory();
    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectoryPath, "config.toml");

    /// <summary>
    /// Gets the platform-specific configuration directory path.
    /// </summary>
    public static string GetConfigDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: %APPDATA%\forebay
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "forebay");
        }
        else
        {
            // Linux/macOS: ~/.config/forebay
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".config", "forebay");
        }
    }

    /// <summary>
    /// Gets the full path to the configuration file.
    /// </summary>
    public static string GetConfigFilePath() => ConfigFilePath;

    /// <summary>
    /// Loads the configuration from disk. Returns null if the file doesn't exist.
    /// </summary>
    public static ForebayConfig? Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return null;
        }

        try
        {
            var toml = File.ReadAllText(ConfigFilePath);
            return Toml.ToModel<ForebayConfig>(toml);
        }
        catch (Exception ex)
        {
            throw new ForebayConfigException($"Failed to load configuration from {ConfigFilePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves the configuration to disk.
    /// </summary>
    public static void Save(ForebayConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(ConfigDirectoryPath);

            // Serialize to TOML
            var toml = Toml.FromModel(config);

            // Write to file
            File.WriteAllText(ConfigFilePath, toml);

            // Set file permissions (Unix only - owner read/write only)
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetUnixFilePermissions(ConfigFilePath, "600");
            }
        }
        catch (Exception ex)
        {
            throw new ForebayConfigException($"Failed to save configuration to {ConfigFilePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes the configuration file.
    /// </summary>
    public static void Delete()
    {
        if (File.Exists(ConfigFilePath))
        {
            File.Delete(ConfigFilePath);
        }
    }

    /// <summary>
    /// Checks if a configuration file exists.
    /// </summary>
    public static bool Exists() => File.Exists(ConfigFilePath);

    private static void SetUnixFilePermissions(string filePath, string permissions)
    {
        try
        {
            // Use chmod to set file permissions (owner read/write only)
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"{permissions} \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();
        }
        catch
        {
            // If chmod fails, continue anyway (file is still written)
            // This can happen in restricted environments
        }
    }
}

public class ForebayConfigException : Exception
{
    public ForebayConfigException(string message) : base(message) { }
    public ForebayConfigException(string message, Exception innerException) : base(message, innerException) { }
}
