using System;
using System.Threading.Tasks;

namespace DigitalSignClient.Services
{
    /// <summary>
    /// Quản lý token xác thực, trạng thái người dùng và tiện ích kiểm tra đăng nhập.
    /// Được inject duy nhất (Singleton) vào toàn ứng dụng.
    /// </summary>
    public class ApiManager
    {
        private string? _token;
        private DateTime? _tokenExpiry;
        private readonly object _lock = new();

        /// <summary>
        /// Lưu token kèm thời gian hết hạn (nếu có).
        /// </summary>
        public void SetToken(string token, DateTime? expiry = null)
        {
            lock (_lock)
            {
                _token = token;
                _tokenExpiry = expiry;
            }
        }

        /// <summary>
        /// Lấy token hiện tại.
        /// </summary>
        public string? GetToken()
        {
            lock (_lock)
            {
                return _token;
            }
        }

        /// <summary>
        /// Kiểm tra xem đã có token hợp lệ chưa.
        /// </summary>
        public bool HasValidToken
        {
            get
            {
                lock (_lock)
                {
                    if (string.IsNullOrEmpty(_token)) return false;
                    if (_tokenExpiry == null) return true;
                    return DateTime.UtcNow < _tokenExpiry.Value;
                }
            }
        }

        /// <summary>
        /// Xoá token (khi đăng xuất).
        /// </summary>
        public void ClearToken()
        {
            lock (_lock)
            {
                _token = null;
                _tokenExpiry = null;
            }
        }

        /// <summary>
        /// Thời gian hết hạn token (nếu có).
        /// </summary>
        public DateTime? TokenExpiry
        {
            get
            {
                lock (_lock)
                {
                    return _tokenExpiry;
                }
            }
        }

        /// <summary>
        /// Thực hiện hành động nào đó nếu token còn hạn, ngược lại ném lỗi.
        /// </summary>
        public Task EnsureAuthenticatedAsync()
        {
            if (!HasValidToken)
                throw new InvalidOperationException("Người dùng chưa đăng nhập hoặc token đã hết hạn.");
            return Task.CompletedTask;
        }
    }
}
