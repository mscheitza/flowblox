using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.ObjectManager;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Attributes;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.Views;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Operations
{
    public class FlowBloxComponentUIActions : ComponentUIActions<FlowBloxComponent>
    {
        private readonly IDialogService _dialogService;
        private readonly IRuntimeStateService _runtimeStateService;
        private readonly IComponentPopupService _componentPopupService;

        public FlowBloxComponentUIActions(FlowBloxComponent component) : base(component)
        {
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            _runtimeStateService = FlowBloxServiceLocator.Instance.GetService<IRuntimeStateService>();
            _componentPopupService = FlowBloxServiceLocator.Instance.GetService<IComponentPopupService>();
        }

        public SKImage ManageUserFieldsIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.account_cog, 16, SKColors.SteelBlue);
        public SKImage QuickStartIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.play_box_outline, 16, SKColors.SteelBlue);

        public bool IsQuickStartVisible() =>
            _componentPopupService?.HasProviderFor(Component, ComponentPopupEvent.Open) == true;

        [Display(Name = "FlowBloxComponentUIActions_QuickStart", ResourceType = typeof(FlowBloxTexts))]
        public void QuickStart()
        {
            _componentPopupService?.ShowForForced(Component, ComponentPopupEvent.Open);
        }

        public bool CanManageUserFields()
        {
            // A nested transaction is the active detail-edit transaction of a parent view.
            // User-field management opens another registry-backed editor and must wait until it is closed.
            return !FlowBloxRegistryProvider.IsCurrentTransactionNested();
        }

        [UIActionMetadata(OnlyShowInPropertyWindow = true)]
        [Display(Name = "FlowBloxComponentUIActions_ManageUserFields", ResourceType = typeof(FlowBloxTexts))]
        public void ManageUserFields()
        {
            if (!CanManageUserFields())
                return;

            var registry = FlowBloxRegistryProvider.GetRegistry();
            var userFieldObjectManager = new UserFieldObjectManager(registry);

            var propertyWindow = new PropertyWindow(new PropertyWindowArgs(
                userFieldObjectManager,
                readOnly: _runtimeStateService?.IsRuntimeActive == true ||
                    _runtimeStateService?.IsExternalProjectEditActive == true,
                deepCopy: false,
                canSave: false))
            {
                Height = 800
            };

            _dialogService.ShowWPFDialog(propertyWindow);
        }
    }
}
