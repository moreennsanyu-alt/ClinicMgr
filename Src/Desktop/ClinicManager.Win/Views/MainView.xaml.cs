using System.Windows.Controls;
using ClinicManager.Win.ViewModels;

namespace ClinicManager.Win.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
