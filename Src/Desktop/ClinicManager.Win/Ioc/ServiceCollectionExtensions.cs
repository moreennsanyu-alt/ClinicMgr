using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ClinicManager.Win.Net.Http;

namespace ClinicManager.Win.Ioc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultLogging(this IServiceCollection services) =>
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddDebug(); // shows in the VS "Output" window
        });

    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        // Handlers used with AddHttpMessageHandler<T> must be registered (transient is the norm).
        // LoggingHttpMessageHandler depends on IRequestLogService, which is registered in App.RegisterTypes.
        services.AddTransient<LoggingHttpMessageHandler>();

        services
            .AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.restful-api.dev/");
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .AddHttpMessageHandler<LoggingHttpMessageHandler>();

        return services;
    }
}
