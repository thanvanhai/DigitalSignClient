using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DigitalSignClient.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiManager _apiManager;

        public AuthService(HttpClient httpClient, ApiManager apiManager)
        {
            _httpClient = httpClient;
            _apiManager = apiManager;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Auth/login", content);

                if (!response.IsSuccessStatusCode)
                    return null;

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                if (loginResponse != null)
                    _apiManager.SetToken(loginResponse.Token);

                return loginResponse;
            }
            catch (HttpRequestException)
            {
                throw new Exception($"Không thể kết nối tới máy chủ {_httpClient.BaseAddress}");
            }
        }
    }
}
