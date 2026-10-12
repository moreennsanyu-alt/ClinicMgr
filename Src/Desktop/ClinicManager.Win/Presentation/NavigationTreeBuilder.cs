namespace ClinicManager.Win.Presentation.Mvvm;

public interface INavigationTreeBuilder
{
    ObservableCollection<NavigationItem> Root { get; }
    void Register(NavigationItem item);
}

public class NavigationTreeBuilder : INavigationTreeBuilder
{
    public ObservableCollection<NavigationItem> Root { get; } = new();

    public void Register(NavigationItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Key))
            throw new ArgumentException("NavigationItem.Key must not be null or empty.");

        if (string.IsNullOrWhiteSpace(item.Path))
            throw new ArgumentException($"NavigationItem '{item.Key}' has no Path.");

        var segments = item.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var level = Root;

        for (int i = 0; i < segments.Length; i++)
        {
            bool isLeaf = i == segments.Length - 1;
            var key = segments[i];
            var existing = level.FirstOrDefault(n => n.Key == key);

            if (existing is null)
            {
                var node = isLeaf
                    ? item
                    : new NavigationItem
                    {
                        Key = key,
                        Title = key,
                        Path = string.Join("/", segments.Take(i + 1))
                    };
                InsertSorted(level, node);
                existing = node;
            }
            else if (isLeaf)
            {
                if (!string.IsNullOrEmpty(existing.ViewName))
                    throw new InvalidOperationException(
                        $"NavigationItem at path '{item.Path}' is already registered with ViewName '{existing.ViewName}'.");

                // fill in the placeholder with real leaf data
                existing.Title = item.Title ?? existing.Title;
                existing.ViewName = item.ViewName;
                //existing.Parameters = item.Parameters;
                existing.Priority = item.Priority;
                existing.BadgeText = item.BadgeText;
                existing.BadgeType = item.BadgeType;
                existing.IsEnabled = item.IsEnabled;
            }

            level = existing.Children;
        }
    }

    private static void InsertSorted(ObservableCollection<NavigationItem> level, NavigationItem node)
    {
        int index = 0;
        while (index < level.Count && level[index].Priority <= node.Priority)
            index++;
        level.Insert(index, node);
    }
}
