using System.Windows;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Prism.DryIoc;
using Prism.Ioc;
using DevExpress.Xpf.Core;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ClinicManager.Win;

public abstract class CMApplication : PrismApplication
{


    static CMApplication() 
    {
        PreloadThemes();
        SetupLogging();
        BuildConfiguration();      
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void PreloadThemes()
    {
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;
        SplashScreenManager.CreateThemed().ShowOnStartup();
        ApplicationThemeHelper.Preload(PreloadCategories.Grid, PreloadCategories.LayoutControl,
                                        PreloadCategories.Core,
                                        PreloadCategories.Ribbon,
                                        PreloadCategories.Docking
                                      );
    }    
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void SetupLogging()
    {
    }

    
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void BuildConfiguration()
    {
    }
    /// <summary>
    /// Build the DryIoc container ourselves so we can pour an <see cref="IServiceCollection"/> into it
    /// *before* Prism starts registering its own types. The returned container is then handed to Prism.
    /// After this, there is ONE container: anything registered through Microsoft DI (IHttpClientFactory,
    /// ILogger&lt;T&gt;, typed clients...) can be injected into Prism view models, and anything registered
    /// through Prism (see <see cref="RegisterTypes"/>) can be injected into Microsoft DI-created objects
    /// such as the HTTP logging handler.
    /// </summary>
    protected override IContainerExtension CreateContainerExtension()
    {
        var services = new ServiceCollection();
        services.AddDefaultLogging();
        services.AddHttpClients();

        IContainer container = new DryIoc.Container(CreateContainerRules())
            .WithDependencyInjectionAdapter(services);

        return new DryIocContainerExtension(container);
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

    // App.xaml.cs
    protected override void ConfigureModuleCatalog(IModuleCatalog catalog)
    {
        var moduleTypes = typeof(CMApplication).Assembly
            .GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t)
                    && t is { IsAbstract: false, IsInterface: false });

        foreach (var type in moduleTypes)
            catalog.AddModule(type);   // WhenAvailable by default
    }
    protected override void ConfigureViewModelLocator()
    {
        ViewModelLocationProvider.SetDefaultViewModelFactory((viewModelType) => 
        {
            return Container.Resolve(viewModelType);
        });
        
        ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
        {
            var viewName = viewType.FullName;
            var assemblyName = viewType.Assembly.FullName;
            string viewModelName;

            if (viewName.EndsWith("Window"))
            {
                var baseName = viewName.Substring(0, viewName.Length - "Window".Length);
                viewModelName = ResolveFirst(assemblyName, baseName + "ViewModel", baseName + "WindowViewModel");
            }
            else if (viewName.EndsWith("Page"))
            {
                var baseName = viewName.Substring(0, viewName.Length - "Page".Length);
                viewModelName = ResolveFirst(assemblyName, baseName + "PageViewModel", baseName + "ViewModel");
            }
            else if (viewName.EndsWith("Control"))
            {
                var baseName = viewName.Substring(0, viewName.Length - "Control".Length);
                viewModelName = ResolveFirst(assemblyName, baseName + "ViewModel", baseName + "ControlViewModel");
            }
            else if (viewName.EndsWith("View"))
            {
                var baseName = viewName.Substring(0, viewName.Length - "View".Length);
                viewModelName = baseName + "ViewModel";
            }
            else
            {
                viewModelName = viewName + "ViewModel";
            }

            return Type.GetType($"{viewModelName}, {assemblyName}");
        });
    }

    private static string ResolveFirst(string assemblyName, string primaryName, string fallbackName)
    {
        return Type.GetType($"{primaryName}, {assemblyName}") != null ? primaryName : fallbackName;
    }
}
