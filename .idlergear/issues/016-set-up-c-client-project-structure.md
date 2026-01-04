---
id: 16
title: Set up C# client project structure
state: open
created: '2026-01-04T08:32:48.980633Z'
labels:
- phase-1
- csharp
- client
- setup
priority: high
---
Initialize the C# client solution for Forebay.

**Project structure:**
```
client/
├── Forebay.sln
├── Forebay.Core/              # Shared library
│   ├── IQueueClient.cs        # Interface
│   ├── CloudflareClient.cs    # HTTP client
│   ├── Models/
│   │   ├── QueueItem.cs
│   │   ├── AuthResponse.cs
│   │   └── QueueStats.cs
│   └── Forebay.Core.csproj
├── Forebay.Cli/               # CLI app
│   ├── Program.cs
│   ├── Commands/
│   │   ├── PushCommand.cs
│   │   ├── PullCommand.cs
│   │   └── LoginCommand.cs
│   └── Forebay.Cli.csproj
└── Forebay.Tests/             # Unit tests
    └── Forebay.Tests.csproj
```

**Requirements:**
- .NET 9.0 target
- Use System.CommandLine for CLI parsing
- HttpClient for REST API calls
- JSON serialization with System.Text.Json
- Config file at ~/.config/forebay/config.toml
- Single binary publish (self-contained or native AOT)

**Initial implementation:**
- Create solution structure
- Add basic `forebay --version` command
- Verify build and publish works
