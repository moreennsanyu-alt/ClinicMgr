using System.Diagnostics;
using ClinicManager.Win.Models;
using ClinicManager.Win.Services;

namespace ClinicManager.Win.Net.Http;

/// <summary>
/// Outgoing-request handler added to the HttpClient pipeline via
/// <c>AddHttpMessageHandler&lt;LoggingHttpMessageHandler&gt;()</c>.
/// It is created by IHttpClientFactory through the DryIoc-backed IServiceProvider, so its constructor
/// dependencies are resolved from the same container Prism uses.
/// </summary>
public sealed class LoggingHttpMessageHandler : DelegatingHandler
{
    private readonly IRequestLogService _log;

    public LoggingHttpMessageHandler(IRequestLogService log) => _log = log;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.Now;
        var sw = Stopwatch.StartNew();
        var method = request.Method.Method;
        var url = request.RequestUri?.ToString() ?? "(no uri)";

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            _log.Log(new RequestLogEntry(started, method, url, (int)response.StatusCode, sw.Elapsed));
            return response;
        }
        catch (Exception ex)
        {
            _log.Log(new RequestLogEntry(started, method, url, null, sw.Elapsed, ex.Message));
            throw;
        }
    }
}
