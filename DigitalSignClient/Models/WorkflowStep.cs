using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Models
{
    /// <summary>
    /// DTO để nhận workflow step từ API
    /// </summary>
    public class WorkflowStepDto
    {
        public Guid Id { get; set; }
        public int Level { get; set; }
        public string Role { get; set; } = string.Empty;
        public string SignatureType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string? NodeType { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO để tạo mới workflow step
    /// </summary>
    public class WorkflowStepCreateDto
    {
        public int Level { get; set; }
        public string Role { get; set; } = string.Empty;
        public string SignatureType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string? NodeType { get; set; }
    }

}
