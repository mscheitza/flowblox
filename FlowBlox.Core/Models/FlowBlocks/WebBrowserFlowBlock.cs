using CsvHelper;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.WebBrowser;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Mysqlx.Crud;
using SkiaSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [FlowBlockUIGroup("WebBrowserFlowBlock_Groups_Advanced", 0)]
    [Display(Name = "WebBrowserFlowBlock_DisplayName", Description = "WebBrowserFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class WebBrowserFlowBlock : BaseSingleResultFlowBlock
    {
        WebBrowserBase _internalWebBrowser;
        public WebBrowserBase InternalWebBrowser
        {
            get
            {
                if (_internalWebBrowser == null)
                {
                    var options = FlowBloxOptions.GetOptionInstance().OptionCollection;
                    var serviceName = options.ContainsKey("WebBrowser.ServiceName") ? options["WebBrowser.ServiceName"].Value : typeof(ChromeWebBrowser).Name;
                    _internalWebBrowser = WebBrowserProvider.Create(serviceName);
                }
                return _internalWebBrowser;
            }
        }

        [Display(Name = "WebBrowserFlowBlock_AssociatedWebBrowser", ResourceType = typeof(FlowBloxTexts), GroupName = "WebBrowserFlowBlock_Groups_Advanced", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleWebBrowsers), 
            SelectionDisplayMember = nameof(Name))]
        public WebBrowserFlowBlock AssociatedWebBrowser { get; set; }

        [Display(Name = "WebBrowserFlowBlock_Timeout", ResourceType = typeof(FlowBloxTexts), GroupName = "WebBrowserFlowBlock_Groups_Advanced", Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public int Timeout { get; set; }

        public List<WebBrowserFlowBlock> GetPossibleWebBrowsers()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFlowBlocks<WebBrowserFlowBlock>()
                .ToList();
        }

        [Display(Name = "WebBrowserFlowBlock_Url", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Url { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.web, 16, SKColors.DeepSkyBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.web, 32, SKColors.DeepSkyBlue);

        public WebBrowserFlowBlock() : base()
        {
            
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Web;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Url));
            return properties;
        }

        protected override void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            this.Url = FlowBloxFieldHelper.ReplaceFQName(this.Url, oldFQFieldName, newFQFieldName);
            base.OnReferencedFieldNameChanged(field, oldFQFieldName, newFQFieldName);
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                // Timeout setzen
                InternalWebBrowser.Timeout = Timeout;

                // URL ermitteln
                string url = FlowBloxFieldHelper.ReplaceFieldsInString(this.Url);
                
                // URL öffnen
                InternalWebBrowser.Open(url);

                // Im Falle eines assoziierten Web-Browsers werden die Cookies gesetzt und die Seite neu geladen
                if (this.AssociatedWebBrowser?.InternalWebBrowser != null)
                {
                    var associatedWebBrowser = this.AssociatedWebBrowser.InternalWebBrowser;
                    InternalWebBrowser.SetCookies(associatedWebBrowser.GetAllCookies());
                    InternalWebBrowser.Open(url);
                }

                string content = InternalWebBrowser.DOMContent;
                
                GenerateResult(runtime, content);
            });
        }

        public override void RuntimeFinished(BaseRuntime runtime)
        {
            if (_internalWebBrowser != null)
            {
                _internalWebBrowser.Close();
                _internalWebBrowser = null;
            }

            base.RuntimeFinished(runtime);
        }
    }
}
