namespace ClinicManager.Win.Models;

public sealed record RequestLogEntry(
    DateTimeOffset Timestamp,
    string Method,
    string Url,
    int? StatusCode,
    TimeSpan Duration,
    string? Error = null)
{
    public string Summary => Error is null
        ? $"{Timestamp:HH:mm:ss.fff}  {Method} {Url}  ->  {StatusCode} ({Duration.TotalMilliseconds:F0} ms)"
        : $"{Timestamp:HH:mm:ss.fff}  {Method} {Url}  ->  FAILED: {Error} ({Duration.TotalMilliseconds:F0} ms)";
}
