using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forebay.Core;
using Forebay.Core.Configuration;

namespace Forebay.TaskManager.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ForebayClient _client;
    private const string TASKS_KEY = "tasks/list";

    [ObservableProperty]
    private string _newTaskText = "";

    [ObservableProperty]
    private ObservableCollection<TaskItemViewModel> _tasks = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainWindowViewModel()
    {
        // Initialize Forebay client
        var config = ConfigManager.Load();
        if (config == null || string.IsNullOrEmpty(config.ApiKey))
        {
            StatusMessage = "Error: Not logged in. Run 'forebay login <api-key>' first.";
            _client = null!;
            return;
        }

        _client = new ForebayClient(config.WorkerUrl ?? "https://forebay-worker.marc-t-jones.workers.dev");
        _client.SetApiKey(config.ApiKey);

        // Load initial tasks
        _ = LoadTasksAsync();
    }

    [RelayCommand]
    private async Task LoadTasksAsync()
    {
        try
        {
            StatusMessage = "Loading tasks...";
            var response = await _client.GetDocumentAsync(TASKS_KEY);
            var tasksJson = response.Content.GetProperty("tasks");
            var tasksList = JsonSerializer.Deserialize<TaskItem[]>(tasksJson.GetRawText()) ?? Array.Empty<TaskItem>();

            Tasks.Clear();
            foreach (var task in tasksList)
            {
                Tasks.Add(new TaskItemViewModel
                {
                    Id = task.Id,
                    Text = task.Text,
                    Done = task.Done,
                    Parent = this
                });
            }

            StatusMessage = $"Loaded {Tasks.Count} tasks";
        }
        catch
        {
            // Document doesn't exist yet
            Tasks.Clear();
            StatusMessage = "No tasks found (starting fresh)";
        }
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskText))
            return;

        var newTask = new TaskItemViewModel
        {
            Id = Tasks.Count > 0 ? Tasks.Max(t => t.Id) + 1 : 1,
            Text = NewTaskText,
            Done = false,
            Parent = this
        };

        Tasks.Add(newTask);
        NewTaskText = "";

        await SaveTasksAsync();
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItemViewModel task)
    {
        Tasks.Remove(task);
        await SaveTasksAsync();
    }

    public async Task SaveTasksAsync()
    {
        try
        {
            StatusMessage = "Saving tasks...";

            var taskItems = Tasks.Select(t => new TaskItem
            {
                Id = t.Id,
                Text = t.Text,
                Done = t.Done
            }).ToArray();

            var tasksJson = JsonSerializer.Serialize(new { tasks = taskItems });
            var content = JsonDocument.Parse(tasksJson).RootElement;

            await _client.PutDocumentAsync(TASKS_KEY, content);
            StatusMessage = $"Saved {Tasks.Count} tasks";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }
}

public partial class TaskItemViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _text = "";

    [ObservableProperty]
    private bool _done;

    public MainWindowViewModel? Parent { get; set; }

    partial void OnDoneChanged(bool value)
    {
        _ = Parent?.SaveTasksAsync();
    }
}

public class TaskItem
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public bool Done { get; set; }
}
