using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using ClinicManager.Win.ViewModels;
using ClinicManager.Win.Views;
using DryIoc;

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
    }

    protected override Rules CreateContainerRules()
{
    return Rules.Default.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient)
                        .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
                        .WithFuncAndLazyWithoutRegistration()
                        .WithTrackingDisposableTransients()
                        //.WithoutFastExpressionCompiler()
                        .WithFactorySelector(Rules.SelectLastRegisteredFactory());
}
}
