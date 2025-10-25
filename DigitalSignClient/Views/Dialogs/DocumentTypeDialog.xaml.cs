using DigitalSignClient.Models;
using System.Windows;

namespace DigitalSignClient.Views.Dialogs
{
    public partial class DocumentTypeDialog : Window
    {
        public string DocumentTypeName => txtName.Text.Trim();
        public string DocumentTypeDescription => txtDescription.Text.Trim();
        public bool IsActive => chkIsActive.IsChecked ?? true;

        public DocumentTypeDialog()
        {
            InitializeComponent();
            Title = "Thêm loại tài liệu mới";
        }

        public DocumentTypeDialog(DocumentType documentType) : this()
        {
            Title = "Chỉnh sửa loại tài liệu";
            txtName.Text = documentType.Name;
            txtDescription.Text = documentType.Description;
            chkIsActive.IsChecked = documentType.IsActive;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DocumentTypeName))
            {
                MessageBox.Show("Vui lòng nhập tên loại tài liệu!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}