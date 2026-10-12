using Prism.Ioc;

namespace ClinicManager.Win.Features.Infrastructure;

    public class InfrastructureModule : IModule
    {
        public void RegisterTypes(IContainerRegistry registry)
        {
           
        }

        public void OnInitialized(IContainerProvider container)
        {
            var nav = container.Resolve<INavigationTreeBuilder>();

            //nav.Register(new NavigationItem
            //{
            //    Key = "AllUsers",
            //    Title = "All Users",
            //    Path = "Users/AllUsers",
            //    ViewName = nameof(HomeView)
            //});

            
            //var regions = container.Resolve<IRegionManager>();
            //regions.RegisterViewWithRegion(RegionNames.Content, typeof(OrderListView));
        }
    }
