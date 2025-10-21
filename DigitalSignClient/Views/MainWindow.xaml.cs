using DigitalSignClient.ViewModels;
using System.Windows;

namespace DigitalSignClient.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDocumentsAsync();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var apiService = new Services.ApiService();
                var loginViewModel = new LoginViewModel(apiService);
                var loginWindow = new LoginWindow(loginViewModel);
                loginWindow.Show();
                Close();
            }
        }
    }
}