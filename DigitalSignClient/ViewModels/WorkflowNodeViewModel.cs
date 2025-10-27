using DigitalSignClient.ViewModels;

public class WorkflowNodeViewModel : BaseViewModel
{
    private Guid _id;
    private string _title = string.Empty;
    private string _role = string.Empty;
    private string _signatureType = string.Empty;
    private string _nodeType = "step";
    private double _x;
    private double _y;
    private int _level;
    private bool _isSelected;

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public string SignatureType
    {
        get => _signatureType;
        set => SetProperty(ref _signatureType, value);
    }

    public string NodeType
    {
        get => _nodeType;
        set => SetProperty(ref _nodeType, value);
    }

    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    public int Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}