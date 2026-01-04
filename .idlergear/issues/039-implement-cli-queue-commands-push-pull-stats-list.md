---
id: 39
title: Implement CLI queue commands (push, pull, stats, list, delete)
state: open
created: '2026-01-04T09:23:20.557218Z'
labels:
- cli
- queue
- csharp
priority: high
---
Implement all queue operation commands for the Forebay CLI.

**Commands to Implement:**

### 1. `forebay push <queue> [message]`

**Usage:**
```bash
forebay push work/tasks "Process this item"
echo "data" | forebay push work/tasks
cat file.json | forebay push work/tasks
```

**Implementation:**
- Read message from argument or stdin
- Call Worker `POST /queues/{queue}/push`
- Display: "Pushed to {queue} (length: {n})"
- Exit code 0 on success, 1 on error

**Options:**
- `--json` - Validate message is valid JSON before sending

### 2. `forebay pull <queue>`

**Usage:**
```bash
forebay pull work/tasks
forebay pull work/tasks | jq .
```

**Implementation:**
- Call Worker `POST /queues/{queue}/pull`
- Print payload to stdout (raw JSON)
- Exit code 0 if item retrieved, 1 if empty queue
- No extra output (just the payload for piping)

**Options:**
- `--pretty` - Pretty-print JSON output

### 3. `forebay stats <queue>`

**Usage:**
```bash
forebay stats work/tasks
```

**Implementation:**
- Call Worker `GET /queues/{queue}/stats`
- Display formatted statistics:
  ```
  Queue: work/tasks
  Length: 5 items
  Oldest: 2 hours ago
  Newest: 30 seconds ago
  Size: 1.2 KB
  ```

**Options:**
- `--json` - Output raw JSON instead

### 4. `forebay list`

**Usage:**
```bash
forebay list
```

**Implementation:**
- Call Worker `GET /queues`
- Display table of all queues:
  ```
  QUEUE           LENGTH  OLDEST
  work/tasks      5       2h ago
  notifications   0       -
  ```

**Options:**
- `--json` - Output raw JSON instead

### 5. `forebay delete <queue>`

**Usage:**
```bash
forebay delete work/tasks
```

**Implementation:**
- Prompt for confirmation: "Delete queue {queue}? (y/N)"
- Call Worker `DELETE /queues/{queue}`
- Display: "Deleted {queue} ({n} items removed)"

**Options:**
- `--force` or `-f` - Skip confirmation prompt

**Common Features (All Commands):**
- Require authentication (check config file)
- Show helpful error messages
- Support `--worker-url` override
- Proper exit codes
- Handle network errors gracefully

**Acceptance Criteria:**
- [ ] All 5 commands implemented
- [ ] Stdin/stdout piping works for push/pull
- [ ] Authentication required and checked
- [ ] Error messages are helpful
- [ ] Exit codes correct (0 = success, 1 = error)
- [ ] JSON output option works where applicable
- [ ] Confirmation prompt on destructive operations
- [ ] Manual testing completed for all commands

**Testing:**
- Create test script that exercises all commands
- Test piping workflows
- Test error scenarios (empty queue, no auth, etc.)
