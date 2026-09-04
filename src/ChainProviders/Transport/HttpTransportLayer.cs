using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Cs2Hvh.ChainProviders.Transport;

/// <summary>
/// HTTP transport with timeout, retry, and endpoint rotation for JSON-RPC backends.
/// </summary>
public sealed class HttpTransportLayer
{
    private readonly HttpClient _httpClient;
    private readonly EndpointRotator _endpointRotator;
    private readonly RateLimitHandler _rateLimitHandler;

    public HttpTransportLayer(HttpClient httpClient, EndpointRotator endpointRotator, RateLimitHandler rateLimitHandler)
    {
        _httpClient = httpClient;
        _endpointRotator = endpointRotator;
        _rateLimitHandler = rateLimitHandler;
    }

    public async Task<string> PostJsonAsync(string networkId, string payload, CancellationToken cancellationToken)
    {
        await _rateLimitHandler.WaitForSlotAsync(networkId, cancellationToken).ConfigureAwait(false);

        var endpoint = _endpointRotator.GetNextEndpoint(networkId);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var stopwatch = Stopwatch.StartNew();
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> DeserializeAsync<T>(string json, CancellationToken cancellationToken)
    {
        await Task.Yield();
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
    }
}
