using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignClient.Models
{
    public class SignatureInfo
    {
        public Guid Id { get; set; }
        public string SignerName { get; set; } = string.Empty;
        public DateTime SignedAt { get; set; }
        public bool IsValid { get; set; }
        public string? Reason { get; set; }
        public string? Location { get; set; }
    }
}
