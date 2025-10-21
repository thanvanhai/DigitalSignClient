using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsSigned { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? Description { get; set; }
        public string UploadedByUsername { get; set; } = string.Empty;
        public int SignatureCount { get; set; }
    }
}
