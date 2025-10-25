using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DigitalSignClient.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly HttpClient _httpClient;

        public DocumentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Document>?> GetDocumentsAsync()
        {
            var response = await _httpClient.GetAsync("Documents");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Document>>(content);
        }

        public async Task<Document?> UploadDocumentAsync(string filePath, string? description, Guid documentTypeId)
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            if (!string.IsNullOrEmpty(description))
                form.Add(new StringContent(description), "description");

            // Thêm documentTypeId
            form.Add(new StringContent(documentTypeId.ToString()), "documentTypeId");

            var response = await _httpClient.PostAsync("Documents/upload", form);//truyền dữ liệu qua body thay vì string
            //var response = await _httpClient.PostAsync($"Documents/upload?documentTypeId={documentTypeId}",form);//truyền dữ liệu qua query string thay vì body

            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Document>(content);
        }

        public async Task<bool> UploadSignedDocumentAsync(Guid documentId, string signedFilePath)
        {
            using var form = new MultipartFormDataContent();
            using var fs = File.OpenRead(signedFilePath);
            form.Add(new StreamContent(fs), "file", Path.GetFileName(signedFilePath));

            var response = await _httpClient.PostAsync($"Documents/{documentId}/upload-signed", form);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SignDocumentAsync(Guid documentId, string reason, string location)
        {
            var json = JsonConvert.SerializeObject(new { reason, location });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"Documents/{documentId}/sign", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<byte[]?> DownloadDocumentAsync(Guid documentId)
        {
            var response = await _httpClient.GetAsync($"Documents/{documentId}/download");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            var response = await _httpClient.DeleteAsync($"Documents/{documentId}");
            return response.IsSuccessStatusCode;
        }
    }
}
