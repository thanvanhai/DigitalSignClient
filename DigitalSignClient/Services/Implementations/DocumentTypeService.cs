using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DigitalSignClient.Services.Implementations
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly HttpClient _httpClient;

        public DocumentTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DocumentType>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("DocumentType");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DocumentType>>(content);
        }

        public async Task<DocumentType?> CreateAsync(DocumentType dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("DocumentType", content);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DocumentType>(responseContent);
        }

        public async Task<DocumentType?> UpdateAsync(Guid id, DocumentType dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"DocumentType/{id}", content);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DocumentType>(responseContent);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"DocumentType/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}