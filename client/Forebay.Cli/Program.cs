using System.CommandLine;
using Forebay.Cli.Commands;

namespace Forebay.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Forebay - Cross-platform message queue transport");

        // Global options
        var workerUrlOption = new Option<string?>(
            new[] { "--worker-url", "-w" },
            "Worker URL (overrides config file)"
        );
        rootCommand.Add(workerUrlOption);

        var verboseOption = new Option<bool>(
            new[] { "--verbose", "-v" },
            "Enable verbose output"
        );
        rootCommand.Add(verboseOption);

        // Add commands
        // Authentication
        rootCommand.Add(AuthCommands.CreateLoginCommand());
        rootCommand.Add(AuthCommands.CreateLogoutCommand());
        rootCommand.Add(AuthCommands.CreateWhoamiCommand());
        // Queue operations
        rootCommand.Add(QueueCommands.CreatePushCommand());
        rootCommand.Add(QueueCommands.CreatePullCommand());
        rootCommand.Add(QueueCommands.CreateStatsCommand());
        rootCommand.Add(QueueCommands.CreateListCommand());
        rootCommand.Add(QueueCommands.CreateDeleteCommand());
        // Storage operations
        rootCommand.Add(StorageCommands.CreatePutCommand());
        rootCommand.Add(StorageCommands.CreateGetCommand());
        rootCommand.Add(StorageCommands.CreateListDocumentsCommand());
        rootCommand.Add(StorageCommands.CreateDeleteDocumentCommand());

        return await rootCommand.InvokeAsync(args);
    }
}
