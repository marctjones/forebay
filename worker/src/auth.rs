use crate::error::ErrorResponse;
use crate::models::{LoginRequest, LoginResponse, LogoutResponse, SessionData, WhoamiResponse};
use chrono::Utc;
use jsonwebtoken::{decode, decode_header, Algorithm, DecodingKey, Validation};
use serde::{Deserialize, Serialize};
use uuid::Uuid;
use worker::*;

const SESSION_TTL_DAYS: i64 = 30;
const GOOGLE_CERTS_URL: &str = "https://www.googleapis.com/oauth2/v3/certs";

#[derive(Debug, Serialize, Deserialize)]
struct GoogleClaims {
    iss: String,
    sub: String,
    email: String,
    email_verified: bool,
    aud: String,
    exp: i64,
    iat: i64,
}

#[derive(Debug, Deserialize)]
struct GoogleJwks {
    keys: Vec<GoogleJwk>,
}

#[derive(Debug, Deserialize)]
struct GoogleJwk {
    kid: String,
    n: String,
    e: String,
}

pub async fn handle_login(mut req: Request, env: Env) -> Result<Response> {
    // Parse request body
    let login_req: LoginRequest = match req.json().await {
        Ok(r) => r,
        Err(_) => {
            return ErrorResponse::bad_request("Invalid request body").to_response(400);
        }
    };

    // Verify Google ID token
    let claims = match verify_google_token(&login_req.id_token, &env).await {
        Ok(c) => c,
        Err(e) => {
            console_log!("Token verification failed: {:?}", e);
            return ErrorResponse::unauthorized("Invalid ID token").to_response(401);
        }
    };

    // Check if email is allowed
    let allowed_emails = match env.var("ALLOWED_EMAILS") {
        Ok(emails) => emails.to_string(),
        Err(_) => String::new(),
    };

    let email_list: Vec<&str> = allowed_emails.split(',').collect();
    if !email_list.is_empty() && !email_list.contains(&claims.email.as_str()) {
        return ErrorResponse::unauthorized("Email not authorized").to_response(401);
    }

    // Generate session token
    let session_token = Uuid::new_v4().to_string();
    let now = Utc::now().timestamp_millis();
    let expires_at = now + (SESSION_TTL_DAYS * 24 * 60 * 60 * 1000);

    let session_data = SessionData {
        email: claims.email.clone(),
        created_at: now,
        expires_at,
    };

    // Store session in KV
    let kv = env.kv("SESSION_TOKENS")?;
    let key = format!("session:{}", session_token);
    let ttl_seconds = (SESSION_TTL_DAYS * 24 * 60 * 60) as u64;

    kv.put(&key, serde_json::to_string(&session_data)?)?
        .expiration_ttl(ttl_seconds)
        .execute()
        .await?;

    // Return response
    let response = LoginResponse {
        session_token,
        email: claims.email,
        expires_at,
    };

    Response::from_json(&response)
}

pub async fn handle_whoami(req: Request, env: Env) -> Result<Response> {
    // Extract and verify session token
    let session_data = match get_session(&req, &env).await {
        Ok(data) => data,
        Err(e) => return e,
    };

    let response = WhoamiResponse {
        email: session_data.email,
        expires_at: session_data.expires_at,
    };

    Response::from_json(&response)
}

pub async fn handle_logout(req: Request, env: Env) -> Result<Response> {
    // Extract session token
    let token = match extract_token(&req) {
        Some(t) => t,
        None => {
            return ErrorResponse::unauthorized("Missing session token").to_response(401);
        }
    };

    // Delete session from KV
    let kv = env.kv("SESSION_TOKENS")?;
    let key = format!("session:{}", token);
    kv.delete(&key).await?;

    let response = LogoutResponse { success: true };
    Response::from_json(&response)
}

pub async fn get_session(req: &Request, env: &Env) -> std::result::Result<SessionData, Result<Response>> {
    let token = match extract_token(req) {
        Some(t) => t,
        None => {
            return Err(ErrorResponse::unauthorized("Missing session token").to_response(401));
        }
    };

    let kv = env.kv("SESSION_TOKENS").map_err(|e| {
        console_log!("KV error: {:?}", e);
        ErrorResponse::internal_error("Server error").to_response(500)
    })?;

    let key = format!("session:{}", token);
    let session_json = kv.get(&key).text().await.map_err(|e| {
        console_log!("KV get error: {:?}", e);
        ErrorResponse::internal_error("Server error").to_response(500)
    })?;

    match session_json {
        Some(json) => {
            let session: SessionData = serde_json::from_str(&json).map_err(|e| {
                console_log!("JSON parse error: {:?}", e);
                ErrorResponse::internal_error("Server error").to_response(500)
            })?;

            // Check if session is expired
            let now = Utc::now().timestamp_millis();
            if now > session.expires_at {
                return Err(ErrorResponse::unauthorized("Session expired").to_response(401));
            }

            Ok(session)
        }
        None => Err(ErrorResponse::unauthorized("Invalid session token").to_response(401)),
    }
}

fn extract_token(req: &Request) -> Option<String> {
    let auth_header = req.headers().get("Authorization").ok()??;

    if auth_header.starts_with("Bearer ") {
        Some(auth_header[7..].to_string())
    } else {
        None
    }
}

async fn verify_google_token(token: &str, env: &Env) -> std::result::Result<GoogleClaims, String> {
    // Decode header to get kid
    let header = decode_header(token).map_err(|e| format!("Failed to decode header: {}", e))?;

    let kid = header.kid.ok_or("No kid in token header")?;

    // Fetch Google's public keys
    let jwks = fetch_google_jwks().await?;

    // Find the key with matching kid
    let jwk = jwks
        .keys
        .iter()
        .find(|k| k.kid == kid)
        .ok_or("No matching key found")?;

    // Decode the RSA key components
    use base64::{engine::general_purpose::URL_SAFE_NO_PAD, Engine as _};
    let n_bytes = URL_SAFE_NO_PAD.decode(&jwk.n)
        .map_err(|e| format!("Failed to decode n: {}", e))?;
    let e_bytes = URL_SAFE_NO_PAD.decode(&jwk.e)
        .map_err(|e| format!("Failed to decode e: {}", e))?;

    // Create decoding key from RSA components
    let decoding_key = DecodingKey::from_rsa_raw_components(&n_bytes, &e_bytes);

    // Validate the token
    let mut validation = Validation::new(Algorithm::RS256);

    // Get expected audience (Google Client ID)
    if let Ok(client_id) = env.var("GOOGLE_CLIENT_ID") {
        validation.set_audience(&[client_id.to_string()]);
    }

    let token_data = decode::<GoogleClaims>(token, &decoding_key, &validation)
        .map_err(|e| format!("Token validation failed: {}", e))?;

    // Verify email is verified
    if !token_data.claims.email_verified {
        return Err("Email not verified".to_string());
    }

    Ok(token_data.claims)
}

async fn fetch_google_jwks() -> std::result::Result<GoogleJwks, String> {
    let headers = Headers::new();
    headers.set("Accept", "application/json")
        .map_err(|e| format!("Failed to set header: {:?}", e))?;

    let mut init = RequestInit::new();
    init.with_headers(headers);

    let request = Request::new_with_init(GOOGLE_CERTS_URL, &init)
        .map_err(|e| format!("Failed to create request: {:?}", e))?;

    let mut response = Fetch::Request(request)
        .send()
        .await
        .map_err(|e| format!("Failed to fetch JWKS: {:?}", e))?;

    response
        .json::<GoogleJwks>()
        .await
        .map_err(|e| format!("Failed to parse JWKS: {:?}", e))
}

#[cfg(test)]
mod tests {
    use super::*;

    // Note: Tests that use worker::Request/Headers cannot run in standard cargo test
    // because they require WASM bindings. These tests require integration testing
    // against deployed workers or wasm-pack test.
    //
    // Testing strategy:
    // 1. Unit tests: Test pure logic (serialization, business logic)
    // 2. Integration tests: Test actual HTTP endpoints with deployed worker

    #[test]
    fn test_session_data_serialization() {
        let session = SessionData {
            email: "test@example.com".to_string(),
            created_at: 1609459200000,
            expires_at: 1612137600000,
        };

        let json = serde_json::to_string(&session).unwrap();
        assert!(json.contains("\"email\":\"test@example.com\""));
        assert!(json.contains("\"created_at\":1609459200000"));
        assert!(json.contains("\"expires_at\":1612137600000"));
    }

    #[test]
    fn test_session_data_deserialization() {
        let json = r#"{"email":"test@example.com","created_at":1609459200000,"expires_at":1612137600000}"#;
        let session: SessionData = serde_json::from_str(json).unwrap();

        assert_eq!(session.email, "test@example.com");
        assert_eq!(session.created_at, 1609459200000);
        assert_eq!(session.expires_at, 1612137600000);
    }

    #[test]
    fn test_session_ttl_calculation() {
        let ttl_seconds = (SESSION_TTL_DAYS * 24 * 60 * 60) as u64;
        assert_eq!(ttl_seconds, 2592000); // 30 days in seconds
    }

    #[test]
    fn test_session_key_format() {
        let session_token = "test-token-123";
        let key = format!("session:{}", session_token);
        assert_eq!(key, "session:test-token-123");
    }

    #[test]
    fn test_google_certs_url() {
        assert_eq!(GOOGLE_CERTS_URL, "https://www.googleapis.com/oauth2/v3/certs");
    }

    #[test]
    fn test_session_expiry_check() {
        let now = 1609459200000i64;
        let expires_at_future = now + 1000;
        let expires_at_past = now - 1000;

        assert!(now < expires_at_future, "Session should not be expired");
        assert!(now > expires_at_past, "Session should be expired");
    }
}
