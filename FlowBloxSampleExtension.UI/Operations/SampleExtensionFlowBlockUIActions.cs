using FlowBlox.Core.DependencyInjection;
using FlowBlox.UICore.Interfaces;
using FlowBloxSampleExtension.Models.FlowBlocks.SampleCategory;
using FlowBloxSampleExtension.UI.Views;
using System.ComponentModel.DataAnnotations;

namespace FlowBloxSampleExtension.UI.Operations
{
    public class SampleExtensionFlowBlockUIActions : ComponentUIActions<SampleExtensionFlowBlock>
    {
        public SampleExtensionFlowBlockUIActions(SampleExtensionFlowBlock component) : base(component) { }

        public bool CanSampleAction()
        {
            return true;
        }

        [Display(Name = "SampleExtensionFlowBlockUIActions_SampleAction", ResourceType = typeof(SampleExtensionResources))]
        public void SampleAction()
        {
            var window = new SampleWindow();
            var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            dialogService.ShowWPFDialog(window);
        }
    }
}