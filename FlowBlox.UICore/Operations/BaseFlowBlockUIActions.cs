using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Views;
using SkiaSharp;
using Svg;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Operations
{
    public class BaseFlowBlockUIActions : ComponentUIActions<BaseFlowBlock>
    {
        private FlowBloxRegistry _registry;
        private readonly IRuntimeStateService _runtimeStateService;

        public BaseFlowBlockUIActions(BaseFlowBlock component) : base(component)
        {
            _registry = FlowBloxRegistryProvider.GetRegistry();
            _runtimeStateService = FlowBloxServiceLocator.Instance.GetService<IRuntimeStateService>();
        }

        public bool CanGenerate()
        {
            if (_runtimeStateService?.IsRuntimeActive == true)
                return false;

            if (!Component.TestDefinitions.Any()) 
                return false;

            if (!Component.GenerationStrategies.Any())
                return false;

            return true;
        }

        public SKImage GenerateIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.auto_fix, 16,SKColors.SeaGreen);

        [Display(Name = "BaseResultFlowBlockUIActions_Generate", ResourceType = typeof(FlowBloxTexts))]
        public void Generate()
        {
            if (!CanGenerate())
                return;

            var generationView = new GenerationView(_registry.Reload(Component));
            var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            dialogService.ShowWPFDialog(generationView);
        }
    }
}
