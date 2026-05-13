using System.Text.Json;
using System.Text.Json.Serialization;

namespace Forebay.Core.Models;

public class PutRequest
{
    [JsonPropertyName("content")]
    public required JsonElement Content { get; set; }
}

public class PutResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("size_bytes")]
    public required int SizeBytes { get; set; }

    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required long UpdatedAt { get; set; }
}

public class GetResponse
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("content")]
    public required JsonElement Content { get; set; }

    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required long UpdatedAt { get; set; }

    [JsonPropertyName("size_bytes")]
    public required int SizeBytes { get; set; }
}

public class DeleteStorageResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("key")]
    public required string Key { get; set; }
}

public class DocumentInfo
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("size_bytes")]
    public required int SizeBytes { get; set; }

    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required long UpdatedAt { get; set; }
}

public class ListDocumentsResponse
{
    [JsonPropertyName("documents")]
    public required List<DocumentInfo> Documents { get; set; }

    [JsonPropertyName("count")]
    public required int Count { get; set; }
}
