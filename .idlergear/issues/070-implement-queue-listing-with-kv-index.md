---
id: 70
title: Implement queue listing with KV index
state: closed
created: '2026-01-05T02:20:39.578861Z'
labels:
- enhancement
priority: medium
---
The list queues endpoint currently returns empty because KV doesn't support native listing.

**Current Implementation (queue.rs:244-260):**
- Returns empty list with TODO comment
- Comment notes: "KV doesn't have native list operation"
- Suggests maintaining separate index

**Solution:**
Maintain a queue index in KV:
- Key: `queues:index` 
- Value: JSON array of queue names
- Update index on queue create/delete
- Return from index on list operation

**Files to modify:**
- worker/src/queue.rs - handle_list(), handle_push(), handle_delete()
- Consider adding cleanup for deleted queues from index

**Testing:**
- Push to new queue → appears in list
- Delete queue → removed from list
- Multiple queues → all listed correctly

**Priority:** Medium (nice-to-have, doesn't block core functionality)
