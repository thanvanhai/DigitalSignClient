using DigitalSignClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DigitalSignClient.Views
{
    public partial class DocumentListView : UserControl
    {
        private readonly DocumentListViewModel _viewModel;

        public DocumentListView(DocumentListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += DocumentListView_Loaded;
        }

        // Constructor không tham số cho designer
        public DocumentListView() : this(new DocumentListViewModel(new Services.ApiService()))
        {
        }

        private async void DocumentListView_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDocumentsAsync();
        }
    }
}