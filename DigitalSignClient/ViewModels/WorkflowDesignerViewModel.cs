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
        private string workflowName = "Workflow mới";

        [ObservableProperty]
        private Guid? currentWorkflowId;

        [ObservableProperty]
        private Guid selectedDocumentTypeId;

        [ObservableProperty]
        private bool isLoading;

        public WorkflowDesignerViewModel(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        // ==============================
        // THÊM NODE
        // ==============================
        [RelayCommand]
        private void AddNode(string nodeType)
        {
            var newNode = new WorkflowNodeViewModel
            {
                Id = Guid.NewGuid(),
                NodeType = nodeType,
                Location = new Point(100 + Nodes.Count * 80, 100 + Nodes.Count * 50),
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

        // ==============================
        // XÓA NODE
        // ==============================
        [RelayCommand]
        private void RemoveNode(WorkflowNodeViewModel node)
        {
            if (node == null) return;
            Nodes.Remove(node);

            // Cập nhật lại Level theo thứ tự
            int i = 1;
            foreach (var n in Nodes.OrderBy(n => n.Level))
                n.Level = i++;
        }

        // ==============================
        // LƯU WORKFLOW
        // ==============================
        [RelayCommand]
        private async Task SaveWorkflow()
        {
            if (string.IsNullOrWhiteSpace(WorkflowName))
            {
                MessageBox.Show("Vui lòng nhập tên workflow!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDocumentTypeId == Guid.Empty)
            {
                MessageBox.Show("Vui lòng chọn loại văn bản!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Nodes.Any())
            {
                MessageBox.Show("Workflow phải có ít nhất 1 bước!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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

        // ==============================
        // TẢI WORKFLOW
        // ==============================
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

                Nodes.Clear();

                CurrentWorkflowId = workflow.Id;
                WorkflowName = workflow.Name;
                SelectedDocumentTypeId = workflow.DocumentTypeId;

                foreach (var step in workflow.Steps.OrderBy(s => s.Level))
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

        // ==============================
        // KIỂM TRA HỢP LỆ
        // ==============================
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

        // ==============================
        // TẠO WORKFLOW MỚI
        // ==============================
        [RelayCommand]
        private void NewWorkflow()
        {
            var result = MessageBox.Show("Tạo workflow mới? Dữ liệu hiện tại sẽ bị xóa.",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Nodes.Clear();
                CurrentWorkflowId = null;
                WorkflowName = "Workflow mới";
            }
        }

        // ==============================
        // SẮP XẾP NODE TỰ ĐỘNG THEO LEVEL
        // ==============================
        [RelayCommand]
        private void AutoLayout()
        {
            if (!Nodes.Any()) return;

            double startX = 100;
            double startY = 100;
            double verticalSpacing = 150;

            foreach (var node in Nodes.OrderBy(n => n.Level))
            {
                node.Location = new Point(startX, startY + (node.Level - 1) * verticalSpacing);
            }

            MessageBox.Show("✅ Đã sắp xếp lại layout theo thứ tự level!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
