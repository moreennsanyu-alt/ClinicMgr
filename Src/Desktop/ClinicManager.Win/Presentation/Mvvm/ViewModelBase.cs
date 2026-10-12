
namespace ClinicManager.Win.Presentation.Mvvm;


public class ViewModelBase : ObservableObject, ISupportServices
{
    IServiceContainer serviceContainer = null;
    protected IServiceContainer ServiceContainer {
        get {
            if(serviceContainer == null)
                serviceContainer = new ServiceContainer(this);
            return serviceContainer; 
        }
    }
    IServiceContainer ISupportServices.ServiceContainer { get { return ServiceContainer; } }
    
}
