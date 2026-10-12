using System.Windows;
using System.Windows.Controls;

namespace ClinicManager.Win.Presentation.Controls
{
    /// <summary>
    /// Base class for all feature views (e.g. HomeView, ChangePasswordView).
    /// Provides a common ViewTitle for use by chrome such as <see cref="ViewHeader"/>,
    /// plus load/unload hooks that derived views can override instead of wiring
    /// the Loaded/Unloaded events directly.
    /// </summary>
    public abstract class ViewBase : UserControl
    {
        public static readonly DependencyProperty ViewTitleProperty =
            DependencyProperty.Register(
                nameof(ViewTitle),
                typeof(string),
                typeof(ViewBase),
                new PropertyMetadata(string.Empty));

        public string ViewTitle
        {
            get => (string)GetValue(ViewTitleProperty);
            set => SetValue(ViewTitleProperty, value);
        }

        protected ViewBase()
        {
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        /// <summary>
        /// Called when the view is loaded into the visual tree. Override to
        /// perform initialization that depends on the view being loaded.
        /// </summary>
        protected virtual void OnViewLoaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Called when the view is removed from the visual tree. Override to
        /// release resources or unsubscribe from events.
        /// </summary>
        protected virtual void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
