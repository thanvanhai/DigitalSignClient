using DigitalSignClient.ViewModels;

public class WorkflowConnectionViewModel : BaseViewModel
{
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
}