from js import Response, Headers
import json
import time

def get_timestamp_ms():
    return int(time.time() * 1000)

async def handle_health(request, env):
    """Health check endpoint"""
    data = {
        "status": "ok",
        "worker": "python",
        "timestamp": get_timestamp_ms()
    }

    headers = Headers.new({"content-type": "application/json"}.items())
    return Response.new(json.dumps(data), headers=headers)

async def handle_kv_write(request, env):
    """KV write endpoint"""
    start = get_timestamp_ms()

    try:
        body = await request.json()
        key = body.get("key")
        value = body.get("value")

        if not key or value is None:
            headers = Headers.new({"content-type": "application/json"}.items())
            error = json.dumps({"error": "Missing key or value"})
            return Response.new(error, status=400, headers=headers)

        # Write to KV
        await env.VIABILITY_KV.put(key, json.dumps(value))

        duration = get_timestamp_ms() - start

        data = {
            "success": True,
            "key": key,
            "duration_ms": duration
        }

        headers = Headers.new({"content-type": "application/json"}.items())
        return Response.new(json.dumps(data), headers=headers)

    except Exception as e:
        headers = Headers.new({"content-type": "application/json"}.items())
        error = json.dumps({"error": str(e)})
        return Response.new(error, status=500, headers=headers)

async def handle_kv_read(request, env, key):
    """KV read endpoint"""
    start = get_timestamp_ms()

    try:
        value = await env.VIABILITY_KV.get(key)

        if value is None:
            headers = Headers.new({"content-type": "application/json"}.items())
            error = json.dumps({"error": "Key not found"})
            return Response.new(error, status=404, headers=headers)

        duration = get_timestamp_ms() - start

        data = {
            "key": key,
            "value": json.loads(value),
            "duration_ms": duration
        }

        headers = Headers.new({"content-type": "application/json"}.items())
        return Response.new(json.dumps(data), headers=headers)

    except Exception as e:
        headers = Headers.new({"content-type": "application/json"}.items())
        error = json.dumps({"error": str(e)})
        return Response.new(error, status=500, headers=headers)

async def handle_queue_push(request, env, queue_name):
    """Queue push endpoint (simulated with KV)"""
    start = get_timestamp_ms()

    try:
        body = await request.json()
        item = body.get("item")

        if item is None:
            headers = Headers.new({"content-type": "application/json"}.items())
            error = json.dumps({"error": "Missing item"})
            return Response.new(error, status=400, headers=headers)

        # Get existing queue or create new
        key = f"queue:{queue_name}"
        existing = await env.VIABILITY_KV.get(key)

        if existing:
            queue = json.loads(existing)
        else:
            queue = []

        # Append item
        queue.append({
            "item": item,
            "timestamp": get_timestamp_ms()
        })

        # Save back
        await env.VIABILITY_KV.put(key, json.dumps(queue))

        duration = get_timestamp_ms() - start

        data = {
            "success": True,
            "queue": queue_name,
            "length": len(queue),
            "duration_ms": duration
        }

        headers = Headers.new({"content-type": "application/json"}.items())
        return Response.new(json.dumps(data), headers=headers)

    except Exception as e:
        headers = Headers.new({"content-type": "application/json"}.items())
        error = json.dumps({"error": str(e)})
        return Response.new(error, status=500, headers=headers)

async def handle_queue_pull(request, env, queue_name):
    """Queue pull endpoint (atomic read + delete)"""
    start = get_timestamp_ms()

    try:
        key = f"queue:{queue_name}"
        existing = await env.VIABILITY_KV.get(key)

        if not existing:
            headers = Headers.new({"content-type": "application/json"}.items())
            error = json.dumps({"error": "Queue not found or empty"})
            return Response.new(error, status=404, headers=headers)

        queue = json.loads(existing)

        if len(queue) == 0:
            headers = Headers.new({"content-type": "application/json"}.items())
            error = json.dumps({"error": "Queue empty"})
            return Response.new(error, status=404, headers=headers)

        # Remove first item
        item_data = queue.pop(0)

        # Save updated queue
        await env.VIABILITY_KV.put(key, json.dumps(queue))

        duration = get_timestamp_ms() - start

        data = {
            "item": item_data["item"],
            "timestamp": item_data["timestamp"],
            "remaining": len(queue),
            "duration_ms": duration
        }

        headers = Headers.new({"content-type": "application/json"}.items())
        return Response.new(json.dumps(data), headers=headers)

    except Exception as e:
        headers = Headers.new({"content-type": "application/json"}.items())
        error = json.dumps({"error": str(e)})
        return Response.new(error, status=500, headers=headers)

async def on_fetch(request, env):
    """Main request handler"""
    url = request.url
    method = request.method

    # Parse path
    from urllib.parse import urlparse
    parsed = urlparse(url)
    path = parsed.path

    # Route requests
    if path == "/health" and method == "GET":
        return await handle_health(request, env)

    elif path == "/kv-write" and method == "POST":
        return await handle_kv_write(request, env)

    elif path.startswith("/kv-read/") and method == "GET":
        key = path.split("/kv-read/")[1]
        return await handle_kv_read(request, env, key)

    elif path.startswith("/queue-push/") and method == "POST":
        queue_name = path.split("/queue-push/")[1]
        return await handle_queue_push(request, env, queue_name)

    elif path.startswith("/queue-pull/") and method == "GET":
        queue_name = path.split("/queue-pull/")[1]
        return await handle_queue_pull(request, env, queue_name)

    else:
        headers = Headers.new({"content-type": "application/json"}.items())
        error = json.dumps({"error": "Not found"})
        return Response.new(error, status=404, headers=headers)
