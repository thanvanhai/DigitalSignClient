using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Models
{
    /// <summary>
    /// DTO để nhận workflow template từ API
    /// </summary>
    public class WorkflowTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DocumentTypeId { get; set; }
        public string? DocumentTypeName { get; set; }
        public List<WorkflowStepDto> Steps { get; set; } = new();
        public List<WorkflowConnectionDto> Connections { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO để tạo mới hoặc update workflow template
    /// </summary>
    public class WorkflowTemplateCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid DocumentTypeId { get; set; }
        public List<WorkflowStepCreateDto> Steps { get; set; } = new();
        public List<WorkflowConnectionCreateDto> Connections { get; set; } = new();
    }
}
