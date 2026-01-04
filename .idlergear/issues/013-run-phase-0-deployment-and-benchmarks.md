---
id: 13
title: Run Phase 0 deployment and benchmarks
state: open
created: '2026-01-04T06:28:43.506131Z'
labels:
- phase-0
- testing
- automation
priority: high
---
Execute the automated test harness to collect performance data:

**Steps:**
1. Navigate to `worker-viability-tests/test-harness/`
2. Install Python dependencies: `pip install -r requirements.txt`
3. Run deployment: `python3 deploy.py`
   - Deploys all 3 workers to Cloudflare
   - Records build times and URLs
4. Run benchmarks: `python3 benchmark.py`
   - Tests cold start, latency, KV ops
   - Takes ~10-15 minutes
5. Generate report: `python3 report.py`
   - Creates `RESULTS.md` with comparison
   - Provides language recommendation

**Prerequisites:**
- Wrangler CLI installed and authenticated
- Node.js (for TypeScript worker)
- Rust toolchain (for Rust worker)
- Python 3.8+ (for scripts)

**Output:** `RESULTS.md` with data-driven language recommendation

**Depends on:** Cloudflare account access via wrangler
