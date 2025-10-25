using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Models
{
    /// <summary>
    /// DTO để nhận workflow connection từ API
    /// </summary>
    public class WorkflowConnectionDto
    {
        public Guid Id { get; set; }
        public Guid SourceStepId { get; set; }
        public Guid TargetStepId { get; set; }
        public string? Condition { get; set; }
        public int Priority { get; set; }
        public string? Label { get; set; }
    }

    /// <summary>
    /// DTO để tạo mới workflow connection
    /// </summary>
    public class WorkflowConnectionCreateDto
    {
        public Guid SourceStepId { get; set; }
        public Guid TargetStepId { get; set; }
        public string? Condition { get; set; } = "auto";
        public int Priority { get; set; } = 0;
        public string? Label { get; set; }
    }
}
