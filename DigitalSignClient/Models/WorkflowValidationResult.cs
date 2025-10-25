using System.Collections.Generic;

namespace DigitalSignClient.Models
{
    /// <summary>
    /// Kết quả validate workflow từ API
    /// </summary>
    public class WorkflowValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}