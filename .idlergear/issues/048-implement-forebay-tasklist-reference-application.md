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

## 1. Ubuntu Terminal (CLI) - Priority: HIGH
**Technology:** Existing C# CLI + task subcommands

**Commands:**
```bash
forebay task add "Buy groceries"
forebay task list
forebay task complete <id>
forebay task delete <id>
forebay task sync
```

**Implementation:**
- Add `task` command group to existing CLI
- Store tasks in local SQLite database
- Sync via Forebay queue on add/complete/delete
- Display formatted table in terminal

**File:** `client/Forebay.Cli/Commands/TaskCommands.cs`

## 2. GNOME Desktop (GTK4) - Priority: MEDIUM
**Technology:** Avalonia or GTK4 with C#

**UI:**
- Main window with task list
- Add task input box
- Checkboxes to complete tasks
- Delete button per task
- Sync indicator
- System tray icon (optional)

**Implementation:**
- Use Avalonia for cross-platform consistency
- Same sync logic as CLI
- Background sync every 30 seconds
- Desktop notifications for new tasks

**Project:** `client/Forebay.Desktop/` (new)

## 3. Windows 11 Desktop (WinUI 3) - Priority: MEDIUM
**Technology:** Avalonia (same codebase as GNOME)

**UI:**
- Windows 11 native look and feel
- Fluent Design System
- Acrylic background
- Toast notifications

**Implementation:**
- Same Avalonia codebase as GNOME Desktop
- Platform-specific styling
- Windows-specific features (taskbar, notifications)

**Shared:** `client/Forebay.Desktop/` with platform renderers

## 4. Windows 11 Terminal - Priority: MEDIUM
**Technology:** Same C# CLI as Ubuntu

**Implementation:**
- Same CLI binary works on Windows
- PowerShell-friendly output
- Windows Terminal integration
- Color support via ANSI

**Testing:**
- Verify CLI works in Windows Terminal
- Test PowerShell piping
- Verify color output

## 5. Android App - Priority: LOW (Future)
**Technology:** Avalonia Mobile or .NET MAUI

**UI:**
- Material Design
- Swipe to complete/delete
- Pull to refresh/sync
- Offline support
- Push notifications

**Implementation:**
- Avalonia.Mobile for consistency with Desktop
- Or .NET MAUI for better mobile integration
- Local SQLite storage
- Background sync service
- Android notification integration

**Project:** `client/Forebay.Mobile/` (new)

**Technical Architecture:**

### Shared Core Library (`Forebay.Tasks`)
```
Forebay.Tasks/
├── Models/
│   ├── Task.cs
│   └── TaskList.cs
├── Storage/
│   └── SqliteTaskStore.cs
├── Sync/
│   ├── TaskSyncService.cs
│   └── ConflictResolver.cs
└── TaskManager.cs
```

**Sync Strategy:**
1. Local changes pushed to queue immediately
2. Pull from queue on app start and periodically
3. Last-write-wins conflict resolution
4. Tombstone records for deletions

**Offline Support:**
- All operations work offline
- Queue operations buffer when offline
- Sync when connection restored

**Implementation Phases:**

### Phase 1: CLI (Ubuntu + Windows Terminal)
- [ ] Create `Forebay.Tasks` shared library
- [ ] Implement SQLite storage
- [ ] Implement sync service
- [ ] Add task commands to CLI
- [ ] Test on Ubuntu and Windows

### Phase 2: Desktop (GNOME + Windows 11)
- [ ] Create `Forebay.Desktop` Avalonia project
- [ ] Design UI with Avalonia
- [ ] Integrate `Forebay.Tasks` library
- [ ] Implement background sync
- [ ] Test on Linux (GNOME) and Windows 11

### Phase 3: Android (Future)
- [ ] Create `Forebay.Mobile` project
- [ ] Evaluate Avalonia.Mobile vs MAUI
- [ ] Implement mobile UI
- [ ] Add background sync service
- [ ] Implement push notifications
- [ ] Test on Android devices

**Acceptance Criteria:**

### CLI
- [ ] All task commands work
- [ ] Tasks sync across devices
- [ ] Offline support functional
- [ ] Works on Ubuntu and Windows

### Desktop
- [ ] Single codebase for Linux and Windows
- [ ] Tasks sync in real-time
- [ ] Background sync every 30 seconds
- [ ] Desktop notifications work
- [ ] Offline mode graceful

### Android (Future)
- [ ] Material Design UI
- [ ] Background sync works
- [ ] Push notifications work
- [ ] Offline support complete

**Documentation:**
- User guide for TaskList app
- Architecture documentation
- Sync conflict resolution explanation
- Example for developers

**Dependencies:**
- SQLite (Microsoft.Data.Sqlite)
- Avalonia (Desktop/Mobile)
- System.Reactive (for sync)

**Testing:**
- Unit tests for sync logic
- Integration tests for multi-device sync
- UI tests for Desktop apps
- Manual testing on all platforms

**Future Enhancements:**
- Task priorities
- Due dates
- Categories/tags
- Shared task lists
- Rich text descriptions
- File attachments
