using DigitalSignClient.Models;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml;

namespace DigitalSignClient.Views.Dialogs
{
    public partial class UploadDocumentDialog : MetroWindow
    {
        public ObservableCollection<DocumentType> DocumentTypes { get; set; }
        public DocumentType? SelectedDocumentType { get; set; }
        public string? Description { get; set; }
        public string FilePath { get; private set; } = string.Empty;

        public UploadDocumentDialog(ObservableCollection<DocumentType> documentTypes)
        {
            InitializeComponent();
            DocumentTypes = documentTypes;
            DataContext = this;
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Chọn file PDF để upload"
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                txtFilePath.Text = Path.GetFileName(FilePath);
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            // Validate file
            if (string.IsNullOrEmpty(FilePath))
            {
                MessageBox.Show("Vui lòng chọn file PDF!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate document type
            if (cmbDocumentType.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại tài liệu!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedDocumentType = cmbDocumentType.SelectedItem as DocumentType;
            Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}