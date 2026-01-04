using System.Text.Json;
using FluentAssertions;
using Forebay.Core.Models;

namespace Forebay.Tests.Models;

public class AuthenticationModelsTests
{
    [Fact]
    public void LoginRequest_ShouldSerializeCorrectly()
    {
        var request = new LoginRequest { IdToken = "test-token" };
        var json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"id_token\":\"test-token\"");
    }

    [Fact]
    public void LoginResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "session_token": "session-123",
                "email": "test@example.com",
                "expires_at": 1609459200000
            }
            """;

        var response = JsonSerializer.Deserialize<LoginResponse>(json);

        response.Should().NotBeNull();
        response!.SessionToken.Should().Be("session-123");
        response.Email.Should().Be("test@example.com");
        response.ExpiresAt.Should().Be(1609459200000);
    }

    [Fact]
    public void WhoamiResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "email": "test@example.com",
                "expires_at": 1609459200000
            }
            """;

        var response = JsonSerializer.Deserialize<WhoamiResponse>(json);

        response.Should().NotBeNull();
        response!.Email.Should().Be("test@example.com");
        response.ExpiresAt.Should().Be(1609459200000);
    }

    [Fact]
    public void LogoutResponse_ShouldDeserializeCorrectly()
    {
        var json = """{"success": true}""";

        var response = JsonSerializer.Deserialize<LogoutResponse>(json);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public void ErrorResponse_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "error": {
                    "code": "UNAUTHORIZED",
                    "message": "Invalid token"
                }
            }
            """;

        var response = JsonSerializer.Deserialize<ErrorResponse>(json);

        response.Should().NotBeNull();
        response!.Error.Code.Should().Be("UNAUTHORIZED");
        response.Error.Message.Should().Be("Invalid token");
        response.Error.Details.Should().BeNull();
    }

    [Fact]
    public void ErrorResponse_WithDetails_ShouldDeserializeCorrectly()
    {
        var json = """
            {
                "error": {
                    "code": "BAD_REQUEST",
                    "message": "Invalid input",
                    "details": "Missing required field"
                }
            }
            """;

        var response = JsonSerializer.Deserialize<ErrorResponse>(json);

        response.Should().NotBeNull();
        response!.Error.Code.Should().Be("BAD_REQUEST");
        response.Error.Message.Should().Be("Invalid input");
        response.Error.Details.Should().Be("Missing required field");
    }
}
