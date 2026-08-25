using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.ObjectManager;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Attributes;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Views;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Operations
{
    public class FlowBloxComponentUIActions : ComponentUIActions<FlowBloxComponent>
    {
        private readonly IDialogService _dialogService;
        private readonly IRuntimeStateService _runtimeStateService;

        public FlowBloxComponentUIActions(FlowBloxComponent component) : base(component)
        {
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            _runtimeStateService = FlowBloxServiceLocator.Instance.GetService<IRuntimeStateService>();
        }

        public SKImage ManageUserFieldsIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.account_cog, 16, SKColors.SteelBlue);

        [UIActionMetadata(OnlyShowInPropertyWindow = true)]
        [Display(Name = "FlowBloxComponentUIActions_ManageUserFields", ResourceType = typeof(FlowBloxTexts))]
        public void ManageUserFields()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var userFieldObjectManager = new UserFieldObjectManager(registry);

            var propertyWindow = new PropertyWindow(new PropertyWindowArgs(
                userFieldObjectManager,
                readOnly: _runtimeStateService?.IsRuntimeActive == true,
                deepCopy: false,
                canSave: false))
            {
                Height = 800
            };

            _dialogService.ShowWPFDialog(propertyWindow);
        }
    }
}
