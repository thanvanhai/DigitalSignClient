using DigitalSignClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<List<WorkflowTemplateDto>> GetAllAsync();
        Task<WorkflowTemplateDto?> GetByIdAsync(Guid id);
        Task<CreatedResponse> CreateAsync(WorkflowTemplateCreateDto dto);
        Task UpdateAsync(Guid id, WorkflowTemplateCreateDto dto);
        Task DeleteAsync(Guid id);
        Task<WorkflowValidationResult> ValidateAsync(Guid id);
    }
}
