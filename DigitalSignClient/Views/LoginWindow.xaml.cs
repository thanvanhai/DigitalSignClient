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

            _viewModel.LoginSuccessful += OnLoginSuccessful;
        }

        // THÊM EVENT HANDLER NÀY
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordBox.Password;
        }

        private void OnLoginSuccessful(object? sender, LoginResponse e)
        {
            var apiService = _viewModel.ApiService;
            var mainViewModel = new MainViewModel(apiService)
            {
                CurrentUser = e.User
            };

            var mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();
            Close();
        }
    }
}
