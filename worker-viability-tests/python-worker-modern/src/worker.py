from workers import WorkerEntrypoint, Response
import json


class Default(WorkerEntrypoint):
    """Modern Python Worker using WorkerEntrypoint pattern"""

    async def fetch(self, request):
        """Main request handler"""
        url = request.url
        method = request.method

        # Parse path from URL
        path = url.split('/', 3)[3] if len(url.split('/', 3)) > 3 else ''
        path = '/' + path

        # Health check
        if path == '/health' and method == 'GET':
            return await self.handle_health()

        # KV write
        elif path == '/kv-write' and method == 'POST':
            return await self.handle_kv_write(request)

        # KV read
        elif path.startswith('/kv-read/') and method == 'GET':
            key = path.split('/kv-read/', 1)[1]
            return await self.handle_kv_read(key)

        # Queue push
        elif path.startswith('/queue-push/') and method == 'POST':
            queue_name = path.split('/queue-push/', 1)[1]
            return await self.handle_queue_push(request, queue_name)

        # Queue pull
        elif path.startswith('/queue-pull/') and method == 'GET':
            queue_name = path.split('/queue-pull/', 1)[1]
            return await self.handle_queue_pull(queue_name)

        # Not found
        return Response(
            json.dumps({"error": "Not found"}),
            status=404,
            headers={"content-type": "application/json"}
        )

    async def handle_health(self):
        """Health check endpoint"""
        import time
        data = {
            "status": "ok",
            "worker": "python-modern",
            "timestamp": int(time.time() * 1000)
        }
        return Response(
            json.dumps(data),
            headers={"content-type": "application/json"}
        )

    async def handle_kv_write(self, request):
        """KV write endpoint"""
        import time
        start = time.time()

        try:
            body = await request.json()
            key = body.get("key")
            value = body.get("value")

            if not key or value is None:
                return Response(
                    json.dumps({"error": "Missing key or value"}),
                    status=400,
                    headers={"content-type": "application/json"}
                )

            # Write to KV using env binding
            await self.env.VIABILITY_KV.put(key, json.dumps(value))

            duration = int((time.time() - start) * 1000)

            return Response(
                json.dumps({
                    "success": True,
                    "key": key,
                    "duration_ms": duration
                }),
                headers={"content-type": "application/json"}
            )

        except Exception as e:
            return Response(
                json.dumps({"error": str(e)}),
                status=500,
                headers={"content-type": "application/json"}
            )

    async def handle_kv_read(self, key):
        """KV read endpoint"""
        import time
        start = time.time()

        try:
            # Read from KV using env binding
            value = await self.env.VIABILITY_KV.get(key)

            if value is None:
                return Response(
                    json.dumps({"error": "Key not found"}),
                    status=404,
                    headers={"content-type": "application/json"}
                )

            duration = int((time.time() - start) * 1000)

            return Response(
                json.dumps({
                    "key": key,
                    "value": json.loads(value),
                    "duration_ms": duration
                }),
                headers={"content-type": "application/json"}
            )

        except Exception as e:
            return Response(
                json.dumps({"error": str(e)}),
                status=500,
                headers={"content-type": "application/json"}
            )

    async def handle_queue_push(self, request, queue_name):
        """Queue push endpoint"""
        import time
        start = time.time()

        try:
            body = await request.json()
            item = body.get("item")

            if item is None:
                return Response(
                    json.dumps({"error": "Missing item"}),
                    status=400,
                    headers={"content-type": "application/json"}
                )

            # Get existing queue
            key = f"queue:{queue_name}"
            existing = await self.env.VIABILITY_KV.get(key)

            queue = json.loads(existing) if existing else []

            # Append item
            queue.append({
                "item": item,
                "timestamp": int(time.time() * 1000)
            })

            # Save back
            await self.env.VIABILITY_KV.put(key, json.dumps(queue))

            duration = int((time.time() - start) * 1000)

            return Response(
                json.dumps({
                    "success": True,
                    "queue": queue_name,
                    "length": len(queue),
                    "duration_ms": duration
                }),
                headers={"content-type": "application/json"}
            )

        except Exception as e:
            return Response(
                json.dumps({"error": str(e)}),
                status=500,
                headers={"content-type": "application/json"}
            )

    async def handle_queue_pull(self, queue_name):
        """Queue pull endpoint (atomic read + delete)"""
        import time
        start = time.time()

        try:
            key = f"queue:{queue_name}"
            existing = await self.env.VIABILITY_KV.get(key)

            if not existing:
                return Response(
                    json.dumps({"error": "Queue not found or empty"}),
                    status=404,
                    headers={"content-type": "application/json"}
                )

            queue = json.loads(existing)

            if len(queue) == 0:
                return Response(
                    json.dumps({"error": "Queue empty"}),
                    status=404,
                    headers={"content-type": "application/json"}
                )

            # Remove first item
            item_data = queue.pop(0)

            # Save updated queue
            await self.env.VIABILITY_KV.put(key, json.dumps(queue))

            duration = int((time.time() - start) * 1000)

            return Response(
                json.dumps({
                    "item": item_data["item"],
                    "timestamp": item_data["timestamp"],
                    "remaining": len(queue),
                    "duration_ms": duration
                }),
                headers={"content-type": "application/json"}
            )

        except Exception as e:
            return Response(
                json.dumps({"error": str(e)}),
                status=500,
                headers={"content-type": "application/json"}
            )
