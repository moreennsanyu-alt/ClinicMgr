using System.Windows.Controls;

namespace ClinicManager.Win.Features.Infrastructure.Authentication
{
    /// <summary>
    /// Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView : UserControl
    {
        public ChangePasswordView()
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel();
        }
    }
}
