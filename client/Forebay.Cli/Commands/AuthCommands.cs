using System.CommandLine;
using System.CommandLine.Invocation;
using Forebay.Core;
using Forebay.Core.Configuration;

namespace Forebay.Cli.Commands;

public static class AuthCommands
{
    public static Command CreateLoginCommand()
    {
        var command = new Command("login", "Set API key for authentication");
        var keyArg = new Argument<string>("api-key", "Your Forebay API key");
        command.Add(keyArg);

        command.SetHandler((string apiKey) =>
        {
            try
            {
                var config = ConfigManager.Load() ?? new ForebayConfig();
                config.ApiKey = apiKey;
                ConfigManager.Save(config);

                Console.WriteLine("API key saved successfully.");
                Console.WriteLine("You can now use Forebay commands.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving API key: {ex.Message}");
            }
        }, keyArg);

        return command;
    }

    public static Command CreateLogoutCommand()
    {
        var command = new Command("logout", "Clear API key");

        command.SetHandler(() =>
        {
            try
            {
                var config = ConfigManager.Load();
                if (config == null || string.IsNullOrEmpty(config.ApiKey))
                {
                    Console.WriteLine("No API key configured.");
                    return;
                }

                ConfigManager.Delete();
                Console.WriteLine("API key cleared successfully.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });

        return command;
    }

    public static Command CreateWhoamiCommand()
    {
        var command = new Command("whoami", "Show current authenticated user");

        command.SetHandler(async () =>
        {
            try
            {
                var config = ConfigManager.Load();
                if (config == null || string.IsNullOrEmpty(config.ApiKey))
                {
                    Console.WriteLine("Not logged in. Run 'forebay login <api-key>' first.");
                    return;
                }

                var workerUrl = config.WorkerUrl ?? "https://forebay.workers.dev";
                var client = new ForebayClient(workerUrl);
                client.SetApiKey(config.ApiKey);

                var response = await client.WhoAmIAsync();

                Console.WriteLine($"Email: {response.Email}");
                Console.WriteLine($"API Key: {config.ApiKey.Substring(0, Math.Min(10, config.ApiKey.Length))}...");
            }
            catch (ForebayApiException ex)
            {
                Console.Error.WriteLine($"API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });

        return command;
    }
}
