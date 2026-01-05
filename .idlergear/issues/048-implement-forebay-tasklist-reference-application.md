---
id: 48
title: Implement Forebay TaskList reference application across all platforms
state: open
created: '2026-01-04T14:16:54.293841Z'
labels:
- app
- example
- cross-platform
- avalonia
priority: medium
---
Create a reference task list application demonstrating Forebay usage across all target platforms.

**Purpose:**
- Demonstrate real-world Forebay usage
- Validate cross-platform client strategy
- Provide example for other developers
- Test Forebay queue operations in production use

**Application Features:**

### Core Functionality
- Add tasks to personal task list
- Mark tasks as complete/incomplete
- Delete tasks
- View all tasks
- Sync across devices via Forebay queues

### Data Model
```json
{
  "id": "uuid",
  "title": "Task description",
  "completed": false,
  "created_at": 1609459200000,
  "completed_at": null
}
```

### Queue Usage Pattern
- Queue name: `tasks/{user_email}/inbox`
- Push: Add new tasks or task updates
- Pull: Sync tasks from other devices
- Local state + queue for offline support

**Platform Implementations:**

## 1. Ubuntu Terminal (TUI + CLI) - Priority: HIGH
**Technology:** C# with Terminal.Gui framework

**Two Modes:**

### Interactive TUI Mode
```bash
forebay task           # Launch interactive TUI
```

**UI Features:**
- List view with navigation (arrow keys)
- Add task input at top
- Toggle complete with spacebar
- Delete with 'd' key
- Sync indicator in status bar
- Real-time updates
- Mouse support optional

**TUI Layout:**
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
│ q: Quit | Space: Toggle | d: Delete | s: Sync     │
└────────────────────────────────────────────────────┘
```

### CLI Mode (Scriptable)
```bash
# Add task
forebay task add "Buy groceries"
forebay task add "Deploy Worker" --priority high

# List tasks
forebay task list
forebay task list --completed
forebay task list --json

# Complete task
forebay task complete <id>
forebay task complete <id> --undo

# Delete task
forebay task delete <id>

# Sync
forebay task sync

# Search
forebay task search "groceries"
```

**Implementation:**
- Extend existing `Forebay.Cli` project
- Add Terminal.Gui dependency
- Add `task` command group
- Interactive mode: `forebay task`
- CLI mode: `forebay task <subcommand>`
- Store tasks in local SQLite database
- Sync via Forebay queue on changes
- Background sync when TUI is open

**File Structure:**
```
client/Forebay.Cli/
├── Commands/
│   └── TaskCommands.cs         # CLI command handlers
├── UI/
│   ├── TaskListView.cs         # Terminal.Gui TUI
│   ├── TaskInputDialog.cs
│   └── SyncIndicator.cs
```

**Dependencies:**
- Terminal.Gui 1.x (Microsoft-maintained)
- Microsoft.Data.Sqlite
- Existing Forebay.Core

## 2. Windows 11 Terminal (Same TUI + CLI) - Priority: HIGH
**Technology:** Same C# binary as Ubuntu

**Features:**
- Same Terminal.Gui TUI works on Windows
- Windows Terminal color support
- PowerShell-friendly output
- Same keyboard shortcuts

**Testing:**
- Verify TUI works in Windows Terminal
- Test PowerShell integration
- Verify ANSI color output
- Test WSL compatibility

## 3. GNOME Desktop (Avalonia) - Priority: MEDIUM
**Technology:** Avalonia with C#

**UI:**
- Native GNOME look with Avalonia
- Main window with task list
- Add task input box
- Checkboxes to complete tasks
- Delete button per task
- Sync indicator
- System tray icon

**Implementation:**
- Use Avalonia for cross-platform consistency
- Same sync logic as CLI
- Background sync every 30 seconds
- Desktop notifications for new tasks
- Share `Forebay.Tasks` library

**Project:** `client/Forebay.Desktop/` (new)

## 4. Windows 11 Desktop (Avalonia) - Priority: MEDIUM
**Technology:** Same Avalonia codebase as GNOME

**UI:**
- Windows 11 Fluent Design
- WinUI 3 styling via Avalonia themes
- Acrylic background
- Taskbar integration
- Toast notifications

**Implementation:**
- Same Avalonia codebase as GNOME Desktop
- Platform-specific styling/themes
- Windows-specific features (notifications)

**Shared:** `client/Forebay.Desktop/` with platform renderers

## 5. Android App - Priority: LOW (Future)
**Technology:** .NET MAUI (better mobile support than Avalonia)

**UI:**
- Material Design 3
- Swipe to complete/delete
- Pull to refresh/sync
- Offline support
- Push notifications

**Implementation:**
- .NET MAUI for native Android experience
- Share `Forebay.Tasks` core library
- Local SQLite storage
- Background sync service
- Android notification integration

**Project:** `client/Forebay.Mobile/` (new)

**Technical Architecture:**

### Shared Core Library (`Forebay.Tasks`)
```
client/Forebay.Tasks/              # New shared library
├── Models/
│   ├── Task.cs
│   ├── TaskList.cs
│   └── SyncOperation.cs
├── Storage/
│   ├── ITaskStore.cs
│   └── SqliteTaskStore.cs
├── Sync/
│   ├── TaskSyncService.cs
│   ├── ConflictResolver.cs
│   └── SyncState.cs
└── TaskManager.cs
```

**Project References:**
- `Forebay.Cli` → `Forebay.Tasks` → `Forebay.Core`
- `Forebay.Desktop` → `Forebay.Tasks` → `Forebay.Core`
- `Forebay.Mobile` → `Forebay.Tasks` → `Forebay.Core`

**Sync Strategy:**
1. Local changes pushed to queue immediately
2. Pull from queue on app start and periodically (30s)
3. Last-write-wins conflict resolution (timestamp-based)
4. Tombstone records for deletions
5. Operation log for conflict debugging

**Conflict Resolution:**
```csharp
// Last-write-wins based on timestamp
if (remoteTask.UpdatedAt > localTask.UpdatedAt) {
    // Remote wins, update local
    await localStore.UpdateAsync(remoteTask);
} else {
    // Local wins, ignore remote
}
```

**Offline Support:**
- All operations work offline
- Queue operations buffer when offline
- Sync when connection restored
- Visual indicator of sync state

**Implementation Phases:**

### Phase 1: CLI + TUI (Ubuntu + Windows Terminal)
**Priority: HIGH** - Foundation for all platforms

- [ ] Create `Forebay.Tasks` shared library
- [ ] Implement `Task` and `TaskList` models
- [ ] Implement SQLite storage (`SqliteTaskStore`)
- [ ] Implement sync service (`TaskSyncService`)
- [ ] Add CLI commands (`task add`, `task list`, etc.)
- [ ] Implement Terminal.Gui TUI
- [ ] Add background sync in TUI mode
- [ ] Test on Ubuntu
- [ ] Test on Windows Terminal
- [ ] Test sync between two machines

**Estimated Effort:** 2-3 days

### Phase 2: Desktop (GNOME + Windows 11)
**Priority: MEDIUM** - Rich GUI experience

- [ ] Create `Forebay.Desktop` Avalonia project
- [ ] Design UI with Avalonia XAML
- [ ] Integrate `Forebay.Tasks` library
- [ ] Implement background sync (every 30s)
- [ ] Add desktop notifications
- [ ] Add system tray icon
- [ ] Style for GNOME (Adwaita-inspired)
- [ ] Style for Windows 11 (Fluent Design)
- [ ] Test on Ubuntu (GNOME)
- [ ] Test on Windows 11

**Estimated Effort:** 3-4 days

### Phase 3: Android (Future)
**Priority: LOW** - Mobile expansion

- [ ] Evaluate MAUI vs Avalonia Mobile
- [ ] Create `Forebay.Mobile` MAUI project
- [ ] Implement Material Design UI
- [ ] Add swipe gestures
- [ ] Implement background sync service
- [ ] Add push notifications
- [ ] Test on Android devices (API 29+)

**Estimated Effort:** 5-7 days

**Dependencies:**

### CLI + TUI
```xml
<PackageReference Include="Terminal.Gui" Version="1.17.1" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
```

### Desktop
```xml
<PackageReference Include="Avalonia" Version="11.0.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
```

### Mobile (Future)
```xml
<PackageReference Include="Microsoft.Maui" Version="9.0.0" />
```

**Acceptance Criteria:**

### CLI + TUI
- [ ] Interactive TUI launches with `forebay task`
- [ ] All CLI commands work in script mode
- [ ] Tasks sync across devices
- [ ] Offline support functional
- [ ] Works on Ubuntu and Windows Terminal
- [ ] Mouse support in TUI (optional)
- [ ] Keyboard shortcuts documented
- [ ] TUI updates in real-time during sync

### Desktop
- [ ] Single codebase for Linux and Windows
- [ ] Tasks sync in real-time
- [ ] Background sync every 30 seconds
- [ ] Desktop notifications work on both platforms
- [ ] System tray icon functional
- [ ] Offline mode graceful degradation
- [ ] Native look and feel per platform

### Android (Future)
- [ ] Material Design 3 UI
- [ ] Background sync works
- [ ] Push notifications work
- [ ] Offline support complete
- [ ] Battery-efficient sync

**Documentation:**
- User guide for TaskList app
- TUI keyboard shortcuts reference
- Architecture documentation for sync
- Sync conflict resolution explanation
- Example code for other developers

**Testing Strategy:**

### Unit Tests
- Task model serialization
- Sync conflict resolution logic
- SQLite operations
- Queue push/pull operations

### Integration Tests
- Multi-device sync scenarios
- Offline → online transitions
- Conflict resolution cases
- Queue message ordering

### Manual Testing
- TUI usability testing
- Cross-platform verification
- Sync timing and reliability
- UI responsiveness

**Future Enhancements:**
- Task priorities (high, medium, low)
- Due dates and reminders
- Categories/tags for organization
- Shared task lists (multi-user)
- Rich text descriptions (Markdown)
- File attachments via separate storage
- Recurring tasks
- Task dependencies
- Search and filtering
- Export to JSON/CSV
- Dark/light theme toggle

**Why Terminal.Gui over Textual?**

1. **Language Consistency:** Same C# as entire project
2. **Code Reuse:** Share Forebay.Core, no FFI needed
3. **Single Binary:** No Python runtime required
4. **Cross-Platform:** Works on Linux, macOS, Windows
5. **Microsoft-Backed:** Actively maintained
6. **Integration:** Natural extension of existing CLI

**Alternative Considered: Python + Textual**

Pros:
- More mature TUI framework
- Better documentation
- Rich widget library

Cons:
- Different language (Python vs C#)
- Can't reuse Forebay.Core directly
- Requires Python runtime
- Separate binary/distribution
- FFI complexity for Forebay API

**Decision: Use C# + Terminal.Gui for language consistency and code reuse.**
