#!/usr/bin/env python3
import requests
import time
import statistics

url = "https://python-modern-viability-test.marc-t-jones.workers.dev"

print("Testing modern Python Worker...")
print(f"URL: {url}\n")

# Test cold start
print("Cold start test...")
time.sleep(30)  # Wait for cold
start = time.time()
r = requests.get(f"{url}/health")
cold_start = (time.time() - start) * 1000
print(f"Cold start: {cold_start:.2f}ms")
print(f"Response: {r.json()}\n")

# Test warm performance
print("Warm performance (20 requests)...")
latencies = []
for i in range(20):
    start = time.time()
    r = requests.get(f"{url}/health")
    latency = (time.time() - start) * 1000
    latencies.append(latency)
    time.sleep(0.1)

print(f"Average: {statistics.mean(latencies):.2f}ms")
print(f"P50: {statistics.median(latencies):.2f}ms")
print(f"P95: {sorted(latencies)[int(len(latencies)*0.95)]:.2f}ms")
print(f"Min: {min(latencies):.2f}ms")
print(f"Max: {max(latencies):.2f}ms")
