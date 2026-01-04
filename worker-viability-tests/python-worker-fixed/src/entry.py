from workers import WorkerEntrypoint, Response
import json


# Modern API class-based handler
class Default(WorkerEntrypoint):
    async def fetch(self, request):
        return await handle_request(request, self.env)


# Legacy API function-based handler (fallback)
async def on_fetch(request, env):
    return await handle_request(request, env)


# Shared request handling logic
async def handle_request(request, env):
    """Shared request handler for both APIs"""
    # Simple URL parsing
    path = request.url.split('/', 3)[3] if '/' in request.url.split('://', 1)[1] else ''
    path = '/' + path if not path.startswith('/') else path

    method = request.method

    # Health check
    if path == '/health' and method == 'GET':
        import time
        data = {
            "status": "ok",
            "worker": "python-fixed",
            "timestamp": int(time.time() * 1000)
        }
        return Response.json(data)

    # KV write
    elif path == '/kv-write' and method == 'POST':
        import time
        start = time.time()

        try:
            body = await request.json()
            key = body.get("key")
            value = body.get("value")

            if not key or value is None:
                return Response.json({"error": "Missing key or value"}, status=400)

            await env.VIABILITY_KV.put(key, json.dumps(value))

            duration = int((time.time() - start) * 1000)
            return Response.json({
                "success": True,
                "key": key,
                "duration_ms": duration
            })
        except Exception as e:
            return Response.json({"error": str(e)}, status=500)

    # KV read
    elif path.startswith('/kv-read/') and method == 'GET':
        import time
        start = time.time()

        try:
            key = path.split('/kv-read/', 1)[1]
            value = await env.VIABILITY_KV.get(key)

            if value is None:
                return Response.json({"error": "Key not found"}, status=404)

            duration = int((time.time() - start) * 1000)
            return Response.json({
                "key": key,
                "value": json.loads(value),
                "duration_ms": duration
            })
        except Exception as e:
            return Response.json({"error": str(e)}, status=500)

    # Queue push
    elif path.startswith('/queue-push/') and method == 'POST':
        import time
        start = time.time()

        try:
            queue_name = path.split('/queue-push/', 1)[1]
            body = await request.json()
            item = body.get("item")

            if item is None:
                return Response.json({"error": "Missing item"}, status=400)

            key = f"queue:{queue_name}"
            existing = await env.VIABILITY_KV.get(key)
            queue = json.loads(existing) if existing else []

            queue.append({
                "item": item,
                "timestamp": int(time.time() * 1000)
            })

            await env.VIABILITY_KV.put(key, json.dumps(queue))

            duration = int((time.time() - start) * 1000)
            return Response.json({
                "success": True,
                "queue": queue_name,
                "length": len(queue),
                "duration_ms": duration
            })
        except Exception as e:
            return Response.json({"error": str(e)}, status=500)

    # Queue pull
    elif path.startswith('/queue-pull/') and method == 'GET':
        import time
        start = time.time()

        try:
            queue_name = path.split('/queue-pull/', 1)[1]
            key = f"queue:{queue_name}"
            existing = await env.VIABILITY_KV.get(key)

            if not existing:
                return Response.json({"error": "Queue not found or empty"}, status=404)

            queue = json.loads(existing)

            if len(queue) == 0:
                return Response.json({"error": "Queue empty"}, status=404)

            item_data = queue.pop(0)
            await env.VIABILITY_KV.put(key, json.dumps(queue))

            duration = int((time.time() - start) * 1000)
            return Response.json({
                "item": item_data["item"],
                "timestamp": item_data["timestamp"],
                "remaining": len(queue),
                "duration_ms": duration
            })
        except Exception as e:
            return Response.json({"error": str(e)}, status=500)

    # Not found
    return Response.json({"error": "Not found"}, status=404)
