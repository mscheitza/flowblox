using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Selection;
using FlowBlox.UICore.PopUp.Constants;
using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.PopUp.Resources;
using FlowBlox.UICore.PopUp.Utilities;

namespace FlowBlox.UICore.PopUp.Implementations
{
    public class SequenceDetectionPopUpProvider :
        ComponentPopupProviderBase<SequenceDetectionFlowBlock>,
        IOptionsRegistration
    {
        public override string OptionKey => PopupOptionNames.ShowSequenceDetectionPopupOnOpen;

        protected override string WindowTitle => SequenceDetectionPopUpTexts.Window_Title;

        protected override IReadOnlyList<ComponentPopupItem> CreateItems(SequenceDetectionFlowBlock target)
        {
            return
            [
                new ComponentPopupItem(
                    SequenceDetectionPopUpTexts.Step1_Headline,
                    SequenceDetectionPopUpTexts.Step1_Description,
                    PopUpImageResourceHelper.GetImageSource(SequenceDetectionPopUpImages.ResourceManager, nameof(SequenceDetectionPopUpImages.SequenceDetectionQuickStart_1))),
                new ComponentPopupItem(
                    SequenceDetectionPopUpTexts.Step2_Headline,
                    SequenceDetectionPopUpTexts.Step2_Description,
                    PopUpImageResourceHelper.GetImageSource(SequenceDetectionPopUpImages.ResourceManager, nameof(SequenceDetectionPopUpImages.SequenceDetectionQuickStart_2))),
                new ComponentPopupItem(
                    SequenceDetectionPopUpTexts.Step3_Headline,
                    SequenceDetectionPopUpTexts.Step3_Description,
                    PopUpImageResourceHelper.GetImageSource(SequenceDetectionPopUpImages.ResourceManager, nameof(SequenceDetectionPopUpImages.SequenceDetectionQuickStart_3)))
            ];
        }

        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions)
        {
            SetOptionWasMissingAtInitialization(
                !currentOptions.Any(x => x.Name == PopupOptionNames.ShowSequenceDetectionPopupOnOpen));

            defaults.Add(new OptionElement(
                PopupOptionNames.ShowSequenceDetectionPopupOnOpen,
                bool.FalseString,
                "Controls whether the Sequence Detection component pop-up dialog is shown.",
                OptionElement.OptionType.Boolean,
                "Sequence Detection: Show Component Pop-up"));
        }
    }
}
