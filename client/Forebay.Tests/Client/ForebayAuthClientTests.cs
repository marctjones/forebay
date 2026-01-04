using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Forebay.Core;
using Forebay.Core.Models;
using Moq;
using Moq.Protected;

namespace Forebay.Tests.Client;

public class ForebayAuthClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly ForebayClient _client;

    public ForebayAuthClientTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };
        _client = new ForebayClient(_httpClient);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_ReturnsLoginResponse()
    {
        var expectedResponse = new LoginResponse
        {
            SessionToken = "session-123",
            Email = "test@example.com",
            ExpiresAt = 1609459200000
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.PathAndQuery == "/auth/login"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var result = await _client.LoginAsync("test-id-token");

        result.Should().NotBeNull();
        result.SessionToken.Should().Be("session-123");
        result.Email.Should().Be("test@example.com");
        result.ExpiresAt.Should().Be(1609459200000);
    }

    [Fact]
    public async Task LoginAsync_WhenUnauthorized_ThrowsForebayApiException()
    {
        var errorResponse = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = "UNAUTHORIZED",
                Message = "Invalid ID token"
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
                StatusCode = HttpStatusCode.Unauthorized,
                Content = JsonContent.Create(errorResponse)
            });

        var act = async () => await _client.LoginAsync("invalid-token");

        await act.Should().ThrowAsync<ForebayApiException>()
            .Where(ex => ex.Code == "UNAUTHORIZED" && ex.Message.Contains("Invalid ID token"));
    }

    [Fact]
    public async Task WhoAmIAsync_WhenSuccessful_ReturnsWhoamiResponse()
    {
        var expectedResponse = new WhoamiResponse
        {
            Email = "test@example.com",
            ExpiresAt = 1609459200000
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.PathAndQuery == "/auth/whoami" &&
                    req.Headers.Authorization!.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == "session-123"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        _client.SetSessionToken("session-123");
        var result = await _client.WhoAmIAsync();

        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.ExpiresAt.Should().Be(1609459200000);
    }

    [Fact]
    public async Task WhoAmIAsync_WithoutSessionToken_ThrowsInvalidOperationException()
    {
        var act = async () => await _client.WhoAmIAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*session token*");
    }

    [Fact]
    public async Task LogoutAsync_WhenSuccessful_ReturnsLogoutResponse()
    {
        var expectedResponse = new LogoutResponse { Success = true };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.PathAndQuery == "/auth/logout" &&
                    req.Headers.Authorization!.Parameter == "session-123"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        _client.SetSessionToken("session-123");
        var result = await _client.LogoutAsync();

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SetSessionToken_ShouldSetAuthorizationHeader()
    {
        var expectedResponse = new WhoamiResponse
        {
            Email = "test@example.com",
            ExpiresAt = 1609459200000
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        _client.SetSessionToken("test-token");
        await _client.WhoAmIAsync();

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("test-token");
    }
}
