use serde::{Deserialize, Serialize};

// Authentication Models

#[derive(Debug, Serialize)]
pub struct WhoamiResponse {
    pub email: String,
}

// Queue Models

#[derive(Debug, Deserialize)]
pub struct PushRequest {
    pub payload: serde_json::Value,
}

#[derive(Debug, Serialize)]
pub struct PushResponse {
    pub success: bool,
    pub queue: String,
    pub length: usize,
    pub item_id: String,
    pub timestamp: i64,
}

#[derive(Debug, Serialize)]
pub struct PullResponse {
    pub item_id: String,
    pub payload: serde_json::Value,
    pub timestamp: i64,
    pub remaining: usize,
}

#[derive(Debug, Serialize)]
pub struct StatsResponse {
    pub queue: String,
    pub length: usize,
    pub oldest_timestamp: Option<i64>,
    pub newest_timestamp: Option<i64>,
    pub total_size_bytes: usize,
}

#[derive(Debug, Serialize)]
pub struct DeleteResponse {
    pub success: bool,
    pub queue: String,
    pub deleted_items: usize,
}

#[derive(Debug, Serialize)]
pub struct QueueInfo {
    pub name: String,
    pub length: usize,
    pub oldest_timestamp: Option<i64>,
}

#[derive(Debug, Serialize)]
pub struct ListQueuesResponse {
    pub queues: Vec<QueueInfo>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct QueueItem {
    pub id: String,
    pub payload: serde_json::Value,
    pub timestamp: i64,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct QueueData {
    pub items: Vec<QueueItem>,
    pub metadata: QueueMetadata,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct QueueMetadata {
    pub created_at: i64,
    pub total_pushed: u64,
    pub total_pulled: u64,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_whoami_response_serialization() {
        let response = WhoamiResponse {
            email: "test@example.com".to_string(),
        };

        let json = serde_json::to_string(&response).unwrap();
        assert!(json.contains("\"email\":\"test@example.com\""));
    }

    #[test]
    fn test_queue_item_serialization() {
        let item = QueueItem {
            id: "item-123".to_string(),
            payload: serde_json::json!({"test": "data"}),
            timestamp: 1609459200000,
        };

        let json = serde_json::to_string(&item).unwrap();
        assert!(json.contains("\"id\":\"item-123\""));
        assert!(json.contains("\"test\":\"data\""));
    }

    #[test]
    fn test_push_request_deserialization() {
        let json = r#"{"payload":{"message":"hello"}}"#;
        let req: PushRequest = serde_json::from_str(json).unwrap();
        assert_eq!(req.payload["message"], "hello");
    }

    #[test]
    fn test_queue_metadata() {
        let metadata = QueueMetadata {
            created_at: 1609459200000,
            total_pushed: 100,
            total_pulled: 50,
        };

        let json = serde_json::to_string(&metadata).unwrap();
        assert!(json.contains("\"total_pushed\":100"));
        assert!(json.contains("\"total_pulled\":50"));
    }

    #[test]
    fn test_queue_data_roundtrip() {
        let data = QueueData {
            items: vec![
                QueueItem {
                    id: "1".to_string(),
                    payload: serde_json::json!({"test": 1}),
                    timestamp: 123,
                },
            ],
            metadata: QueueMetadata {
                created_at: 100,
                total_pushed: 1,
                total_pulled: 0,
            },
        };

        let json = serde_json::to_string(&data).unwrap();
        let parsed: QueueData = serde_json::from_str(&json).unwrap();
        
        assert_eq!(parsed.items.len(), 1);
        assert_eq!(parsed.items[0].id, "1");
        assert_eq!(parsed.metadata.total_pushed, 1);
    }
}
