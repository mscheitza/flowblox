using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Selection;
using FlowBlox.UICore.PopUp.Resources;
using FlowBlox.UICore.PopUp.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBlox.UICore.PopUp.Provider
{
    public class SequenceDetectionQuickStartPopUpProvider :
        QuickStartPopUpProviderBase<SequenceDetectionFlowBlock>,
        IOptionsRegistration
    {
        public const string SequenceDetectionOptionKey = "PopUp.SequenceDetection.ShowQuickStart";

        public override string OptionKey => SequenceDetectionOptionKey;

        protected override string WindowTitle => SequenceDetectionPopUpTexts.Window_Title;

        protected override IReadOnlyList<QuickStartPopUpItem> CreateItems(SequenceDetectionFlowBlock target)
        {
            return
            [
                new QuickStartPopUpItem(
                    SequenceDetectionPopUpTexts.Step1_Headline,
                    SequenceDetectionPopUpTexts.Step1_Description,
                    PopUpImageResourceHelper.GetImageSource(SequenceDetectionPopUpImages.ResourceManager, nameof(SequenceDetectionPopUpImages.SequenceDetectionQuickStart_1))),
                new QuickStartPopUpItem(
                    SequenceDetectionPopUpTexts.Step2_Headline,
                    SequenceDetectionPopUpTexts.Step2_Description,
                    PopUpImageResourceHelper.GetImageSource(SequenceDetectionPopUpImages.ResourceManager, nameof(SequenceDetectionPopUpImages.SequenceDetectionQuickStart_2))),
                new QuickStartPopUpItem(
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
                SequenceDetectionPopUpTexts.Option_ShowQuickStart_Description,
                OptionElement.OptionType.Boolean,
                SequenceDetectionPopUpTexts.Option_ShowQuickStart_DisplayName));
        }
    }

    public class QuickStartPopUpServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IQuickStartPopUpProvider, SequenceDetectionQuickStartPopUpProvider>();
            serviceCollection.AddSingleton<IOptionsRegistration>(sp =>
                (SequenceDetectionQuickStartPopUpProvider)sp.GetRequiredService<IQuickStartPopUpProvider>());
            serviceCollection.AddSingleton<IQuickStartPopUpService, QuickStartPopUpService>();
        }
    }
}
