using System.Windows;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Prism.DryIoc;
using Prism.Ioc;

namespace ClinicManager.Win;

public partial class App : CMApplication
{


    protected override Window CreateShell() => Container.Resolve<ShellWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<INavigationTreeBuilder, NavigationTreeBuilder>();
        containerRegistry.RegisterSingleton<IRequestLogService, RequestLogService>();
        containerRegistry.RegisterSingleton<IApiClient, ApiClient>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        // Cleanup code can be added here later.
    }
}
