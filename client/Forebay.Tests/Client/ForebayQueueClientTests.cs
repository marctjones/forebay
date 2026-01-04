using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Forebay.Core;
using Forebay.Core.Models;
using Moq;
using Moq.Protected;

namespace Forebay.Tests.Client;

public class ForebayQueueClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly ForebayClient _client;

    public ForebayQueueClientTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };
        _client = new ForebayClient(_httpClient);
        _client.SetSessionToken("test-session");
    }

    [Fact]
    public async Task PushAsync_WhenSuccessful_ReturnsPushResponse()
    {
        var payload = JsonDocument.Parse("""{"message": "hello"}""").RootElement;
        var expectedResponse = new PushResponse
        {
            Success = true,
            Queue = "test-queue",
            Length = 1,
            ItemId = "item-123",
            Timestamp = 1609459200000
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.PathAndQuery == "/queues/test-queue/push" &&
                    req.Headers.Authorization!.Parameter == "test-session"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var result = await _client.PushAsync("test-queue", payload);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Queue.Should().Be("test-queue");
        result.Length.Should().Be(1);
        result.ItemId.Should().Be("item-123");
    }

    [Fact]
    public async Task PullAsync_WhenSuccessful_ReturnsPullResponse()
    {
        var expectedResponse = new PullResponse
        {
            ItemId = "item-123",
            Payload = JsonDocument.Parse("""{"message": "hello"}""").RootElement,
            Timestamp = 1609459200000,
            Remaining = 0
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.PathAndQuery == "/queues/test-queue/pull"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var result = await _client.PullAsync("test-queue");

        result.Should().NotBeNull();
        result.ItemId.Should().Be("item-123");
        result.Payload.GetProperty("message").GetString().Should().Be("hello");
        result.Remaining.Should().Be(0);
    }

    [Fact]
    public async Task PullAsync_WhenQueueEmpty_ThrowsForebayApiException()
    {
        var errorResponse = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = "NOT_FOUND",
                Message = "Queue is empty"
            }
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = JsonContent.Create(errorResponse)
            });

        var act = async () => await _client.PullAsync("test-queue");

        await act.Should().ThrowAsync<ForebayApiException>()
            .Where(ex => ex.Code == "NOT_FOUND");
    }

    [Fact]
    public async Task StatsAsync_WhenSuccessful_ReturnsStatsResponse()
    {
        var expectedResponse = new StatsResponse
        {
            Queue = "test-queue",
            Length = 5,
            OldestTimestamp = 1609459200000,
            NewestTimestamp = 1609459210000,
            TotalSizeBytes = 1024
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.PathAndQuery == "/queues/test-queue/stats"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var result = await _client.StatsAsync("test-queue");

        result.Should().NotBeNull();
        result.Queue.Should().Be("test-queue");
        result.Length.Should().Be(5);
        result.OldestTimestamp.Should().Be(1609459200000);
        result.NewestTimestamp.Should().Be(1609459210000);
        result.TotalSizeBytes.Should().Be(1024);
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_ReturnsDeleteResponse()
    {
        var expectedResponse = new DeleteResponse
        {
            Success = true,
            Queue = "test-queue",
            DeletedItems = 3
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.PathAndQuery == "/queues/test-queue"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var result = await _client.DeleteQueueAsync("test-queue");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Queue.Should().Be("test-queue");
        result.DeletedItems.Should().Be(3);
    }

    [Fact]
    public async Task ListQueuesAsync_WhenSuccessful_ReturnsListQueuesResponse()
    {
        var expectedResponse = new ListQueuesResponse
        {
            Queues = new List<QueueInfo>
            {
                new() { Name = "queue1", Length = 5, OldestTimestamp = 1609459200000 },
                new() { Name = "queue2", Length = 0, OldestTimestamp = null }
            }
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.PathAndQuery == "/queues"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var result = await _client.ListQueuesAsync();

        result.Should().NotBeNull();
        result.Queues.Should().HaveCount(2);
        result.Queues[0].Name.Should().Be("queue1");
        result.Queues[1].Name.Should().Be("queue2");
    }

    [Fact]
    public async Task QueueOperations_WithoutSessionToken_ThrowsInvalidOperationException()
    {
        var clientWithoutAuth = new ForebayClient(_httpClient);
        var payload = JsonDocument.Parse("""{"test": "data"}""").RootElement;

        var pushAct = async () => await clientWithoutAuth.PushAsync("test", payload);
        var pullAct = async () => await clientWithoutAuth.PullAsync("test");
        var statsAct = async () => await clientWithoutAuth.StatsAsync("test");
        var deleteAct = async () => await clientWithoutAuth.DeleteQueueAsync("test");
        var listAct = async () => await clientWithoutAuth.ListQueuesAsync();

        await pushAct.Should().ThrowAsync<InvalidOperationException>();
        await pullAct.Should().ThrowAsync<InvalidOperationException>();
        await statsAct.Should().ThrowAsync<InvalidOperationException>();
        await deleteAct.Should().ThrowAsync<InvalidOperationException>();
        await listAct.Should().ThrowAsync<InvalidOperationException>();
    }
}
