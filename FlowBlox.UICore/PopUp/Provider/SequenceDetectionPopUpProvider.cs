using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Selection;
using FlowBlox.UICore.PopUp.Resources;
using FlowBlox.UICore.PopUp.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBlox.UICore.PopUp.Provider
{
    public class SequenceDetectionPopUpProvider :
        ComponentPopupProviderBase<SequenceDetectionFlowBlock>,
        IOptionsRegistration
    {
        public const string SequenceDetectionOptionKey = "PopUp.SequenceDetection.ShowComponentPopup";

        public override string OptionKey => SequenceDetectionOptionKey;

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
            SetOptionWasMissingAtInitialization(!currentOptions.Any(x => x.Name == SequenceDetectionOptionKey));

            defaults.Add(new OptionElement(
                SequenceDetectionOptionKey,
                bool.FalseString,
                "Controls whether the Sequence Detection component pop-up dialog is shown.",
                OptionElement.OptionType.Boolean,
                "Sequence Detection: Show Component Pop-up"));
        }
    }

    public class ComponentPopupServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IComponentPopupProvider, SequenceDetectionPopUpProvider>();
            serviceCollection.AddSingleton<IOptionsRegistration>(sp =>
                (SequenceDetectionPopUpProvider)sp.GetRequiredService<IComponentPopupProvider>());
            serviceCollection.AddSingleton<IComponentPopupService, ComponentPopupService>();
        }
    }
}
