using System.Text.Json;
using FluentAssertions;
using Forebay.Core.Models;

namespace Forebay.Tests.Models;

public class QueueModelsTests
{
    [Fact]
    public void PushRequest_ShouldSerializeCorrectly()
    {
        var payload = JsonDocument.Parse("""{"message": "hello"}""").RootElement;
        var request = new PushRequest { Payload = payload };
        var json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"payload\"");
        json.Should().Contain("\"message\":\"hello\"");
    }

    [Fact]
    public void PushResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "success": true,
                "queue": "test-queue",
                "length": 5,
                "item_id": "item-123",
                "timestamp": 1609459200000
            }
            """;

        var response = JsonSerializer.Deserialize<PushResponse>(json);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Queue.Should().Be("test-queue");
        response.Length.Should().Be(5);
        response.ItemId.Should().Be("item-123");
        response.Timestamp.Should().Be(1609459200000);
    }

    [Fact]
    public void PullResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "item_id": "item-123",
                "payload": {"message": "hello"},
                "timestamp": 1609459200000,
                "remaining": 4
            }
            """;

        var response = JsonSerializer.Deserialize<PullResponse>(json);

        response.Should().NotBeNull();
        response!.ItemId.Should().Be("item-123");
        response.Payload.GetProperty("message").GetString().Should().Be("hello");
        response.Timestamp.Should().Be(1609459200000);
        response.Remaining.Should().Be(4);
    }

    [Fact]
    public void StatsResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "queue": "test-queue",
                "length": 10,
                "oldest_timestamp": 1609459200000,
                "newest_timestamp": 1609459210000,
                "total_size_bytes": 1024
            }
            """;

        var response = JsonSerializer.Deserialize<StatsResponse>(json);

        response.Should().NotBeNull();
        response!.Queue.Should().Be("test-queue");
        response.Length.Should().Be(10);
        response.OldestTimestamp.Should().Be(1609459200000);
        response.NewestTimestamp.Should().Be(1609459210000);
        response.TotalSizeBytes.Should().Be(1024);
    }

    [Fact]
    public void StatsResponse_WithNullTimestamps_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "queue": "empty-queue",
                "length": 0,
                "total_size_bytes": 0
            }
            """;

        var response = JsonSerializer.Deserialize<StatsResponse>(json);

        response.Should().NotBeNull();
        response!.Queue.Should().Be("empty-queue");
        response.Length.Should().Be(0);
        response.OldestTimestamp.Should().BeNull();
        response.NewestTimestamp.Should().BeNull();
        response.TotalSizeBytes.Should().Be(0);
    }

    [Fact]
    public void DeleteResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "success": true,
                "queue": "test-queue",
                "deleted_items": 5
            }
            """;

        var response = JsonSerializer.Deserialize<DeleteResponse>(json);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Queue.Should().Be("test-queue");
        response.DeletedItems.Should().Be(5);
    }

    [Fact]
    public void ListQueuesResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "queues": [
                    {
                        "name": "queue1",
                        "length": 5,
                        "oldest_timestamp": 1609459200000
                    },
                    {
                        "name": "queue2",
                        "length": 0
                    }
                ]
            }
            """;

        var response = JsonSerializer.Deserialize<ListQueuesResponse>(json);

        response.Should().NotBeNull();
        response!.Queues.Should().HaveCount(2);
        response.Queues[0].Name.Should().Be("queue1");
        response.Queues[0].Length.Should().Be(5);
        response.Queues[0].OldestTimestamp.Should().Be(1609459200000);
        response.Queues[1].Name.Should().Be("queue2");
        response.Queues[1].Length.Should().Be(0);
        response.Queues[1].OldestTimestamp.Should().BeNull();
    }
}
