---
id: 37
title: Implement C# CLI basic command structure
state: open
created: '2026-01-04T09:22:39.788663Z'
labels:
- cli
- csharp
- infrastructure
priority: high
---
Create the foundational CLI application structure with command pattern and configuration management.

**CLI Framework:**
- Use System.CommandLine for modern CLI experience
- Command pattern for extensibility
- Proper exit codes and error handling
- Configuration file management

**Commands to Implement:**

1. **`forebay --version`**
   - Display version number
   - Display worker URL (if configured)

2. **`forebay --help`**
   - Show all available commands
   - Show usage examples

3. **Configuration Management:**
   - Config file location: `~/.config/forebay/config.toml`
   - Windows: `%APPDATA%/forebay/config.toml`
   - Structure:
     ```toml
     [auth]
     session_token = "..."
     expires_at = 1234567890
     email = "user@example.com"
     
     [worker]
     url = "https://forebay-worker.<account>.workers.dev"
     ```

**Implementation Tasks:**
- [ ] Add System.CommandLine NuGet package
- [ ] Create `Program.cs` with RootCommand
- [ ] Create `ConfigManager` class for TOML read/write
- [ ] Add Tomlyn NuGet package for TOML parsing
- [ ] Implement config file path resolution (cross-platform)
- [ ] Add `--worker-url` global option
- [ ] Set up proper exit codes (0 = success, 1 = error)
- [ ] Add colored console output (optional)

**Command Structure:**
```
forebay
├── login        (authenticate with Google)
├── logout       (end session)
├── whoami       (show current user)
├── push         (add item to queue)
├── pull         (get item from queue)
├── stats        (show queue statistics)
├── list         (list all queues)
├── delete       (delete a queue)
└── subscribe    (long-poll for items)
```

**Acceptance Criteria:**
- CLI compiles and runs
- `--version` and `--help` work
- Config file read/write works
- Proper error messages
- Cross-platform path handling
- Foundation ready for command implementation

**Dependencies:**
- System.CommandLine
- Tomlyn (TOML parser)
