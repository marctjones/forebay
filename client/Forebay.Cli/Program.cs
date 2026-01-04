﻿namespace Forebay.Cli;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length > 0 && (args[0] == "--version" || args[0] == "-v"))
        {
            Console.WriteLine("forebay 0.1.0");
            return 0;
        }

        Console.WriteLine("Forebay - Cross-platform message queue transport");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  forebay --version                Show version");
        Console.WriteLine("  forebay login                    Authenticate with Google OAuth");
        Console.WriteLine("  forebay push <queue> [message]   Push message to queue");
        Console.WriteLine("  forebay pull <queue>             Pull message from queue");
        Console.WriteLine("  forebay list                     List all queues");
        Console.WriteLine();
        Console.WriteLine("Run 'forebay <command> --help' for more information on a command.");

        return 0;
    }
}
