using System.Text.Json;
using System.Text.Json.Serialization;

namespace Forebay.Core.Models;

public class PushRequest
{
    [JsonPropertyName("payload")]
    public required JsonElement Payload { get; set; }
}

public class PushResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("queue")]
    public required string Queue { get; set; }

    [JsonPropertyName("length")]
    public required int Length { get; set; }

    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }

    [JsonPropertyName("timestamp")]
    public required long Timestamp { get; set; }
}

public class PullResponse
{
    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }

    [JsonPropertyName("payload")]
    public required JsonElement Payload { get; set; }

    [JsonPropertyName("timestamp")]
    public required long Timestamp { get; set; }

    [JsonPropertyName("remaining")]
    public required int Remaining { get; set; }
}

public class StatsResponse
{
    [JsonPropertyName("queue")]
    public required string Queue { get; set; }

    [JsonPropertyName("length")]
    public required int Length { get; set; }

    [JsonPropertyName("oldest_timestamp")]
    public long? OldestTimestamp { get; set; }

    [JsonPropertyName("newest_timestamp")]
    public long? NewestTimestamp { get; set; }

    [JsonPropertyName("total_size_bytes")]
    public required int TotalSizeBytes { get; set; }
}

public class DeleteResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("queue")]
    public required string Queue { get; set; }

    [JsonPropertyName("deleted_items")]
    public required int DeletedItems { get; set; }
}

public class QueueInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("length")]
    public required int Length { get; set; }

    [JsonPropertyName("oldest_timestamp")]
    public long? OldestTimestamp { get; set; }
}

public class ListQueuesResponse
{
    [JsonPropertyName("queues")]
    public required List<QueueInfo> Queues { get; set; }
}
