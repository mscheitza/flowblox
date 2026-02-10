using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.IO
{
    [Display(Name = "FileReaderFlowBlock_DisplayName", Description = "FileReaderFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class FileReaderFlowBlock : BaseSingleResultFlowBlock
    {
        [Required]
        [Display(Name = "PropertyNames_DataSource", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association,
                     SelectionFilterMethod = nameof(GetPossibleDataSources),
                     SelectionDisplayMember = nameof(DataObjectBase.Name))]
        public DataObjectBase DataSource { get; set; }

        [ActivationCondition(MemberName = nameof(ResultField), ActivationMethod = nameof(IsEncodingNameActive))]
        [ConditionallyRequired()]
        [Display(Name = "PropertyNames_EncodingName", Description = "PropertyNames_EncodingName_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public DotNetEncodingNames EncodingName { get; set; }

        private bool IsEncodingNameActive()
        {
            if (this.ResultField?.FieldType?.FieldType == FieldTypes.ByteArray)
                return false;

            return true;
        }

        private IEnumerable<DataObjectBase> GetPossibleDataSources()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetManagedObjects<DataObjectBase>();
        }

        public FileReaderFlowBlock()
        {
            this.EncodingName = DotNetEncodingNames.Default;
        }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_import, 16);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_import, 32);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.IO;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(DataSource));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                try
                {
                    if (DataSource == null || !DataSource.CanRead())
                        throw new InvalidOperationException("The data source is not ready for reading.");

                    byte[] content = DataSource.Content ?? Array.Empty<byte>();

                    string fileContent;
                    if (ResultField?.GetConfiguredType() == typeof(byte[]))
                    {
                        // BytyeArray
                        fileContent = Convert.ToBase64String(content);
                    }
                    else
                    {
                        // String
                        var encoding = EncodingName.ToEncoding();
                        fileContent = encoding.GetString(content);
                    }

                    GenerateResult(runtime, fileContent);
                }
                catch (Exception e)
                {
                    runtime.Report(e.ToString());
                    CreateNotification(runtime, FileReaderNotifications.FailedToReadFile);
                    GenerateResult(runtime);
                }
            });
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(FileReaderNotifications));
                return notificationTypes;
            }
        }

        public enum FileReaderNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "Failed to read the file")]
            FailedToReadFile
        }
    }
}
