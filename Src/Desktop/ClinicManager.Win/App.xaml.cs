using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using ClinicManager.Win.ViewModels;
using ClinicManager.Win.Views;

namespace ClinicManager.Win;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        var shell = Container.Resolve<ShellWindow>();
        return shell;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (MainWindow is ShellWindow shellWindow)
        {
            shellWindow.MainContent.Content = Container.Resolve<LoginView>();
        }
    }
}
