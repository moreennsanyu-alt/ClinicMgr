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
        containerRegistry.RegisterSingleton<ShellWindowViewModel>();
        containerRegistry.RegisterSingleton<LoginViewModel>();
        containerRegistry.RegisterSingleton<MainViewModel>();
        containerRegistry.RegisterSingleton<RegisterViewModel>();
        containerRegistry.RegisterSingleton<ChangePasswordViewModel>();

        // Register views for navigation if needed
        containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
        containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
        containerRegistry.RegisterForNavigation<RegisterView, RegisterViewModel>();
        containerRegistry.RegisterForNavigation<ChangePasswordView, ChangePasswordViewModel>();
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
