using DigitalSignClient.Helpers;
using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DigitalSignClient.ViewModels
{
    public class WorkflowDesignerViewModel : BaseViewModel
    {
        private readonly IWorkflowService _workflowService;
        private readonly IDocumentTypeService _documentTypeService;

        private WorkflowTemplateDto? _selectedWorkflow;
        private WorkflowNodeViewModel? _selectedNode;
        private bool _isLoading;
        private string _workflowName = string.Empty;
        private Guid _selectedDocumentTypeId;
        private string _newNodeRole = string.Empty;
        private string _newNodeSignatureType = "Digital";
        private string _newNodeType = "step";
        private int _nextLevel = 1;

        public WorkflowDesignerViewModel(
            IWorkflowService workflowService,
            IDocumentTypeService documentTypeService)
        {
            _workflowService = workflowService;
            _documentTypeService = documentTypeService;

            WorkflowList = new ObservableCollection<WorkflowTemplateDto>();
            DocumentTypes = new ObservableCollection<DocumentType>();
            Nodes = new ObservableCollection<WorkflowNodeViewModel>();
            Connections = new ObservableCollection<WorkflowConnectionViewModel>();

            LoadWorkflowsCommand = new DelegateCommand<object>(async _ => await LoadWorkflowsAsync());
            CreateNewWorkflowCommand = new DelegateCommand<object>(async _ => await CreateNewWorkflowAsync(), _ => CanCreateWorkflow());
            SaveWorkflowCommand = new DelegateCommand<object>(async _ => await SaveWorkflowAsync(), _ => CanSaveWorkflow());
            DeleteWorkflowCommand = new DelegateCommand<object>(async _ => await DeleteWorkflowAsync(), _ => SelectedWorkflow != null);
            SelectWorkflowCommand = new DelegateCommand<WorkflowTemplateDto>(async w => await SelectWorkflowAsync(w));
            AddNodeCommand = new DelegateCommand<object>(_ => AddNode(), _ => CanAddNode());
            DeleteNodeCommand = new DelegateCommand<object>(_ => DeleteSelectedNode(), _ => SelectedNode != null);
            ConnectNodesCommand = new DelegateCommand<object>(_ => StartConnection());
            ValidateWorkflowCommand = new DelegateCommand<object>(async _ => await ValidateWorkflowAsync(), _ => SelectedWorkflow != null);

            _ = LoadDocumentTypesAsync();
            _ = LoadWorkflowsAsync();
        }

        // Properties
        public ObservableCollection<WorkflowTemplateDto> WorkflowList { get; }
        public ObservableCollection<DocumentType> DocumentTypes { get; }
        public ObservableCollection<WorkflowNodeViewModel> Nodes { get; }
        public ObservableCollection<WorkflowConnectionViewModel> Connections { get; }

        public WorkflowTemplateDto? SelectedWorkflow
        {
            get => _selectedWorkflow;
            set
            {
                SetProperty(ref _selectedWorkflow, value);
                (DeleteWorkflowCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
                (ValidateWorkflowCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        public WorkflowNodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != null)
                    _selectedNode.IsSelected = false;

                SetProperty(ref _selectedNode, value);

                if (_selectedNode != null)
                    _selectedNode.IsSelected = true;

                (DeleteNodeCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string WorkflowName
        {
            get => _workflowName;
            set
            {
                SetProperty(ref _workflowName, value);
                (CreateNewWorkflowCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
                (SaveWorkflowCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        public Guid SelectedDocumentTypeId
        {
            get => _selectedDocumentTypeId;
            set
            {
                SetProperty(ref _selectedDocumentTypeId, value);
                (CreateNewWorkflowCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        public string NewNodeRole
        {
            get => _newNodeRole;
            set
            {
                SetProperty(ref _newNodeRole, value);
                (AddNodeCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        public string NewNodeSignatureType
        {
            get => _newNodeSignatureType;
            set => SetProperty(ref _newNodeSignatureType, value);
        }

        public string NewNodeType
        {
            get => _newNodeType;
            set => SetProperty(ref _newNodeType, value);
        }

        // Commands
        public ICommand LoadWorkflowsCommand { get; }
        public ICommand CreateNewWorkflowCommand { get; }
        public ICommand SaveWorkflowCommand { get; }
        public ICommand DeleteWorkflowCommand { get; }
        public ICommand SelectWorkflowCommand { get; }
        public ICommand AddNodeCommand { get; }
        public ICommand DeleteNodeCommand { get; }
        public ICommand ConnectNodesCommand { get; }
        public ICommand ValidateWorkflowCommand { get; }

        // Methods
        private async Task LoadDocumentTypesAsync()
        {
            try
            {
                var types = await _documentTypeService.GetAllAsync();
                DocumentTypes.Clear();
                foreach (var type in types)
                {
                    DocumentTypes.Add(type);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải loại văn bản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadWorkflowsAsync()
        {
            try
            {
                IsLoading = true;
                var workflows = await _workflowService.GetAllAsync();
                WorkflowList.Clear();
                foreach (var wf in workflows)
                {
                    WorkflowList.Add(wf);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách workflow: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SelectWorkflowAsync(WorkflowTemplateDto workflow)
        {
            SelectedWorkflow = workflow;
            WorkflowName = workflow.Name;
            SelectedDocumentTypeId = workflow.DocumentTypeId;

            // Load nodes lên canvas
            Nodes.Clear();
            Connections.Clear();

            foreach (var step in workflow.Steps)
            {
                var node = new WorkflowNodeViewModel
                {
                    Id = step.Id,
                    Title = $"{step.NodeType.ToUpper()} - Level {step.Level}",
                    Role = step.Role,
                    SignatureType = step.SignatureType,
                    NodeType = step.NodeType,
                    X = step.PositionX,
                    Y = step.PositionY,
                    Level = step.Level
                };
                Nodes.Add(node);
            }

            // Load connections
            foreach (var conn in workflow.Connections)
            {
                var sourceNode = Nodes.FirstOrDefault(n => n.Id == conn.SourceStepId);
                var targetNode = Nodes.FirstOrDefault(n => n.Id == conn.TargetStepId);

                if (sourceNode != null && targetNode != null)
                {
                    Connections.Add(new WorkflowConnectionViewModel
                    {
                        SourceNodeId = sourceNode.Id,
                        TargetNodeId = targetNode.Id,
                        X1 = sourceNode.X + 60,
                        Y1 = sourceNode.Y + 40,
                        X2 = targetNode.X + 60,
                        Y2 = targetNode.Y + 40
                    });
                }
            }

            _nextLevel = workflow.Steps.Any() ? workflow.Steps.Max(s => s.Level) + 1 : 1;
        }

        private bool CanCreateWorkflow()
        {
            return !string.IsNullOrWhiteSpace(WorkflowName) && SelectedDocumentTypeId != Guid.Empty;
        }

        private async Task CreateNewWorkflowAsync()
        {
            try
            {
                IsLoading = true;

                // Validate input
                if (string.IsNullOrWhiteSpace(WorkflowName))
                {
                    MessageBox.Show("Vui lòng nhập tên workflow!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedDocumentTypeId == Guid.Empty)
                {
                    MessageBox.Show("Vui lòng chọn loại văn bản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dto = new WorkflowTemplateCreateDto
                {
                    Name = WorkflowName,
                    DocumentTypeId = SelectedDocumentTypeId,
                    Steps = new List<WorkflowStepCreateDto>(),
                    Connections = new List<WorkflowConnectionCreateDto>()
                };

                var result = await _workflowService.CreateAsync(dto);

                MessageBox.Show("Tạo workflow thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                WorkflowName = string.Empty;
                Nodes.Clear();
                Connections.Clear();
                _nextLevel = 1;

                await LoadWorkflowsAsync();

                // Select workflow vừa tạo
                var newWorkflow = WorkflowList.FirstOrDefault(w => w.Id == result.Id);
                if (newWorkflow != null)
                {
                    await SelectWorkflowAsync(newWorkflow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo workflow: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSaveWorkflow()
        {
            return SelectedWorkflow != null && !string.IsNullOrWhiteSpace(WorkflowName);
        }

        private async Task SaveWorkflowAsync()
        {
            if (SelectedWorkflow == null) return;

            try
            {
                IsLoading = true;

                var dto = new WorkflowTemplateCreateDto
                {
                    Name = WorkflowName,
                    DocumentTypeId = SelectedDocumentTypeId,
                    Steps = Nodes.Select(n => new WorkflowStepCreateDto
                    {
                        Level = n.Level,
                        Role = n.Role,
                        SignatureType = n.SignatureType,
                        NodeType = n.NodeType,
                        PositionX = n.X,
                        PositionY = n.Y,
                        Description = n.Title
                    }).ToList(),
                    Connections = Connections.Select((c, idx) => new WorkflowConnectionCreateDto
                    {
                        SourceStepId = c.SourceNodeId,
                        TargetStepId = c.TargetNodeId,
                        Order = idx
                    }).ToList()
                };

                await _workflowService.UpdateAsync(SelectedWorkflow.Id, dto);

                MessageBox.Show("Lưu workflow thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadWorkflowsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu workflow: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteWorkflowAsync()
        {
            if (SelectedWorkflow == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa workflow '{SelectedWorkflow.Name}'?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _workflowService.DeleteAsync(SelectedWorkflow.Id);

                MessageBox.Show("Xóa workflow thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                Nodes.Clear();
                Connections.Clear();
                SelectedWorkflow = null;
                WorkflowName = string.Empty;

                await LoadWorkflowsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa workflow: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAddNode()
        {
            return !string.IsNullOrWhiteSpace(NewNodeRole);
        }

        private void AddNode()
        {
            var node = new WorkflowNodeViewModel
            {
                Id = Guid.NewGuid(),
                Title = $"{NewNodeType.ToUpper()} - Level {_nextLevel}",
                Role = NewNodeRole,
                SignatureType = NewNodeSignatureType,
                NodeType = NewNodeType,
                Level = _nextLevel,
                X = 100 + (Nodes.Count * 30),
                Y = 100 + (Nodes.Count * 30)
            };

            Nodes.Add(node);
            _nextLevel++;

            NewNodeRole = string.Empty;
        }

        private void DeleteSelectedNode()
        {
            if (SelectedNode == null) return;

            // Xóa các connections liên quan
            var relatedConnections = Connections
                .Where(c => c.SourceNodeId == SelectedNode.Id || c.TargetNodeId == SelectedNode.Id)
                .ToList();

            foreach (var conn in relatedConnections)
            {
                Connections.Remove(conn);
            }

            Nodes.Remove(SelectedNode);
            SelectedNode = null;
        }

        private WorkflowNodeViewModel? _connectionSource;

        private void StartConnection()
        {
            if (SelectedNode == null) return;

            if (_connectionSource == null)
            {
                _connectionSource = SelectedNode;
                MessageBox.Show($"Đã chọn node nguồn. Hãy chọn node đích.", "Kết nối", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (_connectionSource.Id == SelectedNode.Id)
                {
                    MessageBox.Show("Không thể kết nối node với chính nó!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _connectionSource = null;
                    return;
                }

                // Tạo connection
                Connections.Add(new WorkflowConnectionViewModel
                {
                    SourceNodeId = _connectionSource.Id,
                    TargetNodeId = SelectedNode.Id,
                    X1 = _connectionSource.X + 60,
                    Y1 = _connectionSource.Y + 40,
                    X2 = SelectedNode.X + 60,
                    Y2 = SelectedNode.Y + 40
                });

                _connectionSource = null;
            }
        }

        private async Task ValidateWorkflowAsync()
        {
            if (SelectedWorkflow == null) return;

            try
            {
                IsLoading = true;
                var result = await _workflowService.ValidateAsync(SelectedWorkflow.Id);

                if (result.IsValid)
                {
                    MessageBox.Show("Workflow hợp lệ!", "Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errors = result.Errors != null ? string.Join("\n", result.Errors) : result.Message;
                    MessageBox.Show($"Workflow không hợp lệ:\n{errors}", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi validate: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void UpdateNodePosition(Guid nodeId, double x, double y)
        {
            var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node != null)
            {
                node.X = x;
                node.Y = y;

                // Update connections
                foreach (var conn in Connections.Where(c => c.SourceNodeId == nodeId))
                {
                    conn.X1 = x + 60;
                    conn.Y1 = y + 40;
                }

                foreach (var conn in Connections.Where(c => c.TargetNodeId == nodeId))
                {
                    conn.X2 = x + 60;
                    conn.Y2 = y + 40;
                }
            }
        }
    }
}