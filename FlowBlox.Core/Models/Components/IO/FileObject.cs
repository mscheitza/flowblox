using FlowBlox.Core.Attributes;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Components.IO
{
    [Display(Name = "FileObject_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class FileObject : DataObjectBase
    {
        [Required()]
        [Display(Name = "PropertyNames_FilePath", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFieldSelection)]
        [FlowBlockUIFileSelection("All files (*.*)|*.*")]
        public string FilePath { get; set; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public override byte[] Content
        {
            get
            {
                var filePath = GetRuntimeFilePath();
                return File.Exists(filePath) ? 
                    File.ReadAllBytes(filePath) : 
                    null;
            }
            set
            {
                var filePath = GetRuntimeFilePath();
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(filePath, value);
                    TriggerDataSourceChanged();
                }
            }
        }

        public string GetRuntimeFilePath() => ReplaceRuntimeVariables(FilePath);

        private string ReplaceRuntimeVariables(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && 
                filePath.Contains("${Paths.OutputDirectory}"))
            {
                string outputDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["Paths.OutputDir"].Value;
                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);
                filePath = filePath.Replace("${Paths.OutputDirectory}", outputDirectory);
            }

            return FlowBloxFieldHelper.ReplaceFieldsInString(filePath);
        }

        public override bool CanRead()
        {
            return File.Exists(GetRuntimeFilePath());
        }

        public override void RegisterPropertyChangedEventHandlers()
        {
            foreach (var fieldElement in FlowBloxFieldHelper.GetFieldElementsFromString(FilePath))
            {
                fieldElement.OnNameChanged += FieldElement_NameChange;
                fieldElement.OnValueChanged += FieldElement_ValueChange;
            }
        }

        private void FieldElement_ValueChange(FieldElement field, string oldValue, string newValue)
        {
            TriggerDataSourceChanged();
        }

        private void FieldElement_NameChange(FieldElement field, string oldName, string newName)
        {
            FilePath = FilePath.Replace(oldName, newName);
        }

        public override string ToString()
        {
            return $"From file: \"{FilePath}\"";
        }
    }
}

