namespace ClinicManager.Win.Net.Http;

public interface IApiClient
{
    Task<string> GetStringAsync(string relativeUrl, CancellationToken cancellationToken = default);
}
