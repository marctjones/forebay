using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Forebay.Core.Models;

namespace Forebay.Core;

public class ForebayClient
{
    private readonly HttpClient _httpClient;
    private string? _sessionToken;

    public ForebayClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public ForebayClient(string baseUrl) : this(new HttpClient { BaseAddress = new Uri(baseUrl) })
    {
    }

    public void SetSessionToken(string sessionToken)
    {
        _sessionToken = sessionToken;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
    }

    public void ClearSessionToken()
    {
        _sessionToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Authentication methods

    public async Task<LoginResponse> LoginAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest { IdToken = idToken };
        var response = await _httpClient.PostAsJsonAsync("/auth/login", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize login response");

        SetSessionToken(loginResponse.SessionToken);
        return loginResponse;
    }

    public async Task<WhoamiResponse> WhoAmIAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            throw new InvalidOperationException("No session token set. Call LoginAsync first or use SetSessionToken.");
        }

        var response = await _httpClient.GetAsync("/auth/whoami", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<WhoamiResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize whoami response");
    }

    public async Task<LogoutResponse> LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            throw new InvalidOperationException("No session token set. Call LoginAsync first or use SetSessionToken.");
        }

        var response = await _httpClient.PostAsync("/auth/logout", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        var logoutResponse = await response.Content.ReadFromJsonAsync<LogoutResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize logout response");

        ClearSessionToken();
        return logoutResponse;
    }

    // Helper methods

    private static async Task ThrowForebayApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);
            if (errorResponse?.Error != null)
            {
                throw new ForebayApiException(
                    errorResponse.Error.Code,
                    errorResponse.Error.Message,
                    errorResponse.Error.Details);
            }
        }
        catch (JsonException)
        {
            // If we can't parse the error response, fall through to generic error
        }

        throw new ForebayApiException(
            "HTTP_ERROR",
            $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase ?? "Unknown error"}");
    }
}

public class ForebayApiException : Exception
{
    public string Code { get; }
    public string? Details { get; }

    public ForebayApiException(string code, string message, string? details = null)
        : base(message)
    {
        Code = code;
        Details = details;
    }
}
