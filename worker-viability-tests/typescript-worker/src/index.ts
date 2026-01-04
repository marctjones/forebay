import { Hono } from 'hono';

type Bindings = {
  VIABILITY_KV: KVNamespace;
};

const app = new Hono<{ Bindings: Bindings }>();

// Health check
app.get('/health', (c) => {
  return c.json({
    status: 'ok',
    worker: 'typescript',
    timestamp: Date.now(),
  });
});

// KV Write
app.post('/kv-write', async (c) => {
  const start = Date.now();
  const { key, value } = await c.req.json();

  if (!key || value === undefined) {
    return c.json({ error: 'Missing key or value' }, 400);
  }

  await c.env.VIABILITY_KV.put(key, JSON.stringify(value));
  const duration = Date.now() - start;

  return c.json({
    success: true,
    key,
    duration_ms: duration,
  });
});

// KV Read
app.get('/kv-read/:key', async (c) => {
  const start = Date.now();
  const key = c.req.param('key');

  const value = await c.env.VIABILITY_KV.get(key);
  const duration = Date.now() - start;

  if (value === null) {
    return c.json({ error: 'Key not found' }, 404);
  }

  return c.json({
    key,
    value: JSON.parse(value),
    duration_ms: duration,
  });
});

// Queue Push (simulated with KV array)
app.post('/queue-push/:name', async (c) => {
  const start = Date.now();
  const queueName = c.req.param('name');
  const { item } = await c.req.json();

  if (item === undefined) {
    return c.json({ error: 'Missing item' }, 400);
  }

  // Get existing queue or create new
  const existingData = await c.env.VIABILITY_KV.get(`queue:${queueName}`);
  const queue = existingData ? JSON.parse(existingData) : [];

  // Append item
  queue.push({
    item,
    timestamp: Date.now(),
  });

  // Save back
  await c.env.VIABILITY_KV.put(`queue:${queueName}`, JSON.stringify(queue));
  const duration = Date.now() - start;

  return c.json({
    success: true,
    queue: queueName,
    length: queue.length,
    duration_ms: duration,
  });
});

// Queue Pull (atomic read + delete first item)
app.get('/queue-pull/:name', async (c) => {
  const start = Date.now();
  const queueName = c.req.param('name');

  // Get queue
  const existingData = await c.env.VIABILITY_KV.get(`queue:${queueName}`);

  if (!existingData) {
    return c.json({ error: 'Queue not found or empty' }, 404);
  }

  const queue = JSON.parse(existingData);

  if (queue.length === 0) {
    return c.json({ error: 'Queue empty' }, 404);
  }

  // Remove first item
  const item = queue.shift();

  // Save updated queue
  await c.env.VIABILITY_KV.put(`queue:${queueName}`, JSON.stringify(queue));
  const duration = Date.now() - start;

  return c.json({
    item: item.item,
    timestamp: item.timestamp,
    remaining: queue.length,
    duration_ms: duration,
  });
});

export default app;
