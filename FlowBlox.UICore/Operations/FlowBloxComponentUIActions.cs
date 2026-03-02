using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.ObjectManager;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Views;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Operations
{
    public class FlowBloxComponentUIActions : ComponentUIActions<FlowBloxComponent>
    {
        private readonly IDialogService _dialogService;

        public FlowBloxComponentUIActions(FlowBloxComponent component) : base(component)
        {
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
        }

        public SKImage ManageUserFieldsIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.account_cog, 16, SKColors.SteelBlue);

        [Display(Name = "FlowBloxComponentUIActions_ManageUserFields", ResourceType = typeof(FlowBloxTexts))]
        public void ManageUserFields()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var userFieldObjectManager = new UserFieldObjectManager(registry);

            var propertyWindow = new PropertyWindow(new PropertyWindowArgs(
                userFieldObjectManager,
                deepCopy: false,
                canSave: false))
            {
                Height = 800
            };

            _dialogService.ShowWPFDialog(propertyWindow);
        }
    }
}
