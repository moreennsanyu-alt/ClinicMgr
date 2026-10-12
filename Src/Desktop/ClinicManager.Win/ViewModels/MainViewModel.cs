using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClinicManager.Win.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
         [ObservableProperty]
         bool _NavigationPaneVisibility;

         [ObservableProperty]
         RootNavigationItem _selectedRootNavigationItem;
         
         [ObservableProperty]
         NavigationItem _selectedNavigationItem;
         
         [RelayCommand]
         public async Task OnLoaded()
         {
              await Task.CompletedTask; 
         }
          
    public ObservableCollection<RootNavigationItem> NavigationItems { get; } = new();

    partial void OnSelectedRootNavigationItemChanging(RootNavigationItem oldItem,RootNavigationItem newItem)
    {
        // runs before the field is set; value is the NEW value
    }

    partial void OnSelectedRootNavigationItemChanged(RootNavigationItem oldItem,RootNavigationItem newItem)
    {
        if(newItem.SelectedItem == null)
           SelectedNavigationItem = NavignewItem.Items.First();
        else 
           SelectedNavigationItem = newItem.SelectedItem;
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem oldItem,NavigationItem newItem)
    {
        if(SelectedRootNavigationItem.SelectedItem != newItem)
           SelectedRootNavigationItem.SelectedItem = newItem;
        Navigate(newItem);
    }

    public void Navigate(NavigationItem item)
    {

    
    }
    partial void OnSelectedNavigationItemChanging(NavigationItem oldItem,NavigationItem newItem)
    {
        // ...
    }
    }
}
