using DigitalSignClient.ViewModels;
using System.Windows.Controls;

namespace DigitalSignClient.Views
{
    /// <summary>
    /// Interaction logic for WorkflowDesignerView.xaml
    /// </summary>
    public partial class WorkflowDesignerView : UserControl
    {
        public WorkflowDesignerView(WorkflowDesignerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
