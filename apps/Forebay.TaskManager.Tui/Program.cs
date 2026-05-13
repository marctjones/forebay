using System.Text.Json;
using Forebay.Core;
using Forebay.Core.Configuration;
using Terminal.Gui;

namespace Forebay.TaskManager.Tui;

class Program
{
    private static ForebayClient? _client;
    private static ListView? _taskListView;
    private static List<TaskItem> _tasks = new();
    private const string TASKS_KEY = "tasks/list";

    static void Main(string[] args)
    {
        // Initialize Forebay client
        var config = ConfigManager.Load();
        if (config == null || string.IsNullOrEmpty(config.ApiKey))
        {
            Console.WriteLine("Error: Not logged in. Run 'forebay login <api-key>' first.");
            return;
        }

        _client = new ForebayClient(config.WorkerUrl ?? "https://forebay-worker.marc-t-jones.workers.dev");
        _client.SetApiKey(config.ApiKey);

        // Handle Ctrl+C to ensure proper shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Application.RequestStop();
        };

        Application.Init();

        var top = Application.Top;

        // Create menu
        var menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("_Refresh", "", RefreshTasks),
                new MenuItem("_Quit", "", () => Application.RequestStop())
            }),
            new MenuBarItem("_Tasks", new MenuItem[]
            {
                new MenuItem("_Add", "", AddTask),
                new MenuItem("_Toggle Done", "", ToggleTaskDone),
                new MenuItem("_Delete", "", DeleteTask)
            })
        });
        top.Add(menu);

        // Create main window
        var win = new Window("Forebay Task Manager")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Task list
        _taskListView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 3
        };
        win.Add(_taskListView);

        // Status bar
        var statusBar = new Label("F2=Add | F3=Toggle | F4=Delete | F5=Refresh | Ctrl+Q=Quit")
        {
            X = 0,
            Y = Pos.Bottom(win) - 3,
            Width = Dim.Fill(),
            ColorScheme = Colors.TopLevel
        };
        win.Add(statusBar);

        top.Add(win);

        // Keyboard shortcuts
        Application.RootKeyEvent = (keyEvent) =>
        {
            if (keyEvent.KeyValue == (int)Key.F2)
            {
                AddTask();
                return true;
            }
            if (keyEvent.KeyValue == (int)Key.F3)
            {
                ToggleTaskDone();
                return true;
            }
            if (keyEvent.KeyValue == (int)Key.F4)
            {
                DeleteTask();
                return true;
            }
            if (keyEvent.KeyValue == (int)Key.F5)
            {
                RefreshTasks();
                return true;
            }
            if (keyEvent.KeyValue == (int)Key.CtrlMask + 'Q')
            {
                Application.RequestStop();
                return true;
            }
            return false;
        };

        // Load initial tasks asynchronously
        Task.Run(async () =>
        {
            await RefreshTasksAsync();
            Application.MainLoop.Invoke(() => UpdateTaskListView());
        });

        Application.Run();
        Application.Shutdown();
    }

    static async Task RefreshTasksAsync()
    {
        try
        {
            var response = await _client!.GetDocumentAsync(TASKS_KEY);
            var tasksJson = response.Content.GetProperty("tasks");
            _tasks = JsonSerializer.Deserialize<List<TaskItem>>(tasksJson.GetRawText()) ?? new();
        }
        catch
        {
            // Document doesn't exist yet, start with empty list
            _tasks = new();
        }
    }

    static void RefreshTasks()
    {
        Task.Run(async () =>
        {
            await RefreshTasksAsync();
            Application.MainLoop.Invoke(() => UpdateTaskListView());
        });
    }

    static void UpdateTaskListView()
    {
        var displayTasks = _tasks.Select((t, i) =>
            $"{i + 1}. [{(t.Done ? "X" : " ")}] {t.Text}"
        ).ToList();

        _taskListView!.SetSource(displayTasks);
    }

    static void SaveTasks()
    {
        Task.Run(async () =>
        {
            try
            {
                var tasksJson = JsonSerializer.Serialize(new { tasks = _tasks });
                var content = JsonDocument.Parse(tasksJson).RootElement;
                await _client!.PutDocumentAsync(TASKS_KEY, content);
            }
            catch (Exception ex)
            {
                Application.MainLoop.Invoke(() =>
                {
                    MessageBox.ErrorQuery("Error", $"Failed to save tasks: {ex.Message}", "OK");
                });
            }
        });
    }

    static void AddTask()
    {
        var dialog = new Dialog("Add New Task", 60, 10);

        var label = new Label("Task description:")
        {
            X = 1,
            Y = 1
        };

        var textField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill() - 2
        };

        var btnOk = new Button("OK", is_default: true)
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(dialog) - 4
        };
        btnOk.Clicked += () =>
        {
            var text = textField.Text.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                _tasks.Add(new TaskItem
                {
                    Id = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1,
                    Text = text!,
                    Done = false
                });
                SaveTasks();
                UpdateTaskListView();
                Application.RequestStop();
            }
        };

        var btnCancel = new Button("Cancel")
        {
            X = Pos.Center() + 5,
            Y = Pos.Bottom(dialog) - 4
        };
        btnCancel.Clicked += () => Application.RequestStop();

        dialog.Add(label, textField, btnOk, btnCancel);
        textField.SetFocus();

        Application.Run(dialog);
    }

    static void ToggleTaskDone()
    {
        if (_tasks.Count == 0) return;

        var selected = _taskListView!.SelectedItem;
        if (selected >= 0 && selected < _tasks.Count)
        {
            _tasks[selected].Done = !_tasks[selected].Done;
            SaveTasks();
            UpdateTaskListView();
        }
    }

    static void DeleteTask()
    {
        if (_tasks.Count == 0) return;

        var selected = _taskListView!.SelectedItem;
        if (selected >= 0 && selected < _tasks.Count)
        {
            var result = MessageBox.Query("Delete Task",
                $"Delete task: {_tasks[selected].Text}?",
                "Yes", "No");

            if (result == 0)
            {
                _tasks.RemoveAt(selected);
                SaveTasks();
                UpdateTaskListView();
            }
        }
    }
}

class TaskItem
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public bool Done { get; set; }
}
