---
id: 60
title: Add stdin/stdout piping support for queue operations
state: open
created: '2026-01-04T23:56:12.067832Z'
labels:
- cli
- csharp
- enhancement
priority: high
---
Enable Unix-style piping for queue operations.

**Implementation:**
1. Detect if stdin is redirected (not a TTY)
2. Read message from stdin if available
3. Otherwise use command-line argument
4. Output payload to stdout (for pull command)
5. Output metadata to stderr

**Examples:**
```bash
echo '{"task":"process"}' | forebay push work/tasks
forebay pull work/tasks | jq .
```

**Acceptance Criteria:**
- [ ] Stdin detection works
- [ ] Pipe input supported
- [ ] Stdout contains payload only
- [ ] Stderr contains metadata
- [ ] Works in shell scripts
