---
id: 49
title: Create Forebay.Tasks shared library for TaskList app
state: open
created: '2026-01-04T14:40:59.188634Z'
labels:
- tasklist
- library
- core
priority: high
---
Create the shared core library that will be used by CLI, Desktop, and Mobile TaskList apps.

**Project:** `client/Forebay.Tasks/`

**Components to Implement:**

### 1. Models (`Models/`)

**Task.cs:**
```csharp
public class Task
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool Completed { get; set; }
    public long CreatedAt { get; set; }
    public long? CompletedAt { get; set; }
    public long UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }  // Tombstone for deletions
}
```

**TaskList.cs:**
```csharp
public class TaskList
{
    public List<Task> Tasks { get; set; }
    public long LastSyncTimestamp { get; set; }
}
```

**SyncOperation.cs:**
```csharp
public enum SyncOperationType { Create, Update, Delete }

public class SyncOperation
{
    public SyncOperationType Type { get; set; }
    public Task Task { get; set; }
    public long Timestamp { get; set; }
}
```

### 2. Storage (`Storage/`)

**ITaskStore.cs:**
```csharp
public interface ITaskStore
{
    Task<List<Task>> GetAllAsync();
    Task<Task?> GetByIdAsync(Guid id);
    Task AddAsync(Task task);
    Task UpdateAsync(Task task);
    Task DeleteAsync(Guid id);
    Task<long> GetLastSyncTimestampAsync();
    Task SetLastSyncTimestampAsync(long timestamp);
}
```

**SqliteTaskStore.cs:**
```csharp
public class SqliteTaskStore : ITaskStore
{
    private readonly string _dbPath;
    
    // Implement using Microsoft.Data.Sqlite
    // Table schema:
    // CREATE TABLE tasks (
    //   id TEXT PRIMARY KEY,
    //   title TEXT NOT NULL,
    //   completed INTEGER NOT NULL,
    //   created_at INTEGER NOT NULL,
    //   completed_at INTEGER,
    //   updated_at INTEGER NOT NULL,
    //   is_deleted INTEGER NOT NULL
    // )
    //
    // CREATE TABLE sync_state (
    //   key TEXT PRIMARY KEY,
    //   value INTEGER
    // )
}
```

### 3. Sync Service (`Sync/`)

**TaskSyncService.cs:**
```csharp
public class TaskSyncService
{
    private readonly ITaskStore _localStore;
    private readonly ForebayClient _client;
    private readonly string _queueName;
    
    public async Task PushLocalChangesAsync()
    {
        // Push local changes to Forebay queue
    }
    
    public async Task PullRemoteChangesAsync()
    {
        // Pull changes from Forebay queue
        // Resolve conflicts
        // Update local store
    }
    
    public async Task SyncAsync()
    {
        await PushLocalChangesAsync();
        await PullRemoteChangesAsync();
    }
}
```

**ConflictResolver.cs:**
```csharp
public class ConflictResolver
{
    public Task ResolveConflict(Task local, Task remote)
    {
        // Last-write-wins based on UpdatedAt timestamp
        return remote.UpdatedAt > local.UpdatedAt ? remote : local;
    }
}
```

**SyncState.cs:**
```csharp
public enum SyncStatus { Idle, Syncing, Error }

public class SyncState
{
    public SyncStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public long LastSyncTime { get; set; }
}
```

### 4. Task Manager (`TaskManager.cs`)

```csharp
public class TaskManager
{
    private readonly ITaskStore _store;
    private readonly TaskSyncService _syncService;
    
    public async Task<List<Task>> GetTasksAsync(bool includeCompleted = true)
    public async Task<Task> AddTaskAsync(string title)
    public async Task CompleteTaskAsync(Guid id)
    public async Task UncompleteTaskAsync(Guid id)
    public async Task DeleteTaskAsync(Guid id)
    public async Task SyncAsync()
}
```

**Dependencies:**
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
<ProjectReference Include="../Forebay.Core/Forebay.Core.csproj" />
```

**Acceptance Criteria:**
- [ ] All models implemented with JSON serialization
- [ ] SQLite storage working with migrations
- [ ] Sync service pushes/pulls from queue
- [ ] Conflict resolution (last-write-wins)
- [ ] TaskManager provides high-level API
- [ ] Unit tests for all components (80%+ coverage)
- [ ] Integration tests for sync scenarios
- [ ] Tombstone deletion working
- [ ] Offline support (queue operations buffered)

**Testing:**
- Unit tests for conflict resolution
- Unit tests for SQLite operations
- Integration tests for multi-device sync
- Offline → online transition tests
