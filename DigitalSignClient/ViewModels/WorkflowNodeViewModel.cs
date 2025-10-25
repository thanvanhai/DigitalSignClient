using CommunityToolkit.Mvvm.ComponentModel;
using Org.BouncyCastle.Tls;
using System;
using System.Data;
using System.Windows;

namespace DigitalSignClient.ViewModels
{
    public partial class WorkflowNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid id;

        [ObservableProperty]
        private Point location;

        [ObservableProperty]
        private string displayName = "New Node";

        [ObservableProperty]
        private string role = "";

        [ObservableProperty]
        private string signatureType = "Chính";

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private string nodeType = "sign"; // "start", "sign", "approval", "parallel", "end"

        [ObservableProperty]
        private int level;

        // Nodify properties
        [ObservableProperty]
        private bool isSelected;

        public WorkflowNodeViewModel()
        {
            Id = Guid.NewGuid();
        }

        public WorkflowNodeViewModel(Guid id, string role, string signatureType, Point location, string nodeType)
        {
            Id = id;
            Role = role;
            SignatureType = signatureType;
            Location = location;
            NodeType = nodeType;
            DisplayName = $"{role} ({signatureType})";
        }
    }
}