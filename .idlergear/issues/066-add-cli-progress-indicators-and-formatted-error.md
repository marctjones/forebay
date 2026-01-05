---
id: 66
title: Add CLI progress indicators and formatted error messages
state: open
created: '2026-01-05T00:43:24.515286Z'
labels:
- cli
- csharp
- enhancement
- ux
priority: medium
---
Improve CLI user experience with progress indicators, spinners, and formatted error messages.

**Current:** Plain text output, no visual feedback
**Desired:** Modern CLI experience like `gh`, `docker`, `kubectl`

## Features

### Progress Indicators
```bash
forebay push work/tasks '{"data":"..."}'
⠋ Pushing to work/tasks...
✓ Pushed message a1b2c3d4 to work/tasks (120ms)
```

### Spinners for Long Operations
```bash
forebay login
⠋ Opening browser...
⠙ Waiting for OAuth callback...
✓ Logged in as user@example.com
```

### Formatted Error Messages
```bash
forebay push invalid/queue '...'
✗ Error: Queue name invalid
  
  Queue names must:
  - Use alphanumeric characters, dashes, slashes
  - Be between 1-200 characters
  - Not start or end with slashes
  
  Example: work/tasks, data/processing/batch-1
```

### Color-Coded Output
- ✅ Green for success
- ❌ Red for errors
- ⚠️  Yellow for warnings
- ℹ️  Blue for info
- Gray for metadata

### Table Output
```bash
forebay sessions list
┌──────────────┬────────────┬───────────┬─────────┐
│ Device       │ Created    │ Last Used │ Status  │
├──────────────┼────────────┼───────────┼─────────┤
│ MacBook Pro  │ 2 days ago │ now       │ current │
│ Ubuntu       │ 1 week ago │ 5 min ago │ active  │
└──────────────┴────────────┴───────────┴─────────┘
```

## Implementation

### Use Spectre.Console Library
```xml
<PackageReference Include="Spectre.Console" Version="0.49.0" />
```

**Features:**
- Spinners and progress bars
- Tables and trees
- Color markup
- Interactive prompts
- Layout and panels

### Example Code
```csharp
// Progress spinner
await AnsiConsole.Status()
    .Start("Pushing to queue...", async ctx =>
    {
        var response = await client.PushAsync(queue, payload);
        ctx.Status("✓ Complete");
    });

// Formatted error
AnsiConsole.MarkupLine("[red]✗ Error:[/] Queue not found");
AnsiConsole.MarkupLine("[dim]  Queue 'work/tasks' does not exist[/]");

// Table
var table = new Table();
table.AddColumn("Device");
table.AddColumn("Created");
table.AddRow("MacBook Pro", "2 days ago");
AnsiConsole.Write(table);
```

### Quiet Mode
```bash
forebay push --quiet work/tasks '...'
# No spinners, just result (for scripting)
```

## Files
- client/Forebay.Cli/UI/ConsoleUI.cs
- client/Forebay.Cli/UI/Spinners.cs
- client/Forebay.Cli/UI/Formatters.cs

## Acceptance Criteria
- [ ] Spinners for slow operations
- [ ] Color-coded output
- [ ] Formatted error messages
- [ ] Table formatting for list commands
- [ ] --quiet flag disables UI enhancements
- [ ] Works on Windows, Linux, macOS

**Priority:** Medium (nice-to-have for v1.0)
**Milestone:** v1.0 - Polish & Release
