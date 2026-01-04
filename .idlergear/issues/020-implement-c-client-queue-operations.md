---
id: 20
title: Implement C# client queue operations
state: open
created: '2026-01-04T08:32:50.337145Z'
labels:
- phase-1
- csharp
- client
- queue
priority: high
---
Implement queue commands in the C# CLI.

**Commands to implement:**

1. **forebay push <queue> [message]**
   - Read message from argument or stdin
   - POST to /q/:name/push
   - Display success + queue length
   - Support piping: `echo "test" | forebay push work`

2. **forebay pull <queue>**
   - GET from /q/:name/pull
   - Print item to stdout
   - Exit code 1 if queue empty

3. **forebay subscribe <queue>**
   - Long-polling loop to /q/:name/subscribe
   - Print items as they arrive
   - Ctrl+C to exit

4. **forebay stats <queue>**
   - GET from /q/:name/stats
   - Display queue length, oldest item age

5. **forebay list**
   - GET from /queues
   - Display all queues with lengths

**Implementation:**
- Use CloudflareClient class for HTTP calls
- Handle authentication (read token from config)
- Proper error messages for 401, 404, etc.
- Support --worker-url flag for custom Worker URL

**Tests:**
- Unit tests for commands
- Integration tests against real Worker (manual)
