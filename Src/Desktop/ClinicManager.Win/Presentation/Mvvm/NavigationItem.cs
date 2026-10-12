namespace ClinicManager.Win.Presentation.Mvvm;

public enum BadgeType
{
    None,
    Info,
    Warning,
    Error,
    Success
}

public partial class NavigationItem : ViewModelBase
{
    [ObservableProperty]
    string _key;

    [ObservableProperty]
    string _title;
    
    [ObservableProperty]
    string _viewName;

    [ObservableProperty]
    string _path;
    
    [ObservableProperty]
    int _priority;
    
    [ObservableProperty]
    string _badgeText;
    
    [ObservableProperty] 
    BadgeType _badgeType = BadgeType.None;
    
    [ObservableProperty] 
    bool _enabled = true;
    
    [ObservableProperty]
    bool _isSelected = false;
 
    public ObservableCollection<NavigationItem> Items { get; } = new();

}

public partial class RootNavigationItem : NavigationItem
{
    [ObservableProperty]
    NavigationItem _selectedItem = null;

    [ObservableProperty]
    string _defaultKey;
    
}
