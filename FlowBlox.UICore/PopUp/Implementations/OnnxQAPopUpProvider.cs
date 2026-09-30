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
    public class OnnxQAPopUpProvider :
        ComponentPopupProviderBase<OnnxQAFlowBlock>,
        IOptionsRegistration
    {
        public override string OptionKey => PopupOptionNames.ShowOnnxQAPopupOnOpen;

        protected override string WindowTitle => Text("Window_Title");

        protected override IReadOnlyList<ComponentPopupItem> CreateItems(OnnxQAFlowBlock target)
        {
            var scriptPageUri = GetScriptPageUri();

            return
            [
                new ComponentPopupItem(
                    Text("Step1_Headline"),
                    Text("Step1_Description"),
                    PopUpImageResourceHelper.GetImageSource(
                        OnnxQAPopUpImages.ResourceManager,
                        nameof(OnnxQAPopUpImages.OnnxQAQuickStart_1))),
                new ComponentPopupItem(
                    Text("Step2_Headline"),
                    string.Format(CultureInfo.CurrentCulture, Text("Step2_Description"), scriptPageUri),
                    PopUpImageResourceHelper.GetImageSource(
                        OnnxQAPopUpImages.ResourceManager,
                        nameof(OnnxQAPopUpImages.OnnxQAQuickStart_2))),
                new ComponentPopupItem(
                    Text("Step3_Headline"),
                    Text("Step3_Description"),
                    PopUpImageResourceHelper.GetImageSource(
                        OnnxQAPopUpImages.ResourceManager,
                        nameof(OnnxQAPopUpImages.OnnxQAQuickStart_3)))
            ];
        }

        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions)
        {
            SetOptionWasMissingAtInitialization(
                !currentOptions.Any(x => x.Name == PopupOptionNames.ShowOnnxQAPopupOnOpen));

            defaults.Add(new OptionElement(
                PopupOptionNames.ShowOnnxQAPopupOnOpen,
                bool.FalseString,
                "Controls whether the ONNX QA component pop-up dialog is shown.",
                OptionElement.OptionType.Boolean,
                "ONNX QA: Show Component Pop-up"));
        }

        private static string Text(string key) =>
            FlowBloxResourceUtil.GetLocalizedString(key, typeof(OnnxQAPopUpTexts));

        private static string GetScriptPageUri()
        {
            var path = Path.Combine(
                GlobalPaths.CurrentDirectory,
                "data",
                "html",
                "export_onnx_qa_model.html");

            return new Uri(path).AbsoluteUri;
        }
    }
}
