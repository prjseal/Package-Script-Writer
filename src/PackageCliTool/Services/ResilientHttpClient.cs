using Microsoft.Extensions.Logging;
using PackageCliTool.Exceptions;
using Polly;
using Polly.Retry;
using System.Net;

namespace PackageCliTool.Services;

/// <summary>
/// HTTP client with built-in retry logic and resilience patterns
/// </summary>
public class ResilientHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilientHttpClient"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client to use</param>
    /// <param name="logger">Optional logger instance</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    public ResilientHttpClient(HttpClient httpClient, ILogger? logger = null, int maxRetries = 3)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Define retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r =>
            {
                // Retry on transient failures
                return r.StatusCode == HttpStatusCode.RequestTimeout ||
                       r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                       r.StatusCode == HttpStatusCode.GatewayTimeout ||
                       r.StatusCode == HttpStatusCode.TooManyRequests ||
                       (int)r.StatusCode >= 500;
            })
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt =>
                {
                    // Exponential backoff: 2s, 4s, 8s
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    _logger?.LogWarning("Retry attempt {Attempt} after {Delay}ms", retryAttempt, delay.TotalMilliseconds);
                    return delay;
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        _logger?.LogWarning(
                            "Request failed with exception. Retry {RetryCount} after {Delay}ms. Exception: {Exception}",
                            retryCount,
                            timespan.TotalMilliseconds,
                            outcome.Exception.Message
                        );
                    }
                    else
                    {
                        _logger?.LogWarning(
                            "Request failed with status {StatusCode}. Retry {RetryCount} after {Delay}ms",
                            outcome.Result?.StatusCode,
                            retryCount,
                            timespan.TotalMilliseconds
                        );
                    }
                }
            );
    }

    /// <summary>
    /// Sends a GET request with retry logic
    /// </summary>
    public async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        _logger?.LogDebug("Sending GET request to {Uri}", requestUri);

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.GetAsync(requestUri);
            });

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new ApiException(
                    $"API request failed: {response.StatusCode}",
                    (int)response.StatusCode,
                    "Verify the API endpoint is correct and accessible"
                );
            }

            _logger?.LogDebug("GET request to {Uri} succeeded with status {StatusCode}", requestUri, response.StatusCode);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP request failed for {Uri}", requestUri);
            throw new ApiException(
                $"Failed to connect to API: {ex.Message}",
                ex,
                null,
                "Check your internet connection and verify the API is accessible"
            );
        }
        catch (TimeoutException ex)
        {
            _logger?.LogError(ex, "Request timeout for {Uri}", requestUri);
            throw new ApiException(
                $"Request timed out: {ex.Message}",
                ex,
                null,
                "The request took too long. Try again or check your network connection"
            );
        }
    }

    /// <summary>
    /// Sends a POST request with retry logic
    /// </summary>
    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
    {
        _logger?.LogDebug("Sending POST request to {Uri}", requestUri);

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.PostAsync(requestUri, content);
            });

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new ApiException(
                    $"API request failed: {response.StatusCode} - {responseContent}",
                    (int)response.StatusCode,
                    "Verify the request payload and API endpoint are correct"
                );
            }

            _logger?.LogDebug("POST request to {Uri} succeeded with status {StatusCode}", requestUri, response.StatusCode);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP request failed for {Uri}", requestUri);
            throw new ApiException(
                $"Failed to connect to API: {ex.Message}",
                ex,
                null,
                "Check your internet connection and verify the API is accessible"
            );
        }
        catch (TimeoutException ex)
        {
            _logger?.LogError(ex, "Request timeout for {Uri}", requestUri);
            throw new ApiException(
                $"Request timed out: {ex.Message}",
                ex,
                null,
                "The request took too long. Try again or check your network connection"
            );
        }
    }
}
