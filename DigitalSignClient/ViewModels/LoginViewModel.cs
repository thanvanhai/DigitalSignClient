using DigitalSignClient.Helpers;
using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DigitalSignClient.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;

            // ✅ Giá trị mặc định (demo)
            Username = "admin";
            Password = "Admin@123";

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        public event EventHandler<LoginResponse>? LoginSuccessful;

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new LoginRequest
                {
                    Username = Username,
                    Password = Password
                };

                var response = await _authService.LoginAsync(request);

                if (response != null)
                {
                    // ✅ Token đã được lưu vào ApiManager bên trong AuthService
                    LoginSuccessful?.Invoke(this, response);
                }
                else
                {
                    ErrorMessage = "Đăng nhập thất bại. Vui lòng kiểm tra lại tên đăng nhập và mật khẩu.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message switch
                {
                    string msg when msg.Contains("máy chủ") => msg,
                    string msg when msg.Contains("thời gian chờ") => msg,
                    _ => "Không thể kết nối tới máy chủ. Vui lòng kiểm tra lại, có thể server chưa được mở."
                };
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
