using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.SequenceFlow
{
    [Display(Name = "ProjectOutputMappingEntry_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class ProjectOutputMappingEntry : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "ProjectOutputMappingEntry_OutputPropertyName", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        public string OutputPropertyName { get; set; }

        [Required]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public FieldElement Field { get; set; }

        public bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            if (Field == null)
            {
                dependencies = null;
                return true;
            }

            return Field.IsDeletable(out dependencies);
        }
    }

    [FlowBlockUIGroup("ProjectOutputFlowBlock_Groups_Mapping", 0)]
    [Display(Name = "ProjectOutputFlowBlock_DisplayName", Description = "ProjectOutputFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class ProjectOutputFlowBlock : BaseFlowBlock
    {
        [Display(Name = "ProjectOutputFlowBlock_MappingEntries", ResourceType = typeof(FlowBloxTexts), GroupName = "ProjectOutputFlowBlock_Groups_Mapping", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBlockDataGrid]
        public ObservableCollection<ProjectOutputMappingEntry> MappingEntries { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.tray_arrow_up, 16, SKColors.MediumPurple);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.tray_arrow_up, 32, SKColors.MediumPurple);

        public ProjectOutputFlowBlock()
        {
            MappingEntries = new ObservableCollection<ProjectOutputMappingEntry>();
        }

        // Typically used as a sink; allowing multiple incoming connections is practical.
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.IO;

        public override string NamePrefix => "Output";

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                // Collect values only (do not store FieldElements).
                var values = new Dictionary<string, object>();
                foreach (var entry in MappingEntries)
                {
                    if (entry == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(entry.OutputPropertyName))
                        throw new ValidationException("OutputPropertyName must not be empty.");

                    if (entry.Field == null)
                        throw new ValidationException($"Field must be set for output property \"{entry.OutputPropertyName}\".");

                    // Store the plain value only
                    values[entry.OutputPropertyName] = entry.Field.Value;
                }

                var dataset = new Runtime.FlowBloxProjectOutputDataset
                {
                    OutputName = this.Name,
                    CreatedUtc = DateTime.UtcNow,
                    Values = values
                };

                runtime.AppendOutputDataset(this.Name, dataset);

                ExecuteNextFlowBlocks(runtime);
            });
        }
    }
}
