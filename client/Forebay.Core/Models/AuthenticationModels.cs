using System.Text.Json.Serialization;

namespace Forebay.Core.Models;

public class WhoamiResponse
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }
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
