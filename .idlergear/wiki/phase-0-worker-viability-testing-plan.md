---
id: 1
title: Phase 0 - Worker Viability Testing Plan
created: '2026-01-04T06:21:28.018662Z'
updated: '2026-01-04T06:21:28.018692Z'
---
# Phase 0: Worker Viability Testing Plan

## Objective
Empirically determine the best language for Cloudflare Worker implementation by building and benchmarking identical Workers in TypeScript, Rust, and Python.

## Why This Phase Exists
We need to make a **data-driven decision** about the Worker language before committing to the full architecture. This avoids:
- Choosing based on assumptions
- Rewriting later due to performance issues
- Picking the "wrong" language for our use case

## Test Methodology

### Workers to Build
Each Worker implements identical REST API:

```
GET  /health              - Basic health check
POST /kv-write            - Write key-value to KV
GET  /kv-read/:key        - Read from KV
POST /queue-push/:name    - Push item to queue (array in KV)
GET  /queue-pull/:name    - Atomic read+delete first item
```

### Metrics to Measure

**Deployment Metrics:**
- Build time
- Bundle size
- Deployment success/failures

**Performance Metrics:**
- Cold start latency (first request)
- Warm request latency (p50, p95, p99)
- KV write performance
- KV read performance
- Queue operation performance

**Developer Experience (Qualitative):**
- Documentation quality
- Ease of setup
- Ecosystem maturity
- Debugging experience

## Test Harness Architecture

```
worker-viability-tests/
│
├── typescript-worker/      # Baseline (best Cloudflare support)
│   ├── src/index.ts
│   ├── package.json
│   └── wrangler.toml
│
├── rust-worker/            # Performance contender
│   ├── src/lib.rs
│   ├── Cargo.toml
│   └── wrangler.toml
│
├── python-worker/          # User's preferred language
│   ├── src/worker.py
│   ├── requirements.txt
│   └── wrangler.toml
│
└── test-harness/           # Orchestration (Python)
    ├── deploy.py           # Deploy via Cloudflare MCP
    ├── benchmark.py        # Run performance tests
    ├── test_kv.py         # Test KV operations
    ├── test_queue_ops.py  # Test queue simulations
    ├── report.py          # Generate markdown report
    ├── results.json       # Benchmark data
    └── requirements.txt
```

## Test Execution Flow

1. **Setup Phase**
   - Create shared KV namespace via MCP
   - Build each Worker
   - Record build times and bundle sizes

2. **Deployment Phase**
   - Deploy TypeScript Worker
   - Deploy Rust Worker
   - Deploy Python Worker
   - Record deployment success/failures

3. **Benchmark Phase**
   - Cold start test (wait, then single request)
   - Warm latency test (100 requests)
   - KV write test (100 writes, various sizes)
   - KV read test (100 reads)
   - Queue push test (50 pushes)
   - Queue pull test (50 pulls)

4. **Analysis Phase**
   - Calculate statistics (p50, p95, p99)
   - Generate comparison tables
   - Produce recommendation

5. **Report Generation**
   - Create markdown report
   - Save to `RESULTS.md`
   - Present findings for decision

## Expected Outcomes

### Scenario 1: TypeScript Wins
**If:** TypeScript has acceptable performance and best DX
**Then:** Use TypeScript for Worker, C# for client
**Tradeoff:** Can't share types between Worker and client

### Scenario 2: Rust Wins
**If:** Rust has significantly better performance
**Then:** Decide between:
- Option A: Rust Worker + C# client (better GUI)
- Option B: Rust for everything (shared code, steeper learning)
**Tradeoff:** More complex if split, steeper learning curve if all-Rust

### Scenario 3: Python Wins
**If:** Python has acceptable performance and good DX
**Then:** Python Worker + C# client
**Tradeoff:** Python beta status, potential future issues

### Scenario 4: Python Fails
**If:** Python has poor performance or major limitations
**Then:** Eliminate Python, choose between TypeScript and Rust
**Tradeoff:** Can't use user's strongest language for Worker

## Success Criteria

**This phase succeeds when:**
- All three Workers are built and deployed
- Benchmarks are run successfully
- Report is generated with clear data
- Language recommendation is made with justification
- Decision is approved to proceed to Phase 1

## Risks & Mitigations

**Risk:** Python Worker fails to deploy
**Mitigation:** Document failure, recommend TypeScript/Rust

**Risk:** All options perform poorly
**Mitigation:** Investigate Cloudflare limitations, may need different architecture

**Risk:** Performance is similar across all
**Mitigation:** Choose based on developer experience and ecosystem

**Risk:** Test harness is complex to build
**Mitigation:** Keep tests simple, focus on key metrics

## Next Phase Dependency

Phase 1 (Architecture & Project Setup) **cannot begin** until:
1. Phase 0 tests are complete
2. Language recommendation is made
3. Decision is approved

This ensures we don't waste time building on the wrong foundation.

## Timeline Estimate

- Worker implementation: ~2-4 hours
- Test harness: ~2-3 hours
- Benchmarking: ~1 hour (mostly automated)
- Analysis & report: ~1 hour

**Total:** ~6-9 hours of development time

## Deliverables

1. Three working Workers (TypeScript, Rust, Python)
2. Automated test harness
3. Benchmark results (JSON)
4. Comparison report (Markdown)
5. Language recommendation with justification
