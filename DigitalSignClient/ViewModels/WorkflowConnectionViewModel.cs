using CommunityToolkit.Mvvm.ComponentModel;
using Org.BouncyCastle.Asn1.X509;
using System;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace DigitalSignClient.ViewModels
{
    public partial class WorkflowConnectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid id;

        [ObservableProperty]
        private WorkflowNodeViewModel source;

        [ObservableProperty]
        private WorkflowNodeViewModel target;

        [ObservableProperty]
        private string? condition = "auto";

        [ObservableProperty]
        private int priority = 0;

        [ObservableProperty]
        private string? label;

        public WorkflowConnectionViewModel()
        {
            Id = Guid.NewGuid();
        }

        public WorkflowConnectionViewModel(WorkflowNodeViewModel source, WorkflowNodeViewModel target)
        {
            Id = Guid.NewGuid();
            Source = source;
            Target = target;
        }
    }
}