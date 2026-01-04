using System.Text.Json.Serialization;

namespace Forebay.Core.Models;

public class LoginRequest
{
    [JsonPropertyName("id_token")]
    public required string IdToken { get; set; }
}

public class LoginResponse
{
    [JsonPropertyName("session_token")]
    public required string SessionToken { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("expires_at")]
    public required long ExpiresAt { get; set; }
}

public class WhoamiResponse
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("expires_at")]
    public required long ExpiresAt { get; set; }
}

public class LogoutResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }
}

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public required ErrorDetail Error { get; set; }
}

public class ErrorDetail
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
