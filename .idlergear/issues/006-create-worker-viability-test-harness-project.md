---
id: 6
title: Create worker viability test harness project structure
state: closed
created: '2026-01-04T06:21:25.024812Z'
labels:
- phase-0
- testing
priority: high
---
✅ **COMPLETED**

Created complete test harness structure:
- `typescript-worker/` - Hono-based TypeScript implementation
- `rust-worker/` - worker crate Rust implementation  
- `python-worker/` - Pyodide Python implementation
- `test-harness/` - Automation scripts (deploy, benchmark, report)

All workers implement identical REST API:
- GET /health
- POST /kv-write
- GET /kv-read/:key
- POST /queue-push/:name
- GET /queue-pull/:name

Committed to git: 15 files, 1526 lines

**Next:** Run deployment and benchmarks
