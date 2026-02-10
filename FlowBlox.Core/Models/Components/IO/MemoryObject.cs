using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.DeepCopier;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static FlowBlox.Core.Extensions.DotNetEncodingNamesExtension;

namespace FlowBlox.Core.Models.Components.IO
{
    [Display(Name = "MemoryObject_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class MemoryObject : DataObjectBase
    {
        public MemoryObject()
        {
            this.EncodingName = DotNetEncodingNames.Default;
        }

        private FieldElement _field;

        [Required()]
        [Display(Name = "Global_FieldElement", Description = "MemoryObject_Field_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink, 
            SelectionFilterMethod = nameof(GetPossibleFieldElements))]
        public FieldElement Field 
        {
            get => _field;
            set => SetRequiredInputField(ref _field, value);
        }

        [Required()]
        [Display(Name = "PropertyNames_FileName", Description = "MemoryObject_FileName_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public string FileName { get; set; }

        [ActivationCondition(MemberName = nameof(Field), ActivationMethod = nameof(IsEncodingNameActive))]
        [ConditionallyRequired()]
        [Display(Name = "PropertyNames_Encoding", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public DotNetEncodingNames EncodingName { get; set; }

        private bool IsEncodingNameActive()
        {
            if (this.Field?.FieldType?.FieldType == FieldTypes.ByteArray)
                return false;

            return true;
        }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public override byte[] Content
        {
            get
            {
                var fieldValue = Field?.Value;

                if (fieldValue is null)
                    return Array.Empty<byte>();

                if (fieldValue is byte[] byteArray)
                    return byteArray;

                if (fieldValue is string stringValue)
                    return EncodingName.ToEncoding().GetBytes(stringValue);

                throw new NotSupportedException($"Field value of type '{fieldValue?.GetType().FullName}' is not supported for Content conversion.");
            }
            set
            {
                if (Field == null)
                    return;

                var configuredType = Field.GetConfiguredType();
                if (configuredType == typeof(byte[]))
                {
                    Field.StringValue = value != null
                        ? Convert.ToBase64String(value)
                        : null;

                    TriggerDataSourceChanged();
                    return;
                }

                if (configuredType == typeof(string))
                {
                    Field.StringValue = value != null
                        ? EncodingName.ToEncoding().GetString(value)
                        : null;

                    TriggerDataSourceChanged();
                    return;
                }

                throw new NotSupportedException($"Field type '{configuredType.FullName}' is not supported for Content conversion.");
            }
        }

        public string TemporaryFilePath { get; private set; }

        private string _tempDirectory;

        public void CreateTemporaryFile()
        {
            if (string.IsNullOrEmpty(FileName))
                throw new InvalidOperationException("No file name could be determined from the memory object.");

            if (string.IsNullOrEmpty(_tempDirectory))
                _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (!Directory.Exists(_tempDirectory))
                Directory.CreateDirectory(_tempDirectory);

            this.TemporaryFilePath = Path.Combine(_tempDirectory, FileName);
            File.WriteAllBytes(this.TemporaryFilePath, Content);
        }

        public override bool CanRead()
        {
            if (this.Field == null)
                return false;

            if (this.Content == null)
                return false;

            if (string.IsNullOrWhiteSpace(FileName))
                return false;

            return true;
        }

        public bool CanReadTemporaryFile()
        {
            if (string.IsNullOrEmpty(TemporaryFilePath))
                return false;

            if (!File.Exists(TemporaryFilePath))
                return false;

            return true;
        }

        public override string ToString()
        {
            return $"From binary data of field: \"{Field?.Name}\"";
        }
    }
}
