using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Forebay.Core;
using Forebay.Core.Configuration;

namespace Forebay.Cli.Commands;

public static class StorageCommands
{
    public static Command CreatePutCommand()
    {
        var command = new Command("put", "Store a document");
        var keyArg = new Argument<string>("key", "Document key");
        var contentArg = new Argument<string?>("content", () => null, "Content (reads from stdin if not provided)");

        command.Add(keyArg);
        command.Add(contentArg);

        command.SetHandler(async (InvocationContext context) =>
        {
            var key = context.ParseResult.GetValueForArgument(keyArg);
            var content = context.ParseResult.GetValueForArgument(contentArg);

            // Get content from stdin if not provided
            string contentText;
            if (content != null)
            {
                contentText = content;
            }
            else if (Console.IsInputRedirected)
            {
                contentText = await Console.In.ReadToEndAsync();
            }
            else
            {
                Console.Error.WriteLine("Error: Provide content as argument or pipe via stdin");
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
                JsonElement jsonContent;
                try
                {
                    jsonContent = JsonSerializer.Deserialize<JsonElement>(contentText);
                }
                catch
                {
                    jsonContent = JsonSerializer.Deserialize<JsonElement>($"{{\"value\":{JsonSerializer.Serialize(contentText)}}}");
                }

                var client = new ForebayClient(config.WorkerUrl ?? "https://forebay.workers.dev");
                client.SetApiKey(config.ApiKey!);

                var response = await client.PutDocumentAsync(key, jsonContent);
                Console.WriteLine($"Stored '{response.Key}' ({response.SizeBytes} bytes)");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreateGetCommand()
    {
        var command = new Command("get", "Retrieve a document");
        var keyArg = new Argument<string>("key", "Document key");
        var prettyOption = new Option<bool>(new[] { "--pretty", "-p" }, "Pretty-print JSON");

        command.Add(keyArg);
        command.Add(prettyOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var key = context.ParseResult.GetValueForArgument(keyArg);
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

                var response = await client.GetDocumentAsync(key);

                var options = new JsonSerializerOptions { WriteIndented = pretty };
                Console.WriteLine(JsonSerializer.Serialize(response.Content, options));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreateDeleteDocumentCommand()
    {
        var command = new Command("delete-doc", "Delete a document");
        var keyArg = new Argument<string>("key", "Document key");
        var forceOption = new Option<bool>(new[] { "--force", "-f" }, "Skip confirmation");

        command.Add(keyArg);
        command.Add(forceOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var key = context.ParseResult.GetValueForArgument(keyArg);
            var force = context.ParseResult.GetValueForOption(forceOption);

            if (!force)
            {
                Console.Write($"Delete document '{key}'? (y/N): ");
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

                var response = await client.DeleteDocumentAsync(key);
                Console.WriteLine($"Deleted '{response.Key}'");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        });

        return command;
    }

    public static Command CreateListDocumentsCommand()
    {
        var command = new Command("list-docs", "List all documents");
        var prefixOption = new Option<string?>(new[] { "--prefix", "-p" }, "Filter by key prefix");
        var limitOption = new Option<int?>(new[] { "--limit", "-l" }, "Maximum number of documents");

        command.Add(prefixOption);
        command.Add(limitOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var prefix = context.ParseResult.GetValueForOption(prefixOption);
            var limit = context.ParseResult.GetValueForOption(limitOption);

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

                var response = await client.ListDocumentsAsync(prefix, limit);
                if (response.Documents.Count == 0)
                {
                    Console.WriteLine("No documents found.");
                }
                else
                {
                    foreach (var doc in response.Documents)
                    {
                        Console.WriteLine($"{doc.Key} ({doc.SizeBytes} bytes)");
                    }
                    Console.WriteLine($"\nTotal: {response.Count} documents");
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
}
