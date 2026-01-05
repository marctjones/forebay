---
id: 51
title: Implement TaskList Desktop app (GNOME + Windows 11)
state: open
created: '2026-01-04T14:42:41.460227Z'
labels:
- tasklist
- desktop
- avalonia
- gui
priority: medium
---
Create cross-platform desktop GUI for TaskList using Avalonia.

**Depends On:** #49 (Forebay.Tasks library)

**Technology:** Avalonia 11.0 with C#

**Single Codebase for:**
- Ubuntu (GNOME Desktop)
- Windows 11 Desktop

**Project:** `client/Forebay.Desktop/` (new)

**UI Design:**

```
┌─────────────────────────────────────────────────┐
│ Forebay TaskList                        ⚙  ─ □ ✕ │
├─────────────────────────────────────────────────┤
│ Add new task...                        [+ Add] │
├─────────────────────────────────────────────────┤
│ ☐ Buy groceries                      2 hours ago│
│ ☑ Write documentation                   1 day ago│
│ ☐ Deploy Worker                      3 hours ago│
│ ☐ Review PR #42                     30 minutes ago│
│                                                 │
│                                                 │
│                                                 │
├─────────────────────────────────────────────────┤
│ ● Last synced 5 minutes ago            [🔄 Sync]│
└─────────────────────────────────────────────────┘
```

**Features:**

### Core Functionality
- Task list with checkboxes
- Add task input box
- Toggle complete/incomplete
- Delete tasks (right-click or delete key)
- Background sync every 30 seconds
- Manual sync button
- Sync status indicator
- System tray icon (minimize to tray)

### Platform-Specific Styling
**GNOME (Linux):**
- Adwaita-inspired theme
- Native GNOME look
- System notifications via libnotify
- Menu bar integration

**Windows 11:**
- Fluent Design System
- WinUI 3-inspired styling
- Acrylic/Mica background effects
- Toast notifications
- Taskbar integration

**Project Structure:**

```
client/Forebay.Desktop/
├── Forebay.Desktop.csproj
├── Program.cs                    # Entry point
├── App.axaml                     # Application styles
├── Views/
│   ├── MainWindow.axaml          # Main window XAML
│   ├── MainWindow.axaml.cs       # Main window code-behind
│   ├── TaskItemView.axaml        # Individual task control
│   └── SettingsWindow.axaml      # Settings dialog
├── ViewModels/
│   ├── MainWindowViewModel.cs    # MVVM pattern
│   ├── TaskItemViewModel.cs
│   └── SettingsViewModel.cs
├── Assets/
│   ├── icon.ico                  # App icon
│   └── avalonia-logo.png
├── Styles/
│   ├── GnomeTheme.axaml          # GNOME styling
│   └── FluentTheme.axaml         # Windows 11 styling
└── Services/
    └── NotificationService.cs    # Platform-specific notifications
```

**Implementation:**

### 1. MainWindow.axaml

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:Forebay.Desktop.ViewModels"
        x:Class="Forebay.Desktop.Views.MainWindow"
        Title="Forebay TaskList"
        Width="600" Height="500"
        MinWidth="400" MinHeight="300">
    
    <Window.DataContext>
        <vm:MainWindowViewModel />
    </Window.DataContext>
    
    <DockPanel>
        <!-- Add Task Bar -->
        <Border DockPanel.Dock="Top" Padding="10" Background="#F5F5F5">
            <Grid ColumnDefinitions="*,Auto">
                <TextBox Grid.Column="0" 
                         Watermark="Add new task..."
                         Text="{Binding NewTaskTitle}"
                         KeyDown="OnEnterPressed"/>
                <Button Grid.Column="1" 
                        Content="+ Add"
                        Command="{Binding AddTaskCommand}"
                        Margin="10,0,0,0"/>
            </Grid>
        </Border>
        
        <!-- Task List -->
        <ListBox DockPanel.Dock="Top"
                 Items="{Binding Tasks}"
                 SelectedItem="{Binding SelectedTask}">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions="Auto,*,Auto,Auto">
                        <CheckBox Grid.Column="0" 
                                  IsChecked="{Binding Completed}"
                                  Command="{Binding $parent[Window].DataContext.ToggleTaskCommand}"
                                  CommandParameter="{Binding}"/>
                        <TextBlock Grid.Column="1" 
                                   Text="{Binding Title}"
                                   Margin="10,0"
                                   TextDecorations="{Binding Completed, Converter={StaticResource CompletedToStrikethrough}}"/>
                        <TextBlock Grid.Column="2" 
                                   Text="{Binding CreatedAt, Converter={StaticResource TimeAgoConverter}}"
                                   Foreground="Gray"
                                   FontSize="12"/>
                        <Button Grid.Column="3"
                                Content="🗑"
                                Command="{Binding $parent[Window].DataContext.DeleteTaskCommand}"
                                CommandParameter="{Binding}"
                                Margin="5,0,0,0"/>
                    </Grid>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        
        <!-- Status Bar -->
        <Border DockPanel.Dock="Bottom" Padding="10" Background="#F0F0F0">
            <Grid ColumnDefinitions="Auto,*,Auto">
                <TextBlock Grid.Column="0" 
                           Text="{Binding SyncStatus}"
                           VerticalAlignment="Center"/>
                <Button Grid.Column="2"
                        Content="🔄 Sync"
                        Command="{Binding SyncCommand}"/>
            </Grid>
        </Border>
    </DockPanel>
</Window>
```

### 2. MainWindowViewModel.cs

```csharp
public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly TaskManager _taskManager;
    private readonly Timer _syncTimer;
    private ObservableCollection<TaskItemViewModel> _tasks;
    private string _newTaskTitle;
    private string _syncStatus;
    
    public MainWindowViewModel()
    {
        _taskManager = new TaskManager(/* ... */);
        _tasks = new ObservableCollection<TaskItemViewModel>();
        
        AddTaskCommand = ReactiveCommand.CreateFromTask(AddTaskAsync);
        ToggleTaskCommand = ReactiveCommand.CreateFromTask<TaskItemViewModel>(ToggleTaskAsync);
        DeleteTaskCommand = ReactiveCommand.CreateFromTask<TaskItemViewModel>(DeleteTaskAsync);
        SyncCommand = ReactiveCommand.CreateFromTask(SyncAsync);
        
        // Background sync every 30 seconds
        _syncTimer = new Timer(async _ => await SyncAsync(), null, 
                               TimeSpan.Zero, TimeSpan.FromSeconds(30));
        
        LoadTasks();
    }
    
    public ObservableCollection<TaskItemViewModel> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }
    
    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set => this.RaiseAndSetIfChanged(ref _newTaskTitle, value);
    }
    
    public string SyncStatus
    {
        get => _syncStatus;
        set => this.RaiseAndSetIfChanged(ref _syncStatus, value);
    }
    
    public ICommand AddTaskCommand { get; }
    public ICommand ToggleTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand SyncCommand { get; }
    
    private async Task LoadTasks()
    {
        var tasks = await _taskManager.GetTasksAsync(includeCompleted: true);
        Tasks.Clear();
        foreach (var task in tasks)
        {
            Tasks.Add(new TaskItemViewModel(task));
        }
    }
    
    private async Task AddTaskAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewTaskTitle))
        {
            await _taskManager.AddTaskAsync(NewTaskTitle);
            NewTaskTitle = string.Empty;
            await LoadTasks();
            await SyncAsync();
        }
    }
    
    private async Task ToggleTaskAsync(TaskItemViewModel taskVm)
    {
        if (taskVm.Completed)
        {
            await _taskManager.UncompleteTaskAsync(taskVm.Id);
        }
        else
        {
            await _taskManager.CompleteTaskAsync(taskVm.Id);
        }
        await LoadTasks();
        await SyncAsync();
    }
    
    private async Task DeleteTaskAsync(TaskItemViewModel taskVm)
    {
        await _taskManager.DeleteTaskAsync(taskVm.Id);
        await LoadTasks();
        await SyncAsync();
    }
    
    private async Task SyncAsync()
    {
        SyncStatus = "● Syncing...";
        await _taskManager.SyncAsync();
        await LoadTasks();
        SyncStatus = $"● Last synced {DateTime.Now:HH:mm:ss}";
    }
}
```

### 3. System Tray Icon

```csharp
public class TrayIconService
{
    private TrayIcon _trayIcon;
    
    public TrayIconService(Window mainWindow)
    {
        _trayIcon = new TrayIcon
        {
            Icon = new WindowIcon("Assets/icon.ico"),
            ToolTipText = "Forebay TaskList",
            Menu = new NativeMenu
            {
                new NativeMenuItem("Show") { Command = /* show window */ },
                new NativeMenuItem("Sync") { Command = /* sync */ },
                new NativeMenuItemSeparator(),
                new NativeMenuItem("Quit") { Command = /* quit */ }
            }
        };
        
        _trayIcon.Clicked += (s, e) => mainWindow.Show();
    }
}
```

### 4. Platform-Specific Notifications

```csharp
public interface INotificationService
{
    void ShowNotification(string title, string message);
}

public class LinuxNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        // Use libnotify via P/Invoke or notify-send command
        Process.Start("notify-send", $"\"{title}\" \"{message}\"");
    }
}

public class WindowsNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        // Use Windows Toast notifications
        var toast = new ToastNotification(/* ... */);
        ToastNotificationManager.CreateToastNotifier().Show(toast);
    }
}
```

**Dependencies:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
    <PackageReference Include="Avalonia.ReactiveUI" Version="11.0.0" />
    <ProjectReference Include="../Forebay.Tasks/Forebay.Tasks.csproj" />
  </ItemGroup>
  
  <ItemGroup Condition="'$(RuntimeIdentifier)' == 'linux-x64'">
    <!-- Linux-specific dependencies -->
  </ItemGroup>
  
  <ItemGroup Condition="'$(RuntimeIdentifier)' == 'win-x64'">
    <!-- Windows-specific dependencies -->
  </ItemGroup>
</Project>
```

**Platform Builds:**

```bash
# Build for Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Build for Windows
dotnet publish -c Release -r win-x64 --self-contained
```

**Testing:**

### Unit Tests
- ViewModel command logic
- Data binding correctness
- Task list updates

### Integration Tests
- Add task → appears in UI
- Toggle → updates database and syncs
- Delete → removes from UI and database
- Background sync → runs every 30s

### Manual Testing
- **Ubuntu GNOME:**
  - Application launches
  - Native GNOME look
  - System notifications work
  - System tray icon appears
  
- **Windows 11:**
  - Application launches
  - Fluent design styling
  - Toast notifications work
  - Taskbar integration
  - Acrylic effects (if supported)

**Acceptance Criteria:**
- [ ] Single codebase for Linux and Windows
- [ ] Application launches on both platforms
- [ ] Tasks sync in real-time
- [ ] Background sync every 30 seconds
- [ ] System tray icon functional
- [ ] Desktop notifications work (both platforms)
- [ ] Add/complete/delete tasks work
- [ ] Offline mode graceful
- [ ] Platform-appropriate styling
- [ ] MVVM pattern properly implemented
- [ ] No platform-specific crashes
- [ ] Unit tests (80%+ coverage)
- [ ] Packaging for distribution

**Distribution:**

### Linux (AppImage)
```bash
# Create AppImage for distribution
./linuxdeploy --appdir AppDir --executable forebay-desktop --output appimage
```

### Windows (MSIX)
```bash
# Create MSIX package
dotnet publish -p:PublishProfile=win-x64
```

**Documentation:**
- Installation guide per platform
- User guide with screenshots
- Keyboard shortcuts
- Troubleshooting common issues
