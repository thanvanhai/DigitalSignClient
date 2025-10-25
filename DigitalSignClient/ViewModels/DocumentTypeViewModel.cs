using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using DigitalSignClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace DigitalSignClient.ViewModels
{
    public partial class DocumentTypeViewModel : ObservableObject
    {
        private readonly IDocumentTypeService _documentTypeService;

        [ObservableProperty]
        private ObservableCollection<DocumentType> _documentTypes = new();

        [ObservableProperty]
        private DocumentType? _selectedDocumentType;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEmpty;

        public DocumentTypeViewModel(IDocumentTypeService documentTypeService)
        {
            _documentTypeService = documentTypeService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var types = await _documentTypeService.GetAllAsync();

                if (types != null)
                {
                    DocumentTypes = new ObservableCollection<DocumentType>(types);
                    IsEmpty = !DocumentTypes.Any();
                }
                else
                {
                    DocumentTypes.Clear();
                    IsEmpty = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task AddAsync()
        {
            var dialog = new DocumentTypeDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var dto = new DocumentType
                    {
                        Name = dialog.DocumentTypeName,
                        Description = dialog.DocumentTypeDescription,
                        IsActive = dialog.IsActive
                    };

                    var result = await _documentTypeService.CreateAsync(dto);

                    if (result != null)
                    {
                        MessageBox.Show("Thêm loại tài liệu thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Thêm loại tài liệu thất bại!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task EditAsync(DocumentType? documentType)
        {
            if (documentType == null) return;

            var dialog = new DocumentTypeDialog(documentType);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var dto = new DocumentType
                    {
                        Id = documentType.Id,
                        Name = dialog.DocumentTypeName,
                        Description = dialog.DocumentTypeDescription,
                        IsActive = dialog.IsActive
                    };

                    var result = await _documentTypeService.UpdateAsync(documentType.Id, dto);

                    if (result != null)
                    {
                        MessageBox.Show("Cập nhật loại tài liệu thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật loại tài liệu thất bại!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteAsync(DocumentType? documentType)
        {
            if (documentType == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa loại tài liệu '{documentType.Name}'?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _documentTypeService.DeleteAsync(documentType.Id);

                    if (success)
                    {
                        MessageBox.Show("Xóa loại tài liệu thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Xóa loại tài liệu thất bại!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}