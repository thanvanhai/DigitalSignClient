using DigitalSignClient.ViewModels;
using System.Windows;

namespace DigitalSignClient.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
