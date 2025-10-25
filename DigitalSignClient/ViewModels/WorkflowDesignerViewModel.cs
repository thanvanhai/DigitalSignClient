using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace DigitalSignClient.ViewModels
{
    public partial class WorkflowDesignerViewModel : ObservableObject
    {
        private readonly IWorkflowService _workflowService;

        [ObservableProperty]
        private ObservableCollection<WorkflowNodeViewModel> nodes = new();

        [ObservableProperty]
        private ObservableCollection<WorkflowConnectionViewModel> connections = new();

        [ObservableProperty]
        private string workflowName = "Workflow mới";

        [ObservableProperty]
        private Guid? currentWorkflowId;

        [ObservableProperty]
        private Guid selectedDocumentTypeId;

        [ObservableProperty]
        private bool isLoading;

        // Pending connection (khi user đang kéo connection)
        [ObservableProperty]
        private WorkflowNodeViewModel? pendingConnectionSource;

        public WorkflowDesignerViewModel(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [RelayCommand]
        private void AddNode(string nodeType)
        {
            var newNode = new WorkflowNodeViewModel
            {
                Id = Guid.NewGuid(),
                NodeType = nodeType,
                Location = new Point(100 + Nodes.Count * 50, 100 + Nodes.Count * 30),
                Level = Nodes.Count + 1
            };

            switch (nodeType)
            {
                case "start":
                    newNode.DisplayName = "Bắt đầu";
                    newNode.Role = "Start";
                    newNode.SignatureType = "N/A";
                    break;
                case "approval":
                    newNode.DisplayName = "Phê duyệt";
                    newNode.Role = "Trưởng phòng";
                    newNode.SignatureType = "Nháy";
                    break;
                case "sign":
                    newNode.DisplayName = "Ký";
                    newNode.Role = "Giám đốc";
                    newNode.SignatureType = "Chính";
                    break;
                case "parallel":
                    newNode.DisplayName = "Song song";
                    newNode.Role = "Kế toán + Hành chính";
                    newNode.SignatureType = "Nháy";
                    break;
                case "end":
                    newNode.DisplayName = "Kết thúc";
                    newNode.Role = "End";
                    newNode.SignatureType = "N/A";
                    break;
                default:
                    newNode.DisplayName = $"Bước {Nodes.Count + 1}";
                    newNode.Role = "Người ký";
                    newNode.SignatureType = "Chính";
                    break;
            }

            Nodes.Add(newNode);
        }

        [RelayCommand]
        private void RemoveNode(WorkflowNodeViewModel node)
        {
            if (node == null) return;

            // Xóa tất cả connections liên quan
            var relatedConnections = Connections
                .Where(c => c.Source == node || c.Target == node)
                .ToList();

            foreach (var conn in relatedConnections)
            {
                Connections.Remove(conn);
            }

            Nodes.Remove(node);
        }

        [RelayCommand]
        private void StartConnection(WorkflowNodeViewModel source)
        {
            PendingConnectionSource = source;
        }

        [RelayCommand]
        private void CompleteConnection(WorkflowNodeViewModel target)
        {
            if (PendingConnectionSource == null || PendingConnectionSource == target)
            {
                PendingConnectionSource = null;
                return;
            }

            // Kiểm tra đã có connection này chưa
            var existingConnection = Connections.FirstOrDefault(c =>
                c.Source == PendingConnectionSource && c.Target == target);

            if (existingConnection == null)
            {
                var newConnection = new WorkflowConnectionViewModel(PendingConnectionSource, target)
                {
                    Condition = "auto",
                    Label = $"{PendingConnectionSource.DisplayName} → {target.DisplayName}"
                };

                Connections.Add(newConnection);
            }

            PendingConnectionSource = null;
        }

        [RelayCommand]
        private void RemoveConnection(WorkflowConnectionViewModel connection)
        {
            if (connection != null)
            {
                Connections.Remove(connection);
            }
        }

        [RelayCommand]
        private async Task SaveWorkflow()
        {
            if (string.IsNullOrWhiteSpace(WorkflowName))
            {
                MessageBox.Show("Vui lòng nhập tên workflow!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDocumentTypeId == Guid.Empty)
            {
                MessageBox.Show("Vui lòng chọn loại văn bản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Nodes.Any())
            {
                MessageBox.Show("Workflow phải có ít nhất 1 bước!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var dto = new WorkflowTemplateCreateDto
                {
                    Name = WorkflowName,
                    DocumentTypeId = SelectedDocumentTypeId,
                    Steps = Nodes.Select(n => new WorkflowStepCreateDto
                    {
                        Level = n.Level,
                        Role = n.Role,
                        SignatureType = n.SignatureType,
                        Description = n.Description,
                        PositionX = n.Location.X,
                        PositionY = n.Location.Y,
                        NodeType = n.NodeType
                    }).ToList(),
                    Connections = Connections.Select(c => new WorkflowConnectionCreateDto
                    {
                        SourceStepId = c.Source.Id,
                        TargetStepId = c.Target.Id,
                        Condition = c.Condition,
                        Priority = c.Priority,
                        Label = c.Label
                    }).ToList()
                };

                if (CurrentWorkflowId.HasValue)
                {
                    await _workflowService.UpdateAsync(CurrentWorkflowId.Value, dto);
                    MessageBox.Show("✅ Cập nhật workflow thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var result = await _workflowService.CreateAsync(dto);
                    CurrentWorkflowId = result.Id;
                    MessageBox.Show("✅ Tạo workflow thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadWorkflow(Guid workflowId)
        {
            IsLoading = true;

            try
            {
                var workflow = await _workflowService.GetByIdAsync(workflowId);

                if (workflow == null)
                {
                    MessageBox.Show("Không tìm thấy workflow!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Clear current data
                Nodes.Clear();
                Connections.Clear();

                // Load basic info
                CurrentWorkflowId = workflow.Id;
                WorkflowName = workflow.Name;
                SelectedDocumentTypeId = workflow.DocumentTypeId;

                // Load nodes
                var nodeMap = new Dictionary<Guid, WorkflowNodeViewModel>();
                foreach (var step in workflow.Steps)
                {
                    var node = new WorkflowNodeViewModel
                    {
                        Id = step.Id,
                        Level = step.Level,
                        Role = step.Role,
                        SignatureType = step.SignatureType,
                        Description = step.Description,
                        Location = new Point(step.PositionX, step.PositionY),
                        NodeType = step.NodeType ?? "sign",
                        DisplayName = $"{step.Role} ({step.SignatureType})"
                    };

                    Nodes.Add(node);
                    nodeMap[step.Id] = node;
                }

                // Load connections
                foreach (var conn in workflow.Connections)
                {
                    if (nodeMap.TryGetValue(conn.SourceStepId, out var sourceNode) &&
                        nodeMap.TryGetValue(conn.TargetStepId, out var targetNode))
                    {
                        var connection = new WorkflowConnectionViewModel
                        {
                            Id = conn.Id,
                            Source = sourceNode,
                            Target = targetNode,
                            Condition = conn.Condition,
                            Priority = conn.Priority,
                            Label = conn.Label
                        };

                        Connections.Add(connection);
                    }
                }

                MessageBox.Show("✅ Tải workflow thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NewWorkflow()
        {
            var result = MessageBox.Show("Tạo workflow mới? Dữ liệu hiện tại sẽ bị xóa.",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Nodes.Clear();
                Connections.Clear();
                CurrentWorkflowId = null;
                WorkflowName = "Workflow mới";
            }
        }

        [RelayCommand]
        private async Task ValidateWorkflow()
        {
            if (!CurrentWorkflowId.HasValue)
            {
                MessageBox.Show("Vui lòng lưu workflow trước khi kiểm tra!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsLoading = true;

            try
            {
                var result = await _workflowService.ValidateAsync(CurrentWorkflowId.Value);

                if (result.IsValid)
                {
                    MessageBox.Show("✅ Workflow hợp lệ!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errors = string.Join("\n• ", result.Errors);
                    MessageBox.Show($"❌ Workflow chưa hợp lệ:\n\n• {errors}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AutoLayout()
        {
            if (!Nodes.Any()) return;

            // Thuật toán Layered Layout đơn giản
            // 1. Tìm start node
            var startNodes = Nodes.Where(n => n.NodeType == "start").ToList();
            if (!startNodes.Any())
            {
                MessageBox.Show("Không tìm thấy node Start!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var layers = new List<List<WorkflowNodeViewModel>>();
            var visited = new HashSet<Guid>();
            var nodeLayer = new Dictionary<Guid, int>();

            // BFS để tính layer
            var queue = new Queue<(WorkflowNodeViewModel node, int layer)>();
            foreach (var startNode in startNodes)
            {
                queue.Enqueue((startNode, 0));
                visited.Add(startNode.Id);
            }

            while (queue.Count > 0)
            {
                var (currentNode, layer) = queue.Dequeue();
                nodeLayer[currentNode.Id] = layer;

                // Tìm các node tiếp theo
                var outgoingConnections = Connections.Where(c => c.Source == currentNode);
                foreach (var conn in outgoingConnections)
                {
                    if (!visited.Contains(conn.Target.Id))
                    {
                        visited.Add(conn.Target.Id);
                        queue.Enqueue((conn.Target, layer + 1));
                    }
                }
            }

            // Group nodes by layer
            var maxLayer = nodeLayer.Values.Any() ? nodeLayer.Values.Max() : 0;
            for (int i = 0; i <= maxLayer; i++)
            {
                layers.Add(new List<WorkflowNodeViewModel>());
            }

            foreach (var node in Nodes)
            {
                if (nodeLayer.TryGetValue(node.Id, out var layer))
                {
                    layers[layer].Add(node);
                }
                else
                {
                    // Node không kết nối, đặt ở layer cuối
                    layers[maxLayer].Add(node);
                }
            }

            // Arrange nodes
            double horizontalSpacing = 250;
            double verticalSpacing = 150;
            double startX = 100;
            double startY = 100;

            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                double layerY = startY + (layerIndex * verticalSpacing);

                for (int nodeIndex = 0; nodeIndex < layer.Count; nodeIndex++)
                {
                    var node = layer[nodeIndex];
                    double nodeX = startX + (nodeIndex * horizontalSpacing);
                    node.Location = new Point(nodeX, layerY);
                }
            }

            MessageBox.Show("✅ Đã sắp xếp lại layout!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}