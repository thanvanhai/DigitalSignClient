using DigitalSignClient.Helpers;
using DigitalSignClient.Models;
using DigitalSignClient.Services;
using DigitalSignClient.Views;
using System.Windows;
using System.Windows.Input;

namespace DigitalSignClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private object? _currentView;
        private User? _currentUser;

        public MainViewModel(ApiService apiService)
        {
            _apiService = apiService;

            ShowDashboardCommand = new DelegateCommand<object>(_ => ShowDashboard());
            ShowDocumentTypeCommand = new DelegateCommand<object>(_ => ShowDocumentType());
            ShowWorkflowCommand = new DelegateCommand<object>(_ => ShowWorkflow());
            ShowDocumentListCommand = new DelegateCommand<object>(_ => ShowDocumentList());
            LogoutCommand = new DelegateCommand<object>(_ => Logout());

            // Hiển thị Dashboard mặc định
            ShowDashboard();
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ApiService ApiService => _apiService;

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowDocumentTypeCommand { get; }
        public ICommand ShowWorkflowCommand { get; }
        public ICommand ShowDocumentListCommand { get; }
        public ICommand LogoutCommand { get; }

        private void ShowDashboard()
        {
            // TODO: Tạo DashboardView
            CurrentView = null;
        }

        private void ShowDocumentType()
        {
            // TODO: Tạo DocumentTypeView
            CurrentView = null;
        }

        private void ShowWorkflow()
        {
            // TODO: Tạo WorkflowView
            CurrentView = null;
        }

        private void ShowDocumentList()
        {
            var viewModel = new DocumentListViewModel(_apiService)
            {
                CurrentUser = this.CurrentUser
            };
            CurrentView = new DocumentListView(viewModel);
        }

        private void Logout()
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Tìm và đóng MainWindow
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        // Mở LoginWindow
                        var loginViewModel = new LoginViewModel(_apiService);
                        var loginWindow = new LoginWindow(loginViewModel);
                        loginWindow.Show();

                        window.Close();
                        break;
                    }
                }
            }
        }
    }
}