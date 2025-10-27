using System;

namespace DigitalSignClient.Models
{
    public class WorkflowConnectionDto
    {
        public Guid Id { get; set; }
        public Guid SourceStepId { get; set; }
        public Guid TargetStepId { get; set; }
        public string? Condition { get; set; }
        public int? Order { get; set; }
    }

    public class WorkflowConnectionCreateDto
    {
        public Guid SourceStepId { get; set; }
        public Guid TargetStepId { get; set; }
        public string? Condition { get; set; }
        public int? Order { get; set; }
    }
}
