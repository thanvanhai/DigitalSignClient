using DigitalSignClient.Helpers;
using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using DigitalSignClient.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace DigitalSignClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentTypeService _documentTypeService;
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        private object? _currentView;
        private User? _currentUser;

        public MainViewModel(
            IAuthService authService,
            IDocumentService documentService,
            IDocumentTypeService documentTypeService,
            IServiceProvider serviceProvider)
        {
            _authService = authService;
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            _serviceProvider = serviceProvider;

            ShowDashboardCommand = new DelegateCommand<object>(_ => ShowDashboard());
            ShowDocumentTypeCommand = new DelegateCommand<object>(_ => ShowDocumentType());
            ShowWorkflowCommand = new DelegateCommand<object>(_ => ShowWorkflow());
            ShowDocumentListCommand = new DelegateCommand<object>(_ => ShowDocumentList());
            LogoutCommand = new DelegateCommand<object>(_ => Logout());

            // Mở dashboard mặc định
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

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowDocumentTypeCommand { get; }
        public ICommand ShowWorkflowCommand { get; }
        public ICommand ShowDocumentListCommand { get; }
        public ICommand LogoutCommand { get; }

        private void ShowDashboard()
        {
            CurrentView = new DashboardView();
        }

        private void ShowDocumentType()
        {
            var viewModel = _serviceProvider.GetRequiredService<DocumentTypeViewModel>();
            CurrentView = new DocumentTypeView(viewModel);
        }

        private void ShowWorkflow()
        {
            CurrentView = new WorkflowDesignerView();
        }

        private void ShowDocumentList()
        {
            // ⚙️ Lấy ViewModel từ DI container
            var viewModel = new DocumentListViewModel(_documentService, _documentTypeService )
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

            if (result != MessageBoxResult.Yes)
                return;

            // 🔁 Quay lại màn hình đăng nhập
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                    var loginWindow = new LoginWindow(loginViewModel);
                    loginWindow.Show();

                    window.Close();
                    break;
                }
            }
        }
    }
}
