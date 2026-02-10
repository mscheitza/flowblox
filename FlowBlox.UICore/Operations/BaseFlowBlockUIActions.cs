using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Views;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Operations
{
    public class BaseFlowBlockUIActions : ComponentUIActions<BaseFlowBlock>
    {
        private FlowBloxRegistry _registry;

        public BaseFlowBlockUIActions(BaseFlowBlock component) : base(component)
        {
            _registry = FlowBloxRegistryProvider.GetRegistry();
        }

        public bool CanGenerate()
        {
            if (!Component.TestDefinitions.Any()) 
                return false;

            if (!Component.GenerationStrategies.Any())
                return false;

            return true;
        }

        [Display(Name = "BaseResultFlowBlockUIActions_Generate", ResourceType = typeof(FlowBloxTexts))]
        public void Generate()
        {
            var generationView = new GenerationView(_registry.Reload(Component));
            var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            dialogService.ShowWPFDialog(generationView);
        }
    }
}