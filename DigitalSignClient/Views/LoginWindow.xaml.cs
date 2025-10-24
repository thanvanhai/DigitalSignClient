using DigitalSignClient.Models;
using DigitalSignClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DigitalSignClient.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // ✅ Đồng bộ password mặc định hiển thị trong UI
            PasswordBox.Password = _viewModel.Password;
            _viewModel.LoginSuccessful += OnLoginSuccessful;
        }

        // Event handler cho PasswordBox
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void OnLoginSuccessful(object? sender, LoginResponse e)
        {
            var apiService = _viewModel.ApiService;
            var mainViewModel = new MainViewModel(apiService)
            {
                CurrentUser = e.User
            };

            var mainWindow = new MainWindow(mainViewModel);

            // Hiển thị MainWindow trước khi đóng LoginWindow
            mainWindow.Show();

            // Hủy đăng ký event để tránh memory leak
            _viewModel.LoginSuccessful -= OnLoginSuccessful;

            // Đóng LoginWindow
            this.Close();
        }
    }
}