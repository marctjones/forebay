use crate::auth::validate_api_key;
use crate::error::ErrorResponse;
use crate::models::{
    DeleteStorageResponse, DocumentInfo, DocumentMetadata, GetResponse, ListDocumentsResponse,
    PutRequest, PutResponse, StoredDocument,
};
use chrono::Utc;
use worker::*;

const STORAGE_PREFIX: &str = "store:";
const STORAGE_INDEX_KEY: &str = "store:index";

// Helper function to add document to index
async fn add_document_to_index(kv: &kv::KvStore, key: &str) -> Result<()> {
    let mut documents: Vec<String> = match kv.get(STORAGE_INDEX_KEY).text().await? {
        Some(json) => serde_json::from_str(&json).unwrap_or_else(|_| vec![]),
        None => vec![],
    };

    // Add document if not already in index
    if !documents.contains(&key.to_string()) {
        documents.push(key.to_string());
        documents.sort(); // Keep sorted for consistent ordering
        let json = serde_json::to_string(&documents).unwrap();
        kv.put(STORAGE_INDEX_KEY, json)?.execute().await?;
    }

    Ok(())
}

// Helper function to remove document from index
async fn remove_document_from_index(kv: &kv::KvStore, key: &str) -> Result<()> {
    let mut documents: Vec<String> = match kv.get(STORAGE_INDEX_KEY).text().await? {
        Some(json) => serde_json::from_str(&json).unwrap_or_else(|_| vec![]),
        None => vec![],
    };

    // Remove document from index
    documents.retain(|d| d != key);
    let json = serde_json::to_string(&documents).unwrap();
    kv.put(STORAGE_INDEX_KEY, json)?.execute().await?;

    Ok(())
}

pub async fn handle_put(mut req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    // Extract key from path (wildcard parameter)
    let key = match ctx.param("key") {
        Some(k) => k.trim_start_matches('/'),
        None => {
            return ErrorResponse::bad_request("Missing document key").to_response(400);
        }
    };

    // Validate key (basic validation)
    if key.is_empty() || key.len() > 512 {
        return ErrorResponse::bad_request("Invalid document key").to_response(400);
    }

    // Parse request body
    let put_req: PutRequest = match req.json().await {
        Ok(r) => r,
        Err(_) => {
            return ErrorResponse::bad_request("Invalid request body").to_response(400);
        }
    };

    // Get KV store
    let kv = ctx.env.kv("QUEUES")?;
    let storage_key = format!("{}{}", STORAGE_PREFIX, key);

    // Check if document exists (for metadata)
    let now = Utc::now().timestamp_millis();
    let (created_at, updated_at) = match kv.get(&storage_key).text().await? {
        Some(json) => {
            // Document exists, preserve created_at
            match serde_json::from_str::<StoredDocument>(&json) {
                Ok(doc) => (doc.metadata.created_at, now),
                Err(_) => (now, now), // Corrupted data, treat as new
            }
        }
        None => (now, now), // New document
    };

    // Create document with metadata
    let size_bytes = serde_json::to_string(&put_req.content)?.len();
    let document = StoredDocument {
        content: put_req.content,
        metadata: DocumentMetadata {
            created_at,
            updated_at,
            size_bytes,
        },
    };

    // Save to KV
    kv.put(&storage_key, serde_json::to_string(&document)?)?
        .execute()
        .await?;

    // Add to index
    add_document_to_index(&kv, key).await?;

    let response = PutResponse {
        success: true,
        key: key.to_string(),
        size_bytes,
        created_at,
        updated_at,
    };

    Response::from_json(&response)
}

pub async fn handle_get(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    // Extract key from path (wildcard parameter)
    let key = match ctx.param("key") {
        Some(k) => k.trim_start_matches('/'),
        None => {
            return ErrorResponse::bad_request("Missing document key").to_response(400);
        }
    };

    // Get document from KV
    let kv = ctx.env.kv("QUEUES")?;
    let storage_key = format!("{}{}", STORAGE_PREFIX, key);

    let document = match kv.get(&storage_key).text().await? {
        Some(json) => match serde_json::from_str::<StoredDocument>(&json) {
            Ok(doc) => doc,
            Err(_) => {
                return ErrorResponse::internal_error("Failed to parse document")
                    .to_response(500);
            }
        },
        None => {
            return ErrorResponse::not_found("Document not found").to_response(404);
        }
    };

    let response = GetResponse {
        key: key.to_string(),
        content: document.content,
        created_at: document.metadata.created_at,
        updated_at: document.metadata.updated_at,
        size_bytes: document.metadata.size_bytes,
    };

    Response::from_json(&response)
}

pub async fn handle_delete_document(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    // Extract key from path (wildcard parameter)
    let key = match ctx.param("key") {
        Some(k) => k.trim_start_matches('/'),
        None => {
            return ErrorResponse::bad_request("Missing document key").to_response(400);
        }
    };

    // Delete document from KV
    let kv = ctx.env.kv("QUEUES")?;
    let storage_key = format!("{}{}", STORAGE_PREFIX, key);

    // Check if document exists before deletion
    let exists = kv.get(&storage_key).text().await?.is_some();
    if !exists {
        return ErrorResponse::not_found("Document not found").to_response(404);
    }

    // Delete the document
    kv.delete(&storage_key).await?;

    // Remove from index
    remove_document_from_index(&kv, key).await?;

    let response = DeleteStorageResponse {
        success: true,
        key: key.to_string(),
    };

    Response::from_json(&response)
}

pub async fn handle_list_documents(req: Request, ctx: RouteContext<()>) -> Result<Response> {
    // Verify authentication
    let _email = match validate_api_key(&req, &ctx.env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    // Get query parameters
    let url = req.url()?;
    let prefix = url
        .query_pairs()
        .find(|(k, _)| k == "prefix")
        .map(|(_, v)| v.to_string());

    let limit: usize = url
        .query_pairs()
        .find(|(k, _)| k == "limit")
        .and_then(|(_, v)| v.parse().ok())
        .unwrap_or(100);

    // Get document index
    let kv = ctx.env.kv("QUEUES")?;
    let document_keys: Vec<String> = match kv.get(STORAGE_INDEX_KEY).text().await? {
        Some(json) => serde_json::from_str(&json).unwrap_or_else(|_| vec![]),
        None => vec![],
    };

    // Filter by prefix if provided
    let filtered_keys: Vec<String> = if let Some(p) = prefix {
        document_keys
            .into_iter()
            .filter(|k| k.starts_with(&p))
            .collect()
    } else {
        document_keys
    };

    // Apply limit
    let limited_keys: Vec<String> = filtered_keys.into_iter().take(limit).collect();

    // Get metadata for each document
    let mut documents = vec![];
    for key in limited_keys {
        let storage_key = format!("{}{}", STORAGE_PREFIX, key);
        if let Some(json) = kv.get(&storage_key).text().await? {
            if let Ok(doc) = serde_json::from_str::<StoredDocument>(&json) {
                documents.push(DocumentInfo {
                    key,
                    size_bytes: doc.metadata.size_bytes,
                    created_at: doc.metadata.created_at,
                    updated_at: doc.metadata.updated_at,
                });
            }
        }
    }

    let count = documents.len();
    let response = ListDocumentsResponse { documents, count };

    Response::from_json(&response)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_storage_key_format() {
        let key = "my-document";
        let storage_key = format!("{}{}", STORAGE_PREFIX, key);
        assert_eq!(storage_key, "store:my-document");
    }

    #[test]
    fn test_document_metadata_creation() {
        let now = 1609459200000i64;
        let metadata = DocumentMetadata {
            created_at: now,
            updated_at: now,
            size_bytes: 1024,
        };

        assert_eq!(metadata.created_at, now);
        assert_eq!(metadata.updated_at, now);
        assert_eq!(metadata.size_bytes, 1024);
    }

    #[test]
    fn test_stored_document_serialization() {
        let document = StoredDocument {
            content: serde_json::json!({"message": "hello"}),
            metadata: DocumentMetadata {
                created_at: 1609459200000,
                updated_at: 1609459200000,
                size_bytes: 100,
            },
        };

        let json = serde_json::to_string(&document).unwrap();
        assert!(json.contains("\"message\":\"hello\""));
        assert!(json.contains("\"created_at\":1609459200000"));
    }

    #[test]
    fn test_document_key_validation() {
        let valid_key = "tasks/task-123";
        assert!(!valid_key.is_empty());
        assert!(valid_key.len() <= 512);

        let empty_key = "";
        assert!(empty_key.is_empty());

        let too_long = "a".repeat(513);
        assert!(too_long.len() > 512);
    }
}
