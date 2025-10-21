using DigitalSignClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Services
{
    public interface IApiService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<List<Document>?> GetDocumentsAsync();
        Task<Document?> UploadDocumentAsync(string filePath, string? description);
        Task<bool> SignDocumentAsync(Guid documentId, string reason, string location);
        Task<byte[]?> DownloadDocumentAsync(Guid documentId);
        Task<bool> DeleteDocumentAsync(Guid documentId);
    }

    public class ApiService : IApiService 
    {
        private readonly HttpClient _httpClient;
        private string? _token;

        public ApiService()
        {
            // BỎ QUA SSL CERTIFICATE CHO DEVELOPMENT
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7159/api/")
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Log để debug
                    System.Diagnostics.Debug.WriteLine($"Login Response: {responseContent}");

                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    if (loginResponse != null)
                    {
                        SetToken(loginResponse.Token);
                    }

                    return loginResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Login Error: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login exception: {ex.Message}");
                System.Windows.MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<List<Document>?> GetDocumentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("Documents");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Document>>(content);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get documents error: {ex.Message}");
                return null;
            }
        }

        public async Task<Document?> UploadDocumentAsync(string filePath, string? description)
        {
            try
            {
                using var form = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                form.Add(fileContent, "file", Path.GetFileName(filePath));

                if (!string.IsNullOrEmpty(description))
                {
                    form.Add(new StringContent(description), "description");
                }

                var response = await _httpClient.PostAsync("Documents/upload", form);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Document>(content);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Upload error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SignDocumentAsync(Guid documentId, string reason, string location)
        {
            try
            {
                var request = new
                {
                    reason,
                    location
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"Documents/{documentId}/sign", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sign error: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]?> DownloadDocumentAsync(Guid documentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Documents/{documentId}/download");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Download error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Documents/{documentId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete error: {ex.Message}");
                return false;
            }
        }
    }
}