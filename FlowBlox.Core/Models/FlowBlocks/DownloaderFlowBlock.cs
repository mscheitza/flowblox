using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.Downloader;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "DownloaderFlowBlock_DisplayName", Description = "DownloaderFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class DownloaderFlowBlock : BaseFlowBlock
    {
        public enum FilterTargets 
        { 
            FileObjectURL, 
            FileObjectMimeType, 
            FileObjectFileName 
        };

        public string ExportDirectory { get; set; }
        public bool AllowRedirectedFiles { get; set; }
        public bool AutoDownload { get; set; }

        public List<string> FilterExpressions { get; set; } 
        
        protected Dictionary<string, bool> registry = new Dictionary<string, bool>();

        public Dictionary<string, bool> GetCache()
        {
            return registry;
        }

        public override SKImage Icon16 => base.Icon16;
        public override SKImage Icon32 => base.Icon32;

        public DownloaderFlowBlock() : base()
        {
            this.FilterExpressions = new List<string>();
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.IO;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(AutoDownload));
            properties.Add(nameof(AllowRedirectedFiles));
            return properties;
        }

        private bool CheckFilterExpressions(BaseRuntime runtime, WFileInfo fileInfo)
        {
            foreach (string FilterExpression in FilterExpressions)
            {
                string[] astrFilterExpression = FilterExpression.Split('#');
                string Target = astrFilterExpression[0];
                string Expression = (astrFilterExpression.Length > 1) ? astrFilterExpression[1] : string.Empty;
                Regex regex = new Regex(Expression);
                if (!Expression.Equals(string.Empty))
                {
                    if (Target.Equals("FileObjectURL") && !regex.IsMatch(fileInfo.URLToFile))
                    {
                        runtime.Report("The URL \"" + fileInfo.URLToFile + "\" does not match filter expression.");
                        return false;
                    }

                    if (Target.Equals("FileObjectMimeType") && !regex.IsMatch(fileInfo.MIMEType))
                    {
                        runtime.Report("The Type \"" + fileInfo.MIMEType + "\" does not match filter expression.");
                        return false;
                    }

                    if (Target.Equals("FileObjectFileName") && !regex.IsMatch(fileInfo.FileName))
                    {
                        runtime.Report("The Name \"" + fileInfo.FileName + "\" does not match filter expression.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Ermittelt vom verknüpften Eingabeelement den ersten gefundenen Extraktor.
        /// </summary>
        /// <param name="inputElement">Geben Sie hier das Eingabeelement an.</param>
        /// <param name="layer">Geben Sie hier 0 an.</param>
        /// <param name="foundExtractor">Der gefundene Extraktor oder <c>null</c></param>
        private void FindExtractor(BaseFlowBlock inputElement, int layer, ref WebRequestFlowBlock foundExtractor)
        {
            if (inputElement is WebRequestFlowBlock)
            {
                foundExtractor = (WebRequestFlowBlock)inputElement;
            }
            else
            {
                foreach (BaseResultFlowBlock resultElement in inputElement.ReferencedFlowBlocks)
                {
                    FindExtractor(resultElement, (layer + 1), ref foundExtractor);

                    if (foundExtractor != null)
                        break;
                }
            }
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var referencedGridElement = this.ReferencedFlowBlocks.Single();
                WebRequestFlowBlock referencedExtractor = null;
                if (referencedGridElement.GetInputCardinality() != FlowBlockCardinalities.None)
                {
                    FindExtractor(referencedGridElement, 0, ref referencedExtractor);
                }
                if (referencedExtractor != null)
                {
                    if (referencedExtractor.Fields.Count > 0)
                    {
                        FieldElement FieldElement = ((BaseResultFlowBlock)referencedGridElement).Fields[0];

                        string HTTP_URL = referencedExtractor.LiveURL;
                        string HTTP_Content = FieldElement.StringValue;
                        string HTTP_Username = referencedExtractor.UserName;
                        string HTTP_Password = referencedExtractor.Password;

                        // HTTPFileExtractor HTTPFileExtractor = new HTTPFileExtractor
                        //     (
                        //         runtime,
                        //         HTTP_URL,
                        //         HTTP_Content,
                        //         HTTP_Username,
                        //         HTTP_Password,
                        //         AllowRedirectedFiles,
                        //         referencedExtractor.CookieContainer,
                        //         registry
                        //     );
                        // 
                        // HTTPFileExtractor.FileFound += new HTTPFileExtractor.FileFoundEventHandler(HTTPFileExtractor_FileFound);
                        // 
                        // List<WFileInfo> files = HTTPFileExtractor.GetFileObjects();
                        // if (files.Count == 0)
                        // {
                        //     runtime.Report("HTTPFileExtractor returned no files.", FlowBloxLogLevel.Warning);
                        //     Warn(runtime, "No file objects found.");
                        // }
                        // else
                        // {
                        //     runtime.Report("HTTPFileExtractor returned " + files.Count.ToString() + " files to validate.");
                        //     UndoWarn();
                        // }
                    }
                }
            });
        }

        void HTTPFileExtractor_FileFound(BaseRuntime Runtime, WFileInfo FileInfo)
        {
            if (CheckFilterExpressions(Runtime, FileInfo))
            {
                Runtime.Report("Add file object \"" + FileInfo.FileName + "\" to DownloadManager.");

                string ResolvedExportDirectory = ExportDirectory;

                foreach (FieldElement FieldElement in FlowBloxRegistryProvider.GetRegistry().GetAllFields())
                {
                    if (ResolvedExportDirectory.Contains(FieldElement.FullyQualifiedName))
                    {
                        ResolvedExportDirectory = ResolvedExportDirectory.Replace(FieldElement.FullyQualifiedName, FieldElement.StringValue);
                    }
                }

                if (!Directory.Exists(ResolvedExportDirectory))
                {
                    Directory.CreateDirectory(ResolvedExportDirectory);

                    Runtime.Report("Create ExportDirectory=" + ResolvedExportDirectory);
                }

            }
        }
    }
}
