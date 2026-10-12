using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.Logging;
using ClinicManager.Win.Models;

namespace ClinicManager.Win.Services;

public sealed class RequestLogService : IRequestLogService
{
    private readonly ILogger<RequestLogService> _logger; // comes from Microsoft.Extensions.Logging (MS DI side)

    public RequestLogService(ILogger<RequestLogService> logger) => _logger = logger;

    public ObservableCollection<RequestLogEntry> Entries { get; } = new();

    public void Log(RequestLogEntry entry)
    {
        if (entry.Error is null)
            _logger.LogInformation("{Method} {Url} -> {Status} in {Elapsed} ms",
                entry.Method, entry.Url, entry.StatusCode, entry.Duration.TotalMilliseconds);
        else
            _logger.LogWarning("{Method} {Url} failed: {Error}", entry.Method, entry.Url, entry.Error);

        OnUiThread(() => Entries.Add(entry));
    }

    public void Clear() => OnUiThread(Entries.Clear);

    // HttpClient continuations frequently run on thread-pool threads; the collection is bound to the UI.
    private static void OnUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.BeginInvoke(action);
    }
}
