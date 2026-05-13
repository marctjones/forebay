using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Forebay.Core.Models;

namespace Forebay.Core;

public class ForebayClient
{
    private readonly HttpClient _httpClient;
    private string? _apiKey;

    public ForebayClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public ForebayClient(string baseUrl) : this(new HttpClient { BaseAddress = new Uri(baseUrl) })
    {
    }

    public void SetApiKey(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public void ClearApiKey()
    {
        _apiKey = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Legacy methods for backward compatibility
    [Obsolete("Use SetApiKey instead")]
    public void SetSessionToken(string sessionToken) => SetApiKey(sessionToken);

    [Obsolete("Use ClearApiKey instead")]
    public void ClearSessionToken() => ClearApiKey();

    // Authentication methods

    public async Task<WhoamiResponse> WhoAmIAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("No API key set. Use SetApiKey first.");
        }

        var response = await _httpClient.GetAsync("/auth/whoami", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<WhoamiResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize whoami response");
    }

    // Queue methods

    public async Task<PushResponse> PushAsync(string queueName, JsonElement payload, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var request = new PushRequest { Payload = payload };
        var response = await _httpClient.PostAsJsonAsync($"/queues/{queueName}/push", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<PushResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize push response");
    }

    public async Task<PullResponse> PullAsync(string queueName, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var response = await _httpClient.PostAsync($"/queues/{queueName}/pull", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<PullResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize pull response");
    }

    public async Task<StatsResponse> StatsAsync(string queueName, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var response = await _httpClient.GetAsync($"/queues/{queueName}/stats", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<StatsResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize stats response");
    }

    public async Task<DeleteResponse> DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var response = await _httpClient.DeleteAsync($"/queues/{queueName}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<DeleteResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize delete response");
    }

    public async Task<ListQueuesResponse> ListQueuesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var response = await _httpClient.GetAsync("/queues", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<ListQueuesResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize list queues response");
    }

    // Storage methods

    public async Task<PutResponse> PutDocumentAsync(string key, JsonElement content, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var request = new PutRequest { Content = content };
        var response = await _httpClient.PutAsJsonAsync($"/store/{key}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<PutResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize put response");
    }

    public async Task<GetResponse> GetDocumentAsync(string key, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var response = await _httpClient.GetAsync($"/store/{key}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<GetResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize get response");
    }

    public async Task<DeleteStorageResponse> DeleteDocumentAsync(string key, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var response = await _httpClient.DeleteAsync($"/store/{key}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<DeleteStorageResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize delete response");
    }

    public async Task<ListDocumentsResponse> ListDocumentsAsync(string? prefix = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(prefix))
        {
            queryParams.Add($"prefix={Uri.EscapeDataString(prefix)}");
        }
        if (limit.HasValue)
        {
            queryParams.Add($"limit={limit.Value}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await _httpClient.GetAsync($"/store{queryString}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowForebayApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<ListDocumentsResponse>(cancellationToken)
            ?? throw new ForebayApiException("INVALID_RESPONSE", "Failed to deserialize list documents response");
    }

    // Helper methods

    private void EnsureAuthenticated()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("No API key set. Use SetApiKey first.");
        }
    }

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
