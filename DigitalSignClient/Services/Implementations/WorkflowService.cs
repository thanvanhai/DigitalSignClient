using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace DigitalSignClient.Services.Implementations
{
    public class WorkflowService : IWorkflowService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/Workflow";

        public WorkflowService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WorkflowTemplateDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<WorkflowTemplateDto>>(BaseUrl);
                return response ?? new List<WorkflowTemplateDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách workflow: {ex.Message}", ex);
            }
        }

        public async Task<WorkflowTemplateDto?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<WorkflowTemplateDto>($"{BaseUrl}/{id}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy workflow {id}: {ex.Message}", ex);
            }
        }

        public async Task<CreatedResponse> CreateAsync(WorkflowTemplateCreateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CreatedResponse>();
                return result ?? throw new Exception("Không nhận được Id từ server");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo workflow: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Guid id, WorkflowTemplateCreateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật workflow: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa workflow: {ex.Message}", ex);
            }
        }

        public async Task<WorkflowValidationResult> ValidateAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<WorkflowValidationResult>($"{BaseUrl}/{id}/validate");
                return response ?? new WorkflowValidationResult { IsValid = false, Message = "Không nhận được kết quả từ server" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi validate workflow: {ex.Message}", ex);
            }
        }
    }
}
