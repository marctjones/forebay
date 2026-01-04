---
id: 11
title: Build automated benchmark suite
state: closed
created: '2026-01-04T06:21:27.148963Z'
labels:
- phase-0
- testing
- automation
priority: high
---
Create Python scripts to benchmark all three Workers.

**Tests to implement:**

1. **Health Check Latency** (100 requests)
   - Measure p50, p95, p99
   - First request = cold start
   - Subsequent = warm performance

2. **KV Write Performance** (100 writes)
   - Different payload sizes (1KB, 10KB, 100KB)
   - Measure latency distribution

3. **KV Read Performance** (100 reads)
   - After writing data
   - Measure latency distribution

4. **Queue Operations** (50 push + 50 pull)
   - Simulate queue behavior
   - Measure atomic read+delete performance

**Output format:**
```python
{
  "typescript": {
    "cold_start_ms": 125,
    "health_p50_ms": 45,
    "health_p95_ms": 78,
    "kv_write_avg_ms": 45,
    ...
  },
  "rust": { ... },
  "python": { ... }
}
```

**Requirements:**
- Use `requests` library
- Calculate percentiles (numpy or pure Python)
- Run tests sequentially (not concurrent) for fair comparison
- Wait between cold start tests
- Save results to JSON
