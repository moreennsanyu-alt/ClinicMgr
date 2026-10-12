using System.Windows;
using System.Windows.Controls;

namespace ClinicManager.Win.Presentation.Controls
{
    /// <summary>
    /// Reusable, themeable header chrome for views. Derive from this class
    /// to customize behavior, or style it directly via a matching
    /// ControlTemplate in Themes/Generic.xaml.
    /// </summary>
    public class ViewHeader : Control
    {
        static ViewHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ViewHeader),
                new FrameworkPropertyMetadata(typeof(ViewHeader)));
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ViewHeader),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(ViewHeader),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
    }
}
