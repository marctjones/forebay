---
id: 52
title: Implement Forebay Notes - Cross-platform Markdown note-taking app
state: open
created: '2026-01-04T14:50:59.827555Z'
labels:
- notes
- markdown
- vim
- cross-platform
- reference-app
priority: medium
---
Create a powerful, cross-platform note-taking application that combines vim's editing efficiency, Notepad's simplicity, and OneNote's organization - all synced via Forebay queues.

**Goal:** Replace vim, Notepad, and OneNote with a single, synced, Markdown-based note app.

**Target Platforms:**
- Ubuntu Terminal (TUI with vim-like editing)
- Windows Terminal (same TUI)
- GNOME Desktop (GUI)
- Windows 11 Desktop (GUI)
- Android (future)

**Core Features:**

### 1. Vim-Inspired Editing
**Modal editing** (Normal, Insert, Visual modes)
**Essential vim commands:**
- `i` - Insert mode
- `Esc` - Normal mode
- `dd` - Delete line
- `yy` - Yank (copy) line
- `p` - Paste
- `/` - Search forward
- `n` - Next search result
- `:w` - Save
- `:q` - Quit
- `hjkl` - Navigation (optional, arrows also work)

**Beyond basic vim:**
- Markdown syntax highlighting
- Live preview toggle
- Fuzzy file search (like `:FZF`)

### 2. Notepad-Style Simplicity
**Clean, distraction-free writing**
**Quick operations:**
- New note (Ctrl+N)
- Open note (Ctrl+O)
- Save (Ctrl+S)
- Find/Replace (Ctrl+F / Ctrl+H)
- Cut/Copy/Paste (Ctrl+X/C/V)
- Undo/Redo (Ctrl+Z/Y)

**Plain text focus:**
- No complex formatting UI
- Markdown for structure
- Fast startup
- Minimal chrome

### 3. OneNote-Style Organization
**Hierarchical structure:**
```
Notebooks/
├── Work/
│   ├── Projects/
│   │   ├── Forebay.md
│   │   └── Client-Project.md
│   ├── Meetings/
│   └── Ideas/
├── Personal/
│   ├── Journal/
│   └── Recipes/
└── Learning/
    ├── Rust.md
    └── Cloudflare-Workers.md
```

**Wiki-style linking:**
- `[[Note Name]]` - Link to other notes
- Backlinks - See what links to current note
- Graph view - Visualize note connections

**Organization features:**
- Nested folders (notebooks → sections → pages)
- Tags (`#tag`)
- Quick switcher (Ctrl+P)
- Recent notes
- Favorites/pinned notes

### 4. Forebay Sync
**Real-time sync across devices:**
- Notes sync via Forebay queues
- Conflict resolution (last-write-wins with merge)
- Offline editing
- Attachment support (images, PDFs via separate storage)

**Queue pattern:**
```
Queue: notes/{user_email}/updates
Message: {
  "type": "create|update|delete|move",
  "note_id": "uuid",
  "path": "Work/Projects/Forebay.md",
  "content": "# Forebay\n...",
  "timestamp": 1609459200000
}
```

**Platform Implementations:**

## 1. Terminal App (TUI) - Priority: HIGH

**Technology:** C# + Terminal.Gui with vim keybindings

**UI Layout:**
```
┌─ Forebay Notes ─────────────────────────────────────┐
│ Work/Projects/Forebay.md                   [Normal] │
├─────────────┬───────────────────────────────────────┤
│ 📁 Work     │ # Forebay                             │
│   📁 Projects│                                       │
│     📄 Forebay│ A cross-platform message queue      │
│     📄 Client │ transport system.                   │
│   📁 Meetings│                                       │
│ 📁 Personal │ ## Architecture                       │
│ 📁 Learning │                                       │
│             │ - **Worker**: Rust on Cloudflare     │
│             │ - **Client**: C# (.NET 9)            │
│             │                                       │
│             │ See [[Architecture]] for details.    │
│             │                                       │
├─────────────┴───────────────────────────────────────┤
│ :w to save | Ctrl+P search | / find | Esc normal  │
└──────────────────────────────────────────────────────┘
```

**Features:**
- Split panes (folder tree + editor)
- Vim modal editing
- Markdown syntax highlighting
- Live preview mode (toggle with `:preview`)
- Fuzzy file search
- Full-text search across notes
- Tag browser

**Vim Commands Supported:**
```
Normal Mode:
  i, I, a, A     - Insert mode (at cursor, start, after, end)
  dd             - Delete line
  yy             - Yank line
  p, P           - Paste after/before
  /pattern       - Search
  n, N           - Next/previous match
  gg, G          - Top/bottom of file
  :w             - Save
  :q             - Quit
  :wq            - Save and quit
  
Insert Mode:
  Esc            - Back to normal
  Ctrl+C         - Back to normal (alternative)
  
Visual Mode:
  v              - Start visual
  V              - Visual line
  y              - Yank selection
  d              - Delete selection
```

**Additional Commands:**
```
:open <name>     - Open note
:new             - New note
:tree            - Toggle folder tree
:preview         - Toggle markdown preview
:search <query>  - Full-text search
:tags            - Show tag browser
:recent          - Recent notes
:sync            - Manual sync
```

## 2. Desktop App (GUI) - Priority: MEDIUM

**Technology:** Avalonia with rich editor

**UI Design:**
```
┌─────────────────────────────────────────────────────┐
│ 🗒️ Forebay Notes              ─ ☐ ✕  [🔄] [⚙]      │
├──────────┬──────────┬───────────────────────────────┤
│ 📚 Notebooks│ 📋 Notes  │ # Forebay                   │
│          │          │                               │
│ 📁 Work  │ Forebay  │ A cross-platform message     │
│   Projects│ Client   │ queue transport system.      │
│   Meetings│ Ideas    │                               │
│          │          │ ## Architecture              │
│ 📁 Personal│        │                               │
│   Journal│          │ - **Worker**: Rust           │
│          │          │ - **Client**: C#             │
│ 📁 Learning│        │                               │
│   Rust   │          │ [[Architecture Details]]     │
│          │          │                               │
│          │          │                               │
├──────────┴──────────┴───────────────────────────────┤
│ 🏷️ Tags: #work #project | Modified 2m ago | Synced │
└─────────────────────────────────────────────────────┘
```

**Features:**
- Three-pane layout (notebooks → notes → editor)
- Rich Markdown editor with live preview
- Syntax highlighting
- Split view (edit + preview side-by-side)
- Drag-and-drop organization
- Click wiki links to navigate
- Search with highlights
- Tag cloud/list
- Note graph visualization

**Editor Features:**
- Optional vim mode (toggle on/off)
- Rich text toolbar for Markdown (bold, italic, lists, links)
- Image paste support
- Auto-save
- Version history (via Forebay sync log)

## 3. Android App - Priority: LOW (Future)

**Technology:** .NET MAUI

**Features:**
- Mobile-optimized editor
- Voice notes → transcription → markdown
- Camera → OCR → markdown
- Offline editing
- Background sync
- Share to Forebay Notes

**Technical Architecture:**

### Shared Core Library (`Forebay.Notes`)

```
client/Forebay.Notes/
├── Models/
│   ├── Note.cs
│   ├── Notebook.cs
│   ├── Tag.cs
│   └── Link.cs
├── Storage/
│   ├── INoteStore.cs
│   └── SqliteNoteStore.cs
├── Sync/
│   ├── NoteSyncService.cs
│   ├── ConflictResolver.cs
│   └── MergeStrategy.cs
├── Parsing/
│   ├── MarkdownParser.cs
│   └── WikiLinkExtractor.cs
├── Search/
│   ├── FullTextSearch.cs
│   └── FuzzyMatcher.cs
└── NoteManager.cs
```

### Note Model

```csharp
public class Note
{
    public Guid Id { get; set; }
    public string Path { get; set; }        // Work/Projects/Forebay.md
    public string Title { get; set; }       // Extracted from first # heading
    public string Content { get; set; }     // Markdown content
    public List<string> Tags { get; set; }  // Extracted from #tags
    public List<string> Links { get; set; } // Extracted [[links]]
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsFavorite { get; set; }
}

public class Notebook
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }  // For nested notebooks
    public List<Notebook> Children { get; set; }
    public List<Note> Notes { get; set; }
}
```

### Wiki Link Parsing

```csharp
public class WikiLinkExtractor
{
    // Extract [[Note Name]] style links
    public List<string> ExtractLinks(string markdown)
    {
        var regex = new Regex(@"\[\[([^\]]+)\]\]");
        return regex.Matches(markdown)
            .Select(m => m.Groups[1].Value)
            .ToList();
    }
    
    // Replace [[Note Name]] with clickable links
    public string RenderLinks(string markdown, Dictionary<string, string> noteMap)
    {
        return Regex.Replace(markdown, @"\[\[([^\]]+)\]\]", match => {
            var noteName = match.Groups[1].Value;
            if (noteMap.TryGetValue(noteName, out var noteId)) {
                return $"[{noteName}](note://{noteId})";
            }
            return match.Value; // Keep [[]] if note not found
        });
    }
}
```

### Vim Mode Implementation (Terminal)

```csharp
public class VimEditor
{
    private EditorMode _mode = EditorMode.Normal;
    private StringBuilder _content;
    private int _cursorLine;
    private int _cursorCol;
    private string _yankBuffer;
    
    public void HandleKey(KeyEvent key)
    {
        switch (_mode)
        {
            case EditorMode.Normal:
                HandleNormalMode(key);
                break;
            case EditorMode.Insert:
                HandleInsertMode(key);
                break;
            case EditorMode.Visual:
                HandleVisualMode(key);
                break;
        }
    }
    
    private void HandleNormalMode(KeyEvent key)
    {
        switch (key.KeyChar)
        {
            case 'i': _mode = EditorMode.Insert; break;
            case 'd': 
                if (_lastKey == 'd') DeleteLine();
                break;
            case 'y':
                if (_lastKey == 'y') YankLine();
                break;
            case 'p': Paste(); break;
            case '/': StartSearch(); break;
            // ... etc
        }
    }
}
```

### Full-Text Search

```csharp
public class FullTextSearch
{
    private readonly SqliteNoteStore _store;
    
    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        // Use SQLite FTS5 for fast full-text search
        var sql = @"
            SELECT note_id, path, title, 
                   snippet(notes_fts, 2, '<mark>', '</mark>', '...', 32) as snippet,
                   rank
            FROM notes_fts
            WHERE notes_fts MATCH @query
            ORDER BY rank
            LIMIT 50
        ";
        
        // Return results with highlighted snippets
    }
}
```

### Conflict Resolution Strategy

```csharp
public class ConflictResolver
{
    public Note ResolveConflict(Note local, Note remote)
    {
        // For notes, we can do smart merging
        if (local.UpdatedAt > remote.UpdatedAt)
        {
            // Local is newer
            return local;
        }
        else if (remote.UpdatedAt > local.UpdatedAt)
        {
            // Remote is newer
            return remote;
        }
        else
        {
            // Same timestamp - merge content
            return MergeContent(local, remote);
        }
    }
    
    private Note MergeContent(Note local, Note remote)
    {
        // Simple line-based merge (like git)
        var localLines = local.Content.Split('\n');
        var remoteLines = remote.Content.Split('\n');
        
        // Use longest common subsequence or 3-way merge
        // For simplicity, could just append both changes
        // Or use a proper diff/merge library
        
        return new Note
        {
            Id = local.Id,
            Path = local.Path,
            Content = MergeAlgorithm(localLines, remoteLines),
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}
```

**Storage Schema (SQLite):**

```sql
CREATE TABLE notes (
    id TEXT PRIMARY KEY,
    path TEXT NOT NULL UNIQUE,
    title TEXT,
    content TEXT NOT NULL,
    created_at INTEGER NOT NULL,
    updated_at INTEGER NOT NULL,
    is_deleted INTEGER DEFAULT 0,
    is_favorite INTEGER DEFAULT 0
);

CREATE TABLE tags (
    id INTEGER PRIMARY KEY,
    note_id TEXT NOT NULL,
    tag TEXT NOT NULL,
    FOREIGN KEY (note_id) REFERENCES notes(id)
);

CREATE TABLE links (
    id INTEGER PRIMARY KEY,
    from_note_id TEXT NOT NULL,
    to_note_path TEXT NOT NULL,
    FOREIGN KEY (from_note_id) REFERENCES notes(id)
);

-- Full-text search
CREATE VIRTUAL TABLE notes_fts USING fts5(
    path, title, content, tags,
    content='notes',
    content_rowid='rowid'
);

-- Triggers to keep FTS in sync
CREATE TRIGGER notes_ai AFTER INSERT ON notes BEGIN
    INSERT INTO notes_fts(rowid, path, title, content)
    VALUES (new.rowid, new.path, new.title, new.content);
END;
```

**Implementation Phases:**

### Phase 1: Core Library (3-4 days)
- [ ] Create Forebay.Notes library
- [ ] Implement Note/Notebook models
- [ ] SQLite storage with FTS
- [ ] Markdown parsing
- [ ] Wiki link extraction
- [ ] Tag extraction
- [ ] Full-text search
- [ ] Sync service
- [ ] Conflict resolution with merging

### Phase 2: Terminal TUI (4-5 days)
- [ ] Terminal.Gui interface
- [ ] Vim modal editing
- [ ] Folder tree view
- [ ] Markdown syntax highlighting
- [ ] Search functionality
- [ ] Quick switcher (Ctrl+P)
- [ ] Tag browser
- [ ] Live preview mode
- [ ] Keyboard shortcuts

### Phase 3: Desktop GUI (5-6 days)
- [ ] Avalonia application
- [ ] Three-pane layout
- [ ] Rich Markdown editor
- [ ] Live preview pane
- [ ] Wiki link navigation
- [ ] Tag cloud/browser
- [ ] Note graph visualization
- [ ] Search with highlighting
- [ ] Drag-drop organization
- [ ] Optional vim mode

**Dependencies:**

### Terminal App
```xml
<PackageReference Include="Terminal.Gui" Version="1.17.1" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
<PackageReference Include="Markdig" Version="0.37.0" />  <!-- Markdown parser -->
```

### Desktop App
```xml
<PackageReference Include="Avalonia" Version="11.0.0" />
<PackageReference Include="AvaloniaEdit" Version="11.0.0" />  <!-- Code editor -->
<PackageReference Include="Markdig" Version="0.37.0" />
<PackageReference Include="Markdown.Avalonia" Version="11.0.0" />  <!-- Markdown rendering -->
```

**Testing:**

### Unit Tests
- Markdown parsing
- Wiki link extraction
- Tag extraction
- Search indexing
- Conflict resolution
- Vim command parsing

### Integration Tests
- Note creation/update/delete
- Cross-device sync
- Wiki link navigation
- Full-text search
- Conflict scenarios

### Manual Tests
- Vim commands feel natural
- Search is fast (even with 1000s of notes)
- Sync works across devices
- No data loss on conflicts

**Acceptance Criteria:**

### Terminal App
- [ ] Vim modal editing works smoothly
- [ ] All essential vim commands supported
- [ ] Markdown syntax highlighting
- [ ] Fast full-text search (<100ms for 1000 notes)
- [ ] Wiki links clickable
- [ ] Tags extracted and browsable
- [ ] Background sync every 30s
- [ ] Works on Ubuntu and Windows Terminal

### Desktop App
- [ ] Three-pane layout intuitive
- [ ] Live preview updates in real-time
- [ ] Wiki link navigation smooth
- [ ] Note graph useful and fast
- [ ] Search highlights matches
- [ ] Optional vim mode toggle
- [ ] Cross-platform (Linux + Windows)

**Why This Beats vim/Notepad/OneNote:**

**vs vim:**
- ✅ Same modal editing
- ✅ + Organization (notebooks, tags, links)
- ✅ + Sync across devices
- ✅ + Markdown preview
- ✅ + Full-text search
- ✅ + Wiki linking

**vs Notepad:**
- ✅ Same simplicity for quick notes
- ✅ + Powerful editing (vim mode)
- ✅ + Organization
- ✅ + Sync
- ✅ + Search

**vs OneNote:**
- ✅ Same organization
- ✅ Same wiki linking
- ✅ + Faster (plain text vs rich format)
- ✅ + vim editing efficiency
- ✅ + Markdown (portable, version controllable)
- ✅ + Open format (not proprietary)
- ✅ + Cross-platform terminal support
- ✅ + Scriptable (CLI commands)

**Additional Features (Future):**

- Git integration (commit notes to git)
- Export to PDF/HTML
- Daily note template (journaling)
- Todo integration (parse `- [ ]` checkboxes)
- Mermaid diagram support
- Code snippet syntax highlighting
- End-to-end encryption option
- Web clipper browser extension
- Email to note (forward@forebay-notes.app)

This would be the ultimate note-taking app for developers and power users!
