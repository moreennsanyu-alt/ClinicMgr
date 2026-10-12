namespace ClinicManager.Win.Presentation;

public static class NavigationItemExtensions
{
    public static NavigationItem FindByKey(this IEnumerable<NavigationItem> items, string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        foreach (var item in items)
        {
            if (item.Key == key) return item;

            var found = item.Children.FindByKey(key);
            if (found != null) return found;
        }
        return null;
    }
}
