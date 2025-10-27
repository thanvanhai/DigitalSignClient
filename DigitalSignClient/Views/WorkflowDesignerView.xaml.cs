using DigitalSignClient.Models;
using DigitalSignClient.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DigitalSignClient.Views
{
    public partial class WorkflowDesignerView : UserControl
    {
        private WorkflowDesignerViewModel? ViewModel => DataContext as WorkflowDesignerViewModel;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private WorkflowNodeViewModel? _draggingNode;

        public WorkflowDesignerView()
        {
            InitializeComponent();
        }

        public WorkflowDesignerView(WorkflowDesignerViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is WorkflowNodeViewModel node)
            {
                // Select node
                if (ViewModel != null)
                {
                    ViewModel.SelectedNode = node;
                }

                // Start dragging
                _isDragging = true;
                _draggingNode = node;
                _dragStartPoint = e.GetPosition(WorkflowCanvas);
                border.CaptureMouse();

                e.Handled = true;
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggingNode != null && sender is Border border)
            {
                Point currentPoint = e.GetPosition(WorkflowCanvas);

                double deltaX = currentPoint.X - _dragStartPoint.X;
                double deltaY = currentPoint.Y - _dragStartPoint.Y;

                double newX = _draggingNode.X + deltaX;
                double newY = _draggingNode.Y + deltaY;

                // Giới hạn trong canvas
                newX = Math.Max(0, Math.Min(newX, WorkflowCanvas.Width - 120));
                newY = Math.Max(0, Math.Min(newY, WorkflowCanvas.Height - 80));

                if (ViewModel != null)
                {
                    ViewModel.UpdateNodePosition(_draggingNode.Id, newX, newY);
                }

                _dragStartPoint = currentPoint;
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && sender is Border border)
            {
                _isDragging = false;
                _draggingNode = null;
                border.ReleaseMouseCapture();

                e.Handled = true;
            }
        }
    }
}