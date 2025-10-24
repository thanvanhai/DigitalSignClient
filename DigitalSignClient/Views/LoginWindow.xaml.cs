using DigitalSignClient.Models;
using DigitalSignClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace DigitalSignClient.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            PasswordBox.Password = _viewModel.Password;
            _viewModel.LoginSuccessful += OnLoginSuccessful;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                _viewModel.Password = passwordBox.Password;
        }

        private void OnLoginSuccessful(object? sender, LoginResponse e)
        {
            // ✅ Lấy MainViewModel từ DI container
            var mainViewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel.CurrentUser = e.User;

            var mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();

            // Hủy đăng ký event để tránh memory leak
            _viewModel.LoginSuccessful -= OnLoginSuccessful;
            this.Close();
        }
    }
}
