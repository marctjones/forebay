---
id: 10
title: Build automated deployment script
state: open
created: '2026-01-04T06:21:26.719859Z'
labels:
- phase-0
- testing
- automation
priority: high
---
Create Python script to deploy all test Workers using Cloudflare MCP.

**Requirements:**
- Create KV namespace for testing (shared by all Workers)
- Deploy TypeScript Worker to typescript-test.workers.dev
- Deploy Rust Worker to rust-test.workers.dev
- Deploy Python Worker to python-test.workers.dev
- Capture build times and bundle sizes
- Handle deployment failures gracefully
- Output deployment status and URLs

**Use Cloudflare MCP tools:**
- Check available MCP tools for Worker deployment
- Use KV namespace creation tools
- Automate wrangler deployment

**Output:**
```
Deploying Workers...
✓ KV namespace created: viability_test_kv
✓ TypeScript Worker: 2.1s build, 850KB, https://typescript-test.workers.dev
✓ Rust Worker: 8.3s build, 420KB, https://rust-test.workers.dev
✓ Python Worker: 3.2s build, 1.2MB, https://python-test.workers.dev
```
