---
id: 50
title: Implement TaskList CLI + TUI for Ubuntu/Windows Terminal
state: open
created: '2026-01-04T14:41:44.995233Z'
labels:
- tasklist
- cli
- tui
- terminal-gui
priority: high
---
Add TaskList functionality to Forebay.Cli with both interactive TUI and scriptable CLI commands.

**Depends On:** #49 (Forebay.Tasks library)

**Two Modes:**

### 1. Interactive TUI Mode
```bash
forebay task           # Launch Terminal.Gui TUI
```

### 2. CLI Mode (Scriptable)
```bash
forebay task add "Buy groceries"
forebay task list
forebay task complete <id>
forebay task delete <id>
forebay task sync
```

**Implementation:**

### File Structure
```
client/Forebay.Cli/
├── Commands/
│   └── TaskCommands.cs         # CLI command handlers
├── UI/
│   ├── TaskListView.cs         # Main TUI window
│   ├── TaskItemView.cs         # Individual task widget
│   ├── AddTaskDialog.cs        # Add task input
│   └── SyncIndicator.cs        # Sync status widget
└── Forebay.Cli.csproj          # Add Terminal.Gui dependency
```

### TUI Layout
```
┌─ Forebay TaskList ─────────────────────────────────┐
│ Add Task: [________________________] [Add] [Sync]  │
├────────────────────────────────────────────────────┤
│ [ ] Buy groceries                     2h ago       │
│ [✓] Write documentation              1d ago       │
│ [ ] Deploy Worker                     3h ago       │
│ [ ] Review PR #42                     30m ago      │
│                                                    │
│                                                    │
│                                                    │
│                                                    │
│                                                    │
├────────────────────────────────────────────────────┤
│ ● Synced 5m ago | q: Quit | ␣: Toggle | d: Delete │
└────────────────────────────────────────────────────┘
```

### Components to Implement

#### 1. TaskCommands.cs
```csharp
public class TaskCommands
{
    private readonly TaskManager _taskManager;
    
    // Main entry point - decides TUI vs CLI
    public static Command CreateTaskCommand()
    {
        var taskCmd = new Command("task", "Manage tasks");
        
        // No subcommand = launch TUI
        taskCmd.SetHandler(LaunchTUI);
        
        // Add subcommands for CLI mode
        taskCmd.AddCommand(CreateAddCommand());
        taskCmd.AddCommand(CreateListCommand());
        taskCmd.AddCommand(CreateCompleteCommand());
        taskCmd.AddCommand(CreateDeleteCommand());
        taskCmd.AddCommand(CreateSyncCommand());
        
        return taskCmd;
    }
    
    private static void LaunchTUI()
    {
        Application.Init();
        var taskView = new TaskListView();
        Application.Run(taskView);
        Application.Shutdown();
    }
    
    private static Command CreateAddCommand()
    {
        var cmd = new Command("add", "Add a new task");
        var titleArg = new Argument<string>("title", "Task title");
        cmd.AddArgument(titleArg);
        cmd.SetHandler(async (string title) => {
            var task = await _taskManager.AddTaskAsync(title);
            Console.WriteLine($"Added task: {task.Id}");
        }, titleArg);
        return cmd;
    }
    
    private static Command CreateListCommand()
    {
        var cmd = new Command("list", "List all tasks");
        var jsonOpt = new Option<bool>("--json", "Output as JSON");
        var completedOpt = new Option<bool>("--completed", "Include completed");
        cmd.AddOption(jsonOpt);
        cmd.AddOption(completedOpt);
        cmd.SetHandler(async (bool json, bool completed) => {
            var tasks = await _taskManager.GetTasksAsync(completed);
            if (json) {
                Console.WriteLine(JsonSerializer.Serialize(tasks));
            } else {
                foreach (var task in tasks) {
                    var check = task.Completed ? "✓" : " ";
                    Console.WriteLine($"[{check}] {task.Title}");
                }
            }
        }, jsonOpt, completedOpt);
        return cmd;
    }
}
```

#### 2. TaskListView.cs
```csharp
public class TaskListView : Window
{
    private readonly TaskManager _taskManager;
    private readonly ListView _taskListView;
    private readonly TextField _addTaskField;
    private readonly Label _syncLabel;
    private Timer? _syncTimer;
    
    public TaskListView()
    {
        Title = "Forebay TaskList";
        
        // Add task input
        _addTaskField = new TextField { X = 12, Y = 1, Width = 30 };
        var addButton = new Button("Add") { X = 43, Y = 1 };
        addButton.Clicked += OnAddTask;
        
        // Sync button
        var syncButton = new Button("Sync") { X = 49, Y = 1 };
        syncButton.Clicked += OnSync;
        
        // Task list
        _taskListView = new ListView {
            X = 1, Y = 3, Width = Dim.Fill(1), Height = Dim.Fill(2)
        };
        _taskListView.KeyPress += OnKeyPress;
        
        // Status bar
        _syncLabel = new Label("○ Not synced") { X = 1, Y = Pos.Bottom(_taskListView) };
        var helpLabel = new Label("q: Quit | ␣: Toggle | d: Delete | s: Sync") {
            X = Pos.Right(_syncLabel) + 3,
            Y = Pos.Bottom(_taskListView)
        };
        
        Add(_addTaskField, addButton, syncButton, _taskListView, _syncLabel, helpLabel);
        
        // Start background sync
        _syncTimer = new Timer(SyncPeriodically, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        
        LoadTasks();
    }
    
    private async void LoadTasks()
    {
        var tasks = await _taskManager.GetTasksAsync(includeCompleted: true);
        _taskListView.SetSource(tasks.Select(FormatTask).ToList());
    }
    
    private string FormatTask(Task task)
    {
        var check = task.Completed ? "✓" : " ";
        var ago = FormatTimeAgo(task.CreatedAt);
        return $"[{check}] {task.Title.PadRight(40)} {ago}";
    }
    
    private void OnKeyPress(KeyEventEventArgs e)
    {
        if (e.KeyEvent.Key == Key.Space) {
            ToggleSelectedTask();
            e.Handled = true;
        } else if (e.KeyEvent.Key == Key.d) {
            DeleteSelectedTask();
            e.Handled = true;
        } else if (e.KeyEvent.Key == Key.s) {
            OnSync();
            e.Handled = true;
        }
    }
    
    private async void OnAddTask()
    {
        var title = _addTaskField.Text.ToString();
        if (!string.IsNullOrWhiteSpace(title)) {
            await _taskManager.AddTaskAsync(title);
            _addTaskField.Text = "";
            LoadTasks();
            await _taskManager.SyncAsync();
        }
    }
    
    private async void OnSync()
    {
        _syncLabel.Text = "● Syncing...";
        Application.Refresh();
        
        await _taskManager.SyncAsync();
        LoadTasks();
        
        _syncLabel.Text = $"● Synced {DateTime.Now:HH:mm:ss}";
        Application.Refresh();
    }
}
```

#### 3. CLI Commands Implementation

**Add:**
```bash
forebay task add "Buy groceries"
# Output: Added task: 550e8400-e29b-41d4-a716-446655440000
```

**List:**
```bash
forebay task list
# Output:
# [ ] Buy groceries
# [✓] Write documentation
# [ ] Deploy Worker

forebay task list --json
# Output: [{"id":"...","title":"Buy groceries",...}]

forebay task list --completed=false
# Output: (only incomplete tasks)
```

**Complete:**
```bash
forebay task complete 550e8400-e29b-41d4-a716-446655440000
# Output: Task completed

forebay task complete <id> --undo
# Output: Task marked incomplete
```

**Delete:**
```bash
forebay task delete 550e8400-e29b-41d4-a716-446655440000
# Output: Task deleted
```

**Sync:**
```bash
forebay task sync
# Output: Synced 3 changes
```

**Search:**
```bash
forebay task search "groceries"
# Output: [ ] Buy groceries
```

**Dependencies:**
```xml
<PackageReference Include="Terminal.Gui" Version="1.17.1" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
<ProjectReference Include="../Forebay.Tasks/Forebay.Tasks.csproj" />
```

**Configuration:**
Store tasks in:
- Linux: `~/.local/share/forebay/tasks.db`
- Windows: `%LOCALAPPDATA%\forebay\tasks.db`

**Testing:**

### Unit Tests
- Command parsing and routing
- CLI output formatting
- Task ID validation

### Integration Tests
- Add task → appears in list
- Complete task → checkbox updates
- Delete task → removed from list
- Sync → pushes/pulls from queue

### Manual Testing
- TUI launches and displays tasks
- Keyboard shortcuts work
- Background sync runs every 30s
- Works on Ubuntu and Windows Terminal
- Color output works correctly
- Mouse support (if enabled)

**Acceptance Criteria:**
- [ ] `forebay task` launches interactive TUI
- [ ] All CLI commands work (add, list, complete, delete, sync)
- [ ] TUI keyboard shortcuts work (space, d, s, q)
- [ ] Background sync runs every 30 seconds in TUI
- [ ] Sync indicator updates in real-time
- [ ] Tasks persist to SQLite
- [ ] Works on Ubuntu terminal
- [ ] Works on Windows Terminal
- [ ] JSON output option for scripting
- [ ] Proper error messages
- [ ] Unit tests (80%+ coverage)
- [ ] Integration tests pass

**Documentation:**
- Add to README.md usage section
- Document keyboard shortcuts
- Add scripting examples
- Troubleshooting guide
