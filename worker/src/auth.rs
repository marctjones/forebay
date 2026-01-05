use crate::error::ErrorResponse;
use crate::models::{WhoamiResponse};
use worker::*;

// No login endpoint needed with static API keys
// Users simply use their API key in the Authorization header

pub async fn handle_whoami(req: Request, env: Env) -> Result<Response> {
    // Validate API key and get user email
    let email = match validate_api_key(&req, &env).await {
        Ok(email) => email,
        Err(e) => return e,
    };

    let response = WhoamiResponse {
        email,
    };

    Response::from_json(&response)
}

// Validate API key and return associated email
pub async fn validate_api_key(req: &Request, env: &Env) -> std::result::Result<String, Result<Response>> {
    let api_key = match extract_token(req) {
        Some(key) => key,
        None => {
            return Err(ErrorResponse::unauthorized("Missing API key").to_response(401));
        }
    };

    // Get API_KEYS from environment: "key1:email1@example.com,key2:email2@example.com"
    let api_keys_str = env.var("API_KEYS").map_err(|_| {
        console_log!("API_KEYS environment variable not set");
        ErrorResponse::internal_error("Server configuration error").to_response(500)
    })?.to_string();

    // Parse API keys
    for key_pair in api_keys_str.split(',') {
        let parts: Vec<&str> = key_pair.split(':').collect();
        if parts.len() == 2 {
            let (key, email) = (parts[0].trim(), parts[1].trim());
            if key == api_key {
                return Ok(email.to_string());
            }
        }
    }

    Err(ErrorResponse::unauthorized("Invalid API key").to_response(401))
}

fn extract_token(req: &Request) -> Option<String> {
    let auth_header = req.headers().get("Authorization").ok()??;

    if auth_header.starts_with("Bearer ") {
        Some(auth_header[7..].to_string())
    } else {
        None
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    // Note: Tests that use worker::Request/Headers cannot run in standard cargo test
    // because they require WASM bindings. These tests require integration testing
    // against deployed workers or wasm-pack test.
    //
    // Testing strategy:
    // 1. Unit tests: Test pure logic (API key parsing, validation)
    // 2. Integration tests: Test actual HTTP endpoints with deployed worker

    #[test]
    fn test_api_key_parsing() {
        let api_keys_str = "dev-alice:alice@example.com,dev-bob:bob@example.com";
        let keys: Vec<_> = api_keys_str.split(',').collect();

        assert_eq!(keys.len(), 2);
        assert!(keys[0].contains("dev-alice"));
        assert!(keys[0].contains("alice@example.com"));
    }

    #[test]
    fn test_api_key_split() {
        let key_pair = "dev-alice:alice@example.com";
        let parts: Vec<&str> = key_pair.split(':').collect();

        assert_eq!(parts.len(), 2);
        assert_eq!(parts[0], "dev-alice");
        assert_eq!(parts[1], "alice@example.com");
    }

    #[test]
    fn test_bearer_token_format() {
        let auth_header = "Bearer dev-key-123";
        assert!(auth_header.starts_with("Bearer "));

        let token = &auth_header[7..];
        assert_eq!(token, "dev-key-123");
    }
}
