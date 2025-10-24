using DigitalSignClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DigitalSignClient.Views
{
    public partial class DocumentListView : UserControl
    {
        private readonly DocumentListViewModel _viewModel;

        // Constructor nhận ViewModel qua DI
        public DocumentListView(DocumentListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += DocumentListView_Loaded;
        }

        // ✅ Constructor cho Designer hoặc nếu mở view trực tiếp (Design-time)
        public DocumentListView()
        {
        }

        private async void DocumentListView_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDocumentsAsync();
        }
    }
}
