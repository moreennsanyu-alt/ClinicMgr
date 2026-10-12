using System.Windows;

namespace ClinicManager.Win.Features.Infrastructure.Authentication
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var viewModel = new LoginViewModel();
            viewModel.LoginSucceeded += OnLoginSucceeded;
            DataContext = viewModel;
        }

        private void OnLoginSucceeded(object? sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
