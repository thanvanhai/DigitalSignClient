using DigitalSignClient.Helpers;
using DigitalSignClient.Models;
using DigitalSignClient.Services.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DigitalSignClient.ViewModels
{
    public class DocumentListViewModel : BaseViewModel
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentTypeService _documentTypeService;
        private User? _currentUser;
        private Document? _selectedDocument;
        private bool _isLoading;

        public DocumentListViewModel(IDocumentService documentService, IDocumentTypeService documentTypeService)
        {
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            Documents = new ObservableCollection<Document>();
            DocumentTypes = new ObservableCollection<DocumentType>();

            UploadCommand = new DelegateCommand<object>(async _ => await UploadDocumentAsync());
            RefreshCommand = new DelegateCommand<object>(async _ => await LoadDocumentsAsync());
            SignCommand = new DelegateCommand<object>(async _ => await SignDocumentAsync(), _ => SelectedDocument != null);
            DownloadCommand = new DelegateCommand<object>(async _ => await DownloadDocumentAsync(), _ => SelectedDocument != null);
            DeleteCommand = new DelegateCommand<object>(async _ => await DeleteDocumentAsync(), _ => SelectedDocument != null);
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<Document> Documents { get; }
        public ObservableCollection<DocumentType> DocumentTypes { get; }

        public Document? SelectedDocument
        {
            get => _selectedDocument;
            set => SetProperty(ref _selectedDocument, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand UploadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SignCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand DeleteCommand { get; }

        // 📄 Load danh sách tài liệu
        public async Task LoadDocumentsAsync()
        {
            try
            {
                IsLoading = true;
                var documents = await _documentService.GetDocumentsAsync();

                Documents.Clear();
                if (documents != null)
                {
                    foreach (var doc in documents)
                        Documents.Add(doc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 📋 Load danh sách loại tài liệu
        public async Task LoadDocumentTypesAsync()
        {
            try
            {
                // Kiểm tra service có null không
                if (_documentTypeService == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ DocumentTypeService is NULL!");
                    MessageBox.Show("Lỗi: DocumentTypeService chưa được khởi tạo!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var documentTypes = await _documentTypeService.GetAllAsync();

                DocumentTypes.Clear();
                if (documentTypes != null && documentTypes.Count > 0)
                {
                    foreach (var type in documentTypes)
                    {
                        if (type != null)
                            DocumentTypes.Add(type);
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ Loaded {DocumentTypes.Count} document types");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Không có loại tài liệu nào");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải loại tài liệu: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ⬆ Upload tài liệu mới
        private async Task UploadDocumentAsync()
        {
            try
            {
                // Kiểm tra danh sách loại tài liệu
                if (DocumentTypes.Count == 0)
                {
                    MessageBox.Show("Chưa có loại tài liệu nào. Vui lòng thêm loại tài liệu trước!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Hiển thị dialog upload
                var uploadDialog = new Views.Dialogs.UploadDocumentDialog(DocumentTypes)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                // Tìm MainWindow (có thể là MetroWindow hoặc Window thường)
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.IsActive)
                    {
                        try
                        {
                            uploadDialog.Owner = window;
                            break;
                        }
                        catch
                        {
                            // Nếu không set được Owner thì thôi, vẫn mở được
                        }
                    }
                }

                if (uploadDialog.ShowDialog() != true)
                    return;

                // Upload lên server
                IsLoading = true;
                var selectedType = uploadDialog.SelectedDocumentType;
                var description = uploadDialog.Description;
                var filePath = uploadDialog.FilePath;

                var document = await _documentService.UploadDocumentAsync(
                    filePath,
                    description,
                    selectedType.Id);

                if (document != null)
                {
                    Documents.Add(document);
                    MessageBox.Show("Upload thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Upload thất bại!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ✍ Ký tài liệu
        private async Task SignDocumentAsync()
        {
            if (SelectedDocument == null) return;

            try
            {
                IsLoading = true;

                var tempPath = Path.Combine(Path.GetTempPath(), SelectedDocument.OriginalFileName);
                var fileData = await _documentService.DownloadDocumentAsync(SelectedDocument.Id);

                if (fileData == null)
                {
                    MessageBox.Show("Không thể tải file để ký.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await File.WriteAllBytesAsync(tempPath, fileData);

                var signWindow = new Views.SignDocumentWindow(tempPath, SelectedDocument.Id)
                {
                    Owner = Application.Current.MainWindow
                };
                signWindow.ShowDialog();

                await LoadDocumentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ⬇ Tải file PDF
        private async Task DownloadDocumentAsync()
        {
            if (SelectedDocument == null) return;

            try
            {
                var dialog = new SaveFileDialog
                {
                    FileName = SelectedDocument.OriginalFileName,
                    Filter = "PDF Files (*.pdf)|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    var fileData = await _documentService.DownloadDocumentAsync(SelectedDocument.Id);

                    if (fileData != null)
                    {
                        await File.WriteAllBytesAsync(dialog.FileName, fileData);
                        MessageBox.Show("Download thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Download thất bại!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 🗑 Xóa tài liệu
        private async Task DeleteDocumentAsync()
        {
            if (SelectedDocument == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa '{SelectedDocument.OriginalFileName}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    var success = await _documentService.DeleteDocumentAsync(SelectedDocument.Id);

                    if (success)
                    {
                        Documents.Remove(SelectedDocument);
                        MessageBox.Show("Xóa thành công!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}