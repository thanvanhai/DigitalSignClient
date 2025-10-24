using MahApps.Metro.Controls;
using DigitalSignClient.ViewModels;

namespace DigitalSignClient.Views
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void ToggleMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuControl.IsPaneOpen = !HamburgerMenuControl.IsPaneOpen;
        }

        private void HamburgerMenuControl_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Lấy item được click
            var menuItem = e.ClickedItem as HamburgerMenuIconItem;
            if (menuItem == null) return;

            // Thực thi command tương ứng
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null) return;

            switch (menuItem.Tag?.ToString())
            {
                case "Dashboard":
                    viewModel.ShowDashboardCommand?.Execute(null);
                    break;
                case "DocumentType":
                    viewModel.ShowDocumentTypeCommand?.Execute(null);
                    break;
                case "Workflow":
                    viewModel.ShowWorkflowCommand?.Execute(null);
                    break;
                case "DocumentList":
                    viewModel.ShowDocumentListCommand?.Execute(null);
                    break;
            }
        }
    }
}