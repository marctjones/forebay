using System.CommandLine;
using System.CommandLine.Invocation;
using Forebay.Core;
using Forebay.Core.Configuration;

namespace Forebay.Cli.Commands;

public static class AuthCommands
{
    public static Command CreateLoginCommand()
    {
        var command = new Command("login", "Authenticate with Google OAuth");

        command.SetHandler(() =>
        {
            Console.WriteLine("Login functionality coming soon!");
            Console.WriteLine("This will:");
            Console.WriteLine("1. Start a local HTTP server on localhost:8080");
            Console.WriteLine("2. Open your browser to Google OAuth");
            Console.WriteLine("3. Handle the callback and exchange for session token");
            Console.WriteLine("4. Save the session token to ~/.config/forebay/config.toml");
        });

        return command;
    }

    public static Command CreateLogoutCommand()
    {
        var command = new Command("logout", "Log out and clear session");

        command.SetHandler(async () =>
        {
            try
            {
                var config = ConfigManager.Load();
                if (config == null || string.IsNullOrEmpty(config.SessionToken))
                {
                    Console.WriteLine("Not logged in.");
                    return;
                }

                var workerUrl = config.WorkerUrl ?? "https://forebay.workers.dev";
                var client = new ForebayClient(workerUrl);
                client.SetSessionToken(config.SessionToken);

                await client.LogoutAsync();
                ConfigManager.Delete();

                Console.WriteLine("Logged out successfully.");
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

    public static Command CreateWhoamiCommand()
    {
        var command = new Command("whoami", "Show current authenticated user");

        command.SetHandler(async () =>
        {
            try
            {
                var config = ConfigManager.Load();
                if (config == null || string.IsNullOrEmpty(config.SessionToken))
                {
                    Console.WriteLine("Not logged in. Run 'forebay login' first.");
                    return;
                }

                if (!config.IsSessionValid())
                {
                    Console.WriteLine("Session expired. Run 'forebay login' again.");
                    return;
                }

                var workerUrl = config.WorkerUrl ?? "https://forebay.workers.dev";
                var client = new ForebayClient(workerUrl);
                client.SetSessionToken(config.SessionToken);

                var response = await client.WhoAmIAsync();

                Console.WriteLine($"Email: {response.Email}");
                Console.WriteLine($"Session expires: {DateTimeOffset.FromUnixTimeMilliseconds(response.ExpiresAt):yyyy-MM-dd HH:mm:ss UTC}");

                var daysRemaining = (response.ExpiresAt - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / (24 * 60 * 60 * 1000);
                Console.WriteLine($"Days remaining: {daysRemaining}");
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
