using FlowBlox.Core.Constants;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.AI;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.PopUp.Constants;
using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.PopUp.Resources;
using FlowBlox.UICore.PopUp.Utilities;
using System.Globalization;
using System.IO;

namespace FlowBlox.UICore.PopUp.Implementations
{
    public class OnnxGenAIPopUpProvider :
        ComponentPopupProviderBase<OnnxGenAIFlowBlock>,
        IOptionsRegistration
    {
        public override string OptionKey => PopupOptionNames.ShowOnnxGenAIPopupOnOpen;

        protected override string WindowTitle => Text("Window_Title");

        protected override IReadOnlyList<ComponentPopupItem> CreateItems(OnnxGenAIFlowBlock target)
        {
            var scriptPageUri = GetScriptPageUri();

            return
            [
                new ComponentPopupItem(
                    Text("Step1_Headline"),
                    Text("Step1_Description"),
                    PopUpImageResourceHelper.GetImageSource(
                        OnnxGenAIPopUpImages.ResourceManager,
                        nameof(OnnxGenAIPopUpImages.OnnxGenAIQuickStart_1))),
                new ComponentPopupItem(
                    Text("Step2_Headline"),
                    string.Format(CultureInfo.CurrentCulture, Text("Step2_Description"), scriptPageUri),
                    PopUpImageResourceHelper.GetImageSource(
                        OnnxGenAIPopUpImages.ResourceManager,
                        nameof(OnnxGenAIPopUpImages.OnnxGenAIQuickStart_2))),
                new ComponentPopupItem(
                    Text("Step3_Headline"),
                    Text("Step3_Description"),
                    PopUpImageResourceHelper.GetImageSource(
                        OnnxGenAIPopUpImages.ResourceManager,
                        nameof(OnnxGenAIPopUpImages.OnnxGenAIQuickStart_3)))
            ];
        }

        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions)
        {
            SetOptionWasMissingAtInitialization(
                !currentOptions.Any(x => x.Name == PopupOptionNames.ShowOnnxGenAIPopupOnOpen));

            defaults.Add(new OptionElement(
                PopupOptionNames.ShowOnnxGenAIPopupOnOpen,
                bool.FalseString,
                "Controls whether the ONNX GenAI component pop-up dialog is shown.",
                OptionElement.OptionType.Boolean,
                "ONNX GenAI: Show Component Pop-up"));
        }

        private static string Text(string key) =>
            FlowBloxResourceUtil.GetLocalizedString(key, typeof(OnnxGenAIPopUpTexts));

        private static string GetScriptPageUri()
        {
            var path = Path.Combine(
                GlobalPaths.CurrentDirectory,
                "data",
                "html",
                "download_phi4_onnx.html");

            return new Uri(path).AbsoluteUri;
        }
    }
}
