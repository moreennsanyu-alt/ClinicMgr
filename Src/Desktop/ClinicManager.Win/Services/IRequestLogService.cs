using System.Collections.ObjectModel;
using ClinicManager.Win.Models;

namespace ClinicManager.Win.Services;

/// <summary>Collects HTTP request/response log entries so the UI can display them.</summary>
public interface IRequestLogService
{
    ObservableCollection<RequestLogEntry> Entries { get; }

    /// <summary>Thread-safe: may be called from any thread.</summary>
    void Log(RequestLogEntry entry);

    void Clear();
}
