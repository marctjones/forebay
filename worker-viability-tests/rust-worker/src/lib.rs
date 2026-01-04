use worker::*;
use serde::{Deserialize, Serialize};

#[derive(Deserialize)]
struct KvWriteRequest {
    key: String,
    value: serde_json::Value,
}

#[derive(Deserialize)]
struct QueuePushRequest {
    item: serde_json::Value,
}

#[derive(Serialize, Deserialize)]
struct QueueItem {
    item: serde_json::Value,
    timestamp: u64,
}

#[derive(Serialize)]
struct HealthResponse {
    status: String,
    worker: String,
    timestamp: u64,
}

#[derive(Serialize)]
struct KvWriteResponse {
    success: bool,
    key: String,
    duration_ms: u128,
}

#[derive(Serialize)]
struct KvReadResponse {
    key: String,
    value: serde_json::Value,
    duration_ms: u128,
}

#[derive(Serialize)]
struct QueuePushResponse {
    success: bool,
    queue: String,
    length: usize,
    duration_ms: u128,
}

#[derive(Serialize)]
struct QueuePullResponse {
    item: serde_json::Value,
    timestamp: u64,
    remaining: usize,
    duration_ms: u128,
}

#[derive(Serialize)]
struct ErrorResponse {
    error: String,
}

fn get_timestamp_ms() -> u64 {
    Date::now().as_millis()
}

#[event(fetch)]
async fn main(req: Request, env: Env, _ctx: Context) -> Result<Response> {
    let router = Router::new();

    router
        .get_async("/health", |_, _| async move {
            Response::from_json(&HealthResponse {
                status: "ok".to_string(),
                worker: "rust".to_string(),
                timestamp: get_timestamp_ms(),
            })
        })
        .post_async("/kv-write", |mut req, ctx| async move {
            let start = get_timestamp_ms();

            let data: KvWriteRequest = match req.json().await {
                Ok(d) => d,
                Err(_) => return Response::error("Invalid JSON", 400),
            };

            let kv = ctx.kv("VIABILITY_KV")?;
            kv.put(&data.key, data.value.to_string())?.execute().await?;

            let duration = get_timestamp_ms() - start;

            Response::from_json(&KvWriteResponse {
                success: true,
                key: data.key,
                duration_ms: duration as u128,
            })
        })
        .get_async("/kv-read/:key", |_, ctx| async move {
            let start = get_timestamp_ms();

            let key = match ctx.param("key") {
                Some(k) => k,
                None => return Response::error("Missing key", 400),
            };

            let kv = ctx.kv("VIABILITY_KV")?;
            let value = match kv.get(key).text().await? {
                Some(v) => v,
                None => return Response::error("Key not found", 404),
            };

            let duration = get_timestamp_ms() - start;

            let json_value: serde_json::Value = serde_json::from_str(&value)
                .unwrap_or(serde_json::Value::String(value));

            Response::from_json(&KvReadResponse {
                key: key.to_string(),
                value: json_value,
                duration_ms: duration as u128,
            })
        })
        .post_async("/queue-push/:name", |mut req, ctx| async move {
            let start = get_timestamp_ms();

            let queue_name = match ctx.param("name") {
                Some(n) => n,
                None => return Response::error("Missing queue name", 400),
            };

            let data: QueuePushRequest = match req.json().await {
                Ok(d) => d,
                Err(_) => return Response::error("Invalid JSON", 400),
            };

            let kv = ctx.kv("VIABILITY_KV")?;
            let key = format!("queue:{}", queue_name);

            // Get existing queue or create new
            let mut queue: Vec<QueueItem> = match kv.get(&key).text().await? {
                Some(v) => serde_json::from_str(&v).unwrap_or_else(|_| Vec::new()),
                None => Vec::new(),
            };

            // Append item
            queue.push(QueueItem {
                item: data.item,
                timestamp: get_timestamp_ms(),
            });

            // Save back
            let queue_json = serde_json::to_string(&queue)?;
            kv.put(&key, queue_json)?.execute().await?;

            let duration = get_timestamp_ms() - start;

            Response::from_json(&QueuePushResponse {
                success: true,
                queue: queue_name.to_string(),
                length: queue.len(),
                duration_ms: duration as u128,
            })
        })
        .get_async("/queue-pull/:name", |_, ctx| async move {
            let start = get_timestamp_ms();

            let queue_name = match ctx.param("name") {
                Some(n) => n,
                None => return Response::error("Missing queue name", 400),
            };

            let kv = ctx.kv("VIABILITY_KV")?;
            let key = format!("queue:{}", queue_name);

            // Get queue
            let mut queue: Vec<QueueItem> = match kv.get(&key).text().await? {
                Some(v) => serde_json::from_str(&v).unwrap_or_else(|_| Vec::new()),
                None => return Response::error("Queue not found or empty", 404),
            };

            if queue.is_empty() {
                return Response::error("Queue empty", 404);
            }

            // Remove first item
            let item = queue.remove(0);

            // Save updated queue
            let queue_json = serde_json::to_string(&queue)?;
            kv.put(&key, queue_json)?.execute().await?;

            let duration = get_timestamp_ms() - start;

            Response::from_json(&QueuePullResponse {
                item: item.item,
                timestamp: item.timestamp,
                remaining: queue.len(),
                duration_ms: duration as u128,
            })
        })
        .run(req, env)
        .await
}
