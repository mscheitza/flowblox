using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Web;
using FlowBlox.UICore.PopUp.Constants;
using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.PopUp.Resources;
using FlowBlox.UICore.PopUp.Utilities;

namespace FlowBlox.UICore.PopUp.Implementations
{
    public class WebBrowserPopUpProvider :
        ComponentPopupProviderBase<WebBrowserFlowBlock>,
        IOptionsRegistration
    {
        public override string OptionKey => PopupOptionNames.ShowWebBrowserPopupOnOpen;

        protected override string WindowTitle => WebBrowserPopUpTexts.Window_Title;

        protected override IReadOnlyList<ComponentPopupItem> CreateItems(WebBrowserFlowBlock target)
        {
            return
            [
                new ComponentPopupItem(
                    WebBrowserPopUpTexts.Step1_Headline,
                    WebBrowserPopUpTexts.Step1_Description,
                    PopUpImageResourceHelper.GetImageSource(WebBrowserPopUpImages.ResourceManager, nameof(WebBrowserPopUpImages.WebBrowserQuickStart_1))),
                new ComponentPopupItem(
                    WebBrowserPopUpTexts.Step2_Headline,
                    WebBrowserPopUpTexts.Step2_Description,
                    PopUpImageResourceHelper.GetImageSource(WebBrowserPopUpImages.ResourceManager, nameof(WebBrowserPopUpImages.WebBrowserQuickStart_2)))
            ];
        }

        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions)
        {
            SetOptionWasMissingAtInitialization(
                !currentOptions.Any(x => x.Name == PopupOptionNames.ShowWebBrowserPopupOnOpen));

            defaults.Add(new OptionElement(
                PopupOptionNames.ShowWebBrowserPopupOnOpen,
                bool.FalseString,
                "Controls whether the Web Browser component pop-up dialog is shown.",
                OptionElement.OptionType.Boolean,
                "Web Browser: Show Component Pop-up"));
        }
    }
}
