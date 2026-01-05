using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Forebay.Core;
using Forebay.Core.Configuration;

namespace Forebay.Cli.Commands;

public static class QueueCommands
{
    public static Command CreatePushCommand()
    {
        var command = new Command("push", "Push a message to a queue");
        var queueArg = new Argument<string>("queue", "Queue name");
        var messageArg = new Argument<string?>("message", () => null, "Message (reads from stdin if not provided)");

        command.Add(queueArg);
        command.Add(messageArg);

        command.SetHandler(async (InvocationContext context) =>
        {
            var queue = context.ParseResult.GetValueForArgument(queueArg);
            var message = context.ParseResult.GetValueForArgument(messageArg);

            // Get message from stdin if not provided
            string messageText;
            if (message != null)
            {
                messageText = message;
            }
            else if (Console.IsInputRedirected)
            {
                messageText = await Console.In.ReadToEndAsync();
            }
            else
            {
                Console.Error.WriteLine("Error: Provide message as argument or pipe via stdin");
                context.ExitCode = 1;
                return;
            }

            var config = ConfigManager.Load();
            if (config == null || string.IsNullOrEmpty(config.ApiKey))
            {
                Console.Error.WriteLine("Not logged in. Run 'forebay login <api-key>' first.");
                context.ExitCode = 1;
                return;
            }

            try
            {
                JsonElement payload;
                try
                {
                    payload = JsonSerializer.Deserialize<JsonElement>(messageText);
                }
                catch
                {
                    payload = JsonSerializer.Deserialize<JsonElement>($"{{\"message\":{JsonSerializer.Serialize(messageText)}}}");
                }

                var client = new ForebayClient(config.WorkerUrl ?? "https://forebay.workers.dev");
                client.SetApiKey(config.ApiKey!);

                var response = await client.PushAsync(queue, payload);
                Console.WriteLine($"Pushed to {response.Queue} (length: {response.Length}, id: {response.ItemId})");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreatePullCommand()
    {
        var command = new Command("pull", "Pull a message from a queue");
        var queueArg = new Argument<string>("queue", "Queue name");
        var prettyOption = new Option<bool>(new[] { "--pretty", "-p" }, "Pretty-print JSON");

        command.Add(queueArg);
        command.Add(prettyOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var queue = context.ParseResult.GetValueForArgument(queueArg);
            var pretty = context.ParseResult.GetValueForOption(prettyOption);

            var config = ConfigManager.Load();
            if (config == null || string.IsNullOrEmpty(config.ApiKey))
            {
                Console.Error.WriteLine("Not logged in.");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var client = new ForebayClient(config.WorkerUrl ?? "https://forebay.workers.dev");
                client.SetApiKey(config.ApiKey!);

                var response = await client.PullAsync(queue);

                var options = new JsonSerializerOptions { WriteIndented = pretty };
                Console.WriteLine(JsonSerializer.Serialize(response.Payload, options));
                Console.Error.WriteLine($"ID: {response.ItemId}, Remaining: {response.Remaining}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreateStatsCommand()
    {
        var command = new Command("stats", "Show queue statistics");
        var queueArg = new Argument<string>("queue", "Queue name");
        command.Add(queueArg);

        command.SetHandler(async (InvocationContext context) =>
        {
            var queue = context.ParseResult.GetValueForArgument(queueArg);
            var config = ConfigManager.Load();

            if (config == null || string.IsNullOrEmpty(config.ApiKey))
            {
                Console.Error.WriteLine("Not logged in.");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var client = new ForebayClient(config.WorkerUrl ?? "https://forebay.workers.dev");
                client.SetApiKey(config.ApiKey!);

                var response = await client.StatsAsync(queue);
                Console.WriteLine($"Queue: {response.Queue}");
                Console.WriteLine($"Length: {response.Length} items");
                Console.WriteLine($"Size: {response.TotalSizeBytes} bytes");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreateListCommand()
    {
        var command = new Command("list", "List all queues");

        command.SetHandler(async (InvocationContext context) =>
        {
            var config = ConfigManager.Load();
            if (config == null || string.IsNullOrEmpty(config.ApiKey))
            {
                Console.Error.WriteLine("Not logged in.");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var client = new ForebayClient(config.WorkerUrl ?? "https://forebay.workers.dev");
                client.SetApiKey(config.ApiKey!);

                var response = await client.ListQueuesAsync();
                if (response.Queues.Count == 0)
                {
                    Console.WriteLine("No queues found.");
                }
                else
                {
                    foreach (var q in response.Queues)
                    {
                        Console.WriteLine($"{q.Name} ({q.Length} items)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreateDeleteCommand()
    {
        var command = new Command("delete", "Delete a queue");
        var queueArg = new Argument<string>("queue", "Queue name");
        var forceOption = new Option<bool>(new[] { "--force", "-f" }, "Skip confirmation");

        command.Add(queueArg);
        command.Add(forceOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var queue = context.ParseResult.GetValueForArgument(queueArg);
            var force = context.ParseResult.GetValueForOption(forceOption);

            if (!force)
            {
                Console.Write($"Delete queue '{queue}'? (y/N): ");
                var input = Console.ReadLine();
                if (input?.ToLower() != "y")
                {
                    Console.WriteLine("Cancelled.");
                    return;
                }
            }

            var config = ConfigManager.Load();
            if (config == null || string.IsNullOrEmpty(config.ApiKey))
            {
                Console.Error.WriteLine("Not logged in.");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var client = new ForebayClient(config.WorkerUrl ?? "https://forebay.workers.dev");
                client.SetApiKey(config.ApiKey!);

                var response = await client.DeleteQueueAsync(queue);
                Console.WriteLine($"Deleted '{response.Queue}' ({response.DeletedItems} items removed)");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }
}
