namespace ClinicManager.Win.Net.Http;

/// <summary>Typed client: the HttpClient is supplied (and configured) by IHttpClientFactory.</summary>
public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public async Task<string> GetStringAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(relativeUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
