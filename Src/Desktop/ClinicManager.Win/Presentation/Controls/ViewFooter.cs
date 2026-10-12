using System.Windows;
using System.Windows.Controls;

namespace ClinicManager.Win.Presentation.Controls
{
    /// <summary>
    /// Reusable, themeable footer chrome for views (status text, busy state).
    /// Derive from this class to customize behavior, or style it directly via
    /// a matching ControlTemplate in Themes/Generic.xaml.
    /// </summary>
    public class ViewFooter : Control
    {
        static ViewFooter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ViewFooter),
                new FrameworkPropertyMetadata(typeof(ViewFooter)));
        }

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(
                nameof(StatusText),
                typeof(string),
                typeof(ViewFooter),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(
                nameof(IsBusy),
                typeof(bool),
                typeof(ViewFooter),
                new PropertyMetadata(false));

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }
    }
}
