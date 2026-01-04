use serde::Serialize;
use worker::*;

#[derive(Debug, Serialize)]
pub struct ErrorResponse {
    pub error: ErrorDetail,
}

#[derive(Debug, Serialize)]
pub struct ErrorDetail {
    pub code: String,
    pub message: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub details: Option<String>,
}

impl ErrorResponse {
    pub fn unauthorized(message: &str) -> Self {
        Self {
            error: ErrorDetail {
                code: "UNAUTHORIZED".to_string(),
                message: message.to_string(),
                details: None,
            },
        }
    }

    pub fn bad_request(message: &str) -> Self {
        Self {
            error: ErrorDetail {
                code: "BAD_REQUEST".to_string(),
                message: message.to_string(),
                details: None,
            },
        }
    }

    pub fn not_found(message: &str) -> Self {
        Self {
            error: ErrorDetail {
                code: "NOT_FOUND".to_string(),
                message: message.to_string(),
                details: None,
            },
        }
    }

    pub fn internal_error(message: &str) -> Self {
        Self {
            error: ErrorDetail {
                code: "INTERNAL_ERROR".to_string(),
                message: message.to_string(),
                details: None,
            },
        }
    }

    pub fn to_response(&self, status: u16) -> Result<Response> {
        Ok(Response::from_json(self)?.with_status(status))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_unauthorized_error() {
        let err = ErrorResponse::unauthorized("test message");
        assert_eq!(err.error.code, "UNAUTHORIZED");
        assert_eq!(err.error.message, "test message");
        assert!(err.error.details.is_none());
    }

    #[test]
    fn test_bad_request_error() {
        let err = ErrorResponse::bad_request("invalid input");
        assert_eq!(err.error.code, "BAD_REQUEST");
        assert_eq!(err.error.message, "invalid input");
        assert!(err.error.details.is_none());
    }

    #[test]
    fn test_not_found_error() {
        let err = ErrorResponse::not_found("resource not found");
        assert_eq!(err.error.code, "NOT_FOUND");
        assert_eq!(err.error.message, "resource not found");
    }

    #[test]
    fn test_internal_error() {
        let err = ErrorResponse::internal_error("server error");
        assert_eq!(err.error.code, "INTERNAL_ERROR");
        assert_eq!(err.error.message, "server error");
    }

    #[test]
    fn test_error_serialization() {
        let err = ErrorResponse::unauthorized("test");
        let json = serde_json::to_string(&err).unwrap();
        assert!(json.contains("\"code\":\"UNAUTHORIZED\""));
        assert!(json.contains("\"message\":\"test\""));
    }

    #[test]
    fn test_error_detail_no_details() {
        let err = ErrorResponse::unauthorized("test");
        let json = serde_json::to_string(&err).unwrap();
        // details field should not appear when None
        assert!(!json.contains("\"details\""));
    }
}
