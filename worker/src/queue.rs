use crate::auth::validate_api_key;
use crate::error::ErrorResponse;
use crate::models::{
    DeleteResponse, ListQueuesResponse, PullResponse, PushRequest, PushResponse, QueueData,
    QueueItem, QueueMetadata, StatsResponse,
};
use chrono::Utc;
use uuid::Uuid;
use worker::*;

const MAX_QUEUE_NAME_LENGTH: usize = 64;
const QUEUE_PREFIX: &str = "queue:";
const QUEUE_INDEX_KEY: &str = "queues:index";

// Helper function to add queue to index
async fn add_queue_to_index(kv: &kv::KvStore, queue_name: &str) -> Result<()> {
    let mut queues: Vec<String> = match kv.get(QUEUE_INDEX_KEY).text().await? {
        Some(json) => serde_json::from_str(&json).unwrap_or_else(|_| vec![]),
        None => vec![],
    };

    // Add queue if not already in index
    if !queues.contains(&queue_name.to_string()) {
        queues.push(queue_name.to_string());
        queues.sort(); // Keep sorted for consistent ordering
        let json = serde_json::to_string(&queues).unwrap();
        kv.put(QUEUE_INDEX_KEY, json)?.execute().await?;
    }

    Ok(())
}

// Helper function to remove queue from index
async fn remove_queue_from_index(kv: &kv::KvStore, queue_name: &str) -> Result<()> {
    let mut queues: Vec<String> = match kv.get(QUEUE_INDEX_KEY).text().await? {
        Some(json) => serde_json::from_str(&json).unwrap_or_else(|_| vec![]),
        None => vec![],
    };

    // Remove queue from index
    queues.retain(|q| q != queue_name);
    let json = serde_json::to_string(&queues).unwrap();
    kv.put(QUEUE_INDEX_KEY, json)?.execute().await?;

    Ok(())
}

pub async fn handle_push(mut req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    // Extract queue name from path
    let queue_name = match ctx.param("queue") {
        Some(name) => name,
        None => {
            return ErrorResponse::bad_request("Missing queue name").to_response(400);
        }
    };

    // Validate queue name
    if queue_name.is_empty() || queue_name.len() > MAX_QUEUE_NAME_LENGTH {
        return ErrorResponse::bad_request("Invalid queue name").to_response(400);
    }

    // Parse request body
    let push_req: PushRequest = match req.json().await {
        Ok(r) => r,
        Err(_) => {
            return ErrorResponse::bad_request("Invalid request body").to_response(400);
        }
    };

    // Get or create queue data
    let kv = ctx.env.kv("QUEUES")?;
    let key = format!("{}{}", QUEUE_PREFIX, queue_name);

    let mut queue_data = match kv.get(&key).text().await? {
        Some(json) => serde_json::from_str::<QueueData>(&json).unwrap_or_else(|_| QueueData {
            items: vec![],
            metadata: QueueMetadata {
                created_at: Utc::now().timestamp_millis(),
                total_pushed: 0,
                total_pulled: 0,
            },
        }),
        None => QueueData {
            items: vec![],
            metadata: QueueMetadata {
                created_at: Utc::now().timestamp_millis(),
                total_pushed: 0,
                total_pulled: 0,
            },
        },
    };

    // Create new queue item
    let item_id = Uuid::new_v4().to_string();
    let timestamp = Utc::now().timestamp_millis();
    let item = QueueItem {
        id: item_id.clone(),
        payload: push_req.payload,
        timestamp,
    };

    // Add item to queue
    queue_data.items.push(item);
    queue_data.metadata.total_pushed += 1;

    // Save to KV
    kv.put(&key, serde_json::to_string(&queue_data)?)?
        .execute()
        .await?;

    // Add queue to index (if it's a new queue, this is idempotent)
    add_queue_to_index(&kv, queue_name).await?;

    let response = PushResponse {
        success: true,
        queue: queue_name.to_string(),
        length: queue_data.items.len(),
        item_id,
        timestamp,
    };

    Response::from_json(&response)
}

pub async fn handle_pull(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    // Extract queue name
    let queue_name = match ctx.param("queue") {
        Some(name) => name,
        None => {
            return ErrorResponse::bad_request("Missing queue name").to_response(400);
        }
    };

    // Get queue data
    let kv = ctx.env.kv("QUEUES")?;
    let key = format!("{}{}", QUEUE_PREFIX, queue_name);

    let mut queue_data = match kv.get(&key).text().await? {
        Some(json) => match serde_json::from_str::<QueueData>(&json) {
            Ok(data) => data,
            Err(_) => {
                return ErrorResponse::internal_error("Failed to parse queue data")
                    .to_response(500);
            }
        },
        None => {
            return ErrorResponse::not_found("Queue not found").to_response(404);
        }
    };

    // Check if queue is empty
    if queue_data.items.is_empty() {
        return ErrorResponse::not_found("Queue is empty").to_response(404);
    }

    // Pull first item (FIFO)
    let item = queue_data.items.remove(0);
    queue_data.metadata.total_pulled += 1;

    // Save updated queue
    kv.put(&key, serde_json::to_string(&queue_data)?)?
        .execute()
        .await?;

    let response = PullResponse {
        item_id: item.id,
        payload: item.payload,
        timestamp: item.timestamp,
        remaining: queue_data.items.len(),
    };

    Response::from_json(&response)
}

pub async fn handle_stats(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(s) => s,
        Err(e) => return e,
    };

    // Extract queue name
    let queue_name = match ctx.param("queue") {
        Some(name) => name,
        None => {
            return ErrorResponse::bad_request("Missing queue name").to_response(400);
        }
    };

    // Get queue data
    let kv = ctx.env.kv("QUEUES")?;
    let key = format!("{}{}", QUEUE_PREFIX, queue_name);

    let queue_data = match kv.get(&key).text().await? {
        Some(json) => match serde_json::from_str::<QueueData>(&json) {
            Ok(data) => data,
            Err(_) => {
                return ErrorResponse::internal_error("Failed to parse queue data")
                    .to_response(500);
            }
        },
        None => {
            // Return empty stats for non-existent queue
            let response = StatsResponse {
                queue: queue_name.to_string(),
                length: 0,
                oldest_timestamp: None,
                newest_timestamp: None,
                total_size_bytes: 0,
            };
            return Response::from_json(&response);
        }
    };

    let oldest_timestamp = queue_data.items.first().map(|item| item.timestamp);
    let newest_timestamp = queue_data.items.last().map(|item| item.timestamp);
    let total_size_bytes = serde_json::to_string(&queue_data)?.len();

    let response = StatsResponse {
        queue: queue_name.to_string(),
        length: queue_data.items.len(),
        oldest_timestamp,
        newest_timestamp,
        total_size_bytes,
    };

    Response::from_json(&response)
}

pub async fn handle_delete(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(s) => s,
        Err(e) => return e,
    };

    // Extract queue name
    let queue_name = match ctx.param("queue") {
        Some(name) => name,
        None => {
            return ErrorResponse::bad_request("Missing queue name").to_response(400);
        }
    };

    // Get queue data to count deleted items
    let kv = ctx.env.kv("QUEUES")?;
    let key = format!("{}{}", QUEUE_PREFIX, queue_name);

    let deleted_count = match kv.get(&key).text().await? {
        Some(json) => match serde_json::from_str::<QueueData>(&json) {
            Ok(data) => data.items.len(),
            Err(_) => 0,
        },
        None => 0,
    };

    // Delete queue
    kv.delete(&key).await?;

    // Remove queue from index
    remove_queue_from_index(&kv, queue_name).await?;

    let response = DeleteResponse {
        success: true,
        queue: queue_name.to_string(),
        deleted_items: deleted_count,
    };

    Response::from_json(&response)
}

pub async fn handle_list(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(s) => s,
        Err(e) => return e,
    };

    // Get queue index
    let kv = ctx.env.kv("QUEUES")?;
    let queue_names: Vec<String> = match kv.get(QUEUE_INDEX_KEY).text().await? {
        Some(json) => serde_json::from_str(&json).unwrap_or_else(|_| vec![]),
        None => vec![],
    };

    // Get stats for each queue
    let mut queues = vec![];
    for queue_name in queue_names {
        let key = format!("{}{}", QUEUE_PREFIX, queue_name);
        if let Some(json) = kv.get(&key).text().await? {
            if let Ok(queue_data) = serde_json::from_str::<QueueData>(&json) {
                let oldest_timestamp = queue_data.items.first().map(|item| item.timestamp);
                queues.push(crate::models::QueueInfo {
                    name: queue_name,
                    length: queue_data.items.len(),
                    oldest_timestamp,
                });
            }
        }
    }

    let response = ListQueuesResponse { queues };

    Response::from_json(&response)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_queue_key_format() {
        let queue_name = "test-queue";
        let key = format!("{}{}", QUEUE_PREFIX, queue_name);
        assert_eq!(key, "queue:test-queue");
    }

    #[test]
    fn test_max_queue_name_length() {
        assert_eq!(MAX_QUEUE_NAME_LENGTH, 64);
    }

    #[test]
    fn test_queue_data_initialization() {
        let now = 1609459200000i64;
        let queue_data = QueueData {
            items: vec![],
            metadata: QueueMetadata {
                created_at: now,
                total_pushed: 0,
                total_pulled: 0,
            },
        };

        assert_eq!(queue_data.items.len(), 0);
        assert_eq!(queue_data.metadata.created_at, now);
        assert_eq!(queue_data.metadata.total_pushed, 0);
        assert_eq!(queue_data.metadata.total_pulled, 0);
    }

    #[test]
    fn test_queue_item_creation() {
        let item = QueueItem {
            id: "test-id".to_string(),
            payload: serde_json::json!({"message": "hello"}),
            timestamp: 1609459200000,
        };

        assert_eq!(item.id, "test-id");
        assert_eq!(item.timestamp, 1609459200000);
        assert_eq!(item.payload["message"], "hello");
    }

    #[test]
    fn test_queue_fifo_order() {
        let mut items = vec![
            QueueItem {
                id: "1".to_string(),
                payload: serde_json::json!({"order": 1}),
                timestamp: 100,
            },
            QueueItem {
                id: "2".to_string(),
                payload: serde_json::json!({"order": 2}),
                timestamp: 200,
            },
            QueueItem {
                id: "3".to_string(),
                payload: serde_json::json!({"order": 3}),
                timestamp: 300,
            },
        ];

        let first = items.remove(0);
        assert_eq!(first.id, "1");
        assert_eq!(items.len(), 2);

        let second = items.remove(0);
        assert_eq!(second.id, "2");
        assert_eq!(items.len(), 1);
    }

    #[test]
    fn test_queue_metadata_counters() {
        let mut metadata = QueueMetadata {
            created_at: 1609459200000,
            total_pushed: 0,
            total_pulled: 0,
        };

        metadata.total_pushed += 1;
        metadata.total_pushed += 1;
        metadata.total_pulled += 1;

        assert_eq!(metadata.total_pushed, 2);
        assert_eq!(metadata.total_pulled, 1);
    }

    #[test]
    fn test_queue_name_validation() {
        let valid_name = "my-queue";
        assert!(!valid_name.is_empty());
        assert!(valid_name.len() <= MAX_QUEUE_NAME_LENGTH);

        let too_long = "a".repeat(MAX_QUEUE_NAME_LENGTH + 1);
        assert!(too_long.len() > MAX_QUEUE_NAME_LENGTH);

        let empty = "";
        assert!(empty.is_empty());
    }
}
