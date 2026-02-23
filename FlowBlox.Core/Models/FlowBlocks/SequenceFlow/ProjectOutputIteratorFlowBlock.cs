using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow.ExecuteProject;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [FlowBlockUIGroup("ProjectOutputIteratorFlowBlock_Groups_Source", 0)]
    [FlowBlockUIGroup("ProjectOutputIteratorFlowBlock_Groups_Mapping", 1)]
    [Display(Name = "ProjectOutputIteratorFlowBlock_DisplayName", Description = "ProjectOutputIteratorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class ProjectOutputIteratorFlowBlock : BaseResultFlowBlock
    {
        private FieldElement _inputField;

        [Display(Name = "Global_InputField", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association, SelectionFilterMethod = nameof(GetPossibleFieldElements), SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), Operations = UIOperations.Link | UIOperations.Unlink)]
        [Required()]
        public FieldElement InputField
        {
            get => _inputField;
            set => SetRequiredInputField(ref _inputField, value);
        }

        public override List<FieldElement> GetPossibleFieldElements() => FlowBlockHelper.GetFieldElementsOfAccoiatedFlowBlocks(this);

        [Required]
        [Display(Name = "ProjectOutputIteratorFlowBlock_OutputName", ResourceType = typeof(FlowBloxTexts), GroupName = "ProjectOutputIteratorFlowBlock_Groups_Source", Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public string OutputName { get; set; }

        [Display(Name = "ProjectOutputIteratorFlowBlock_OutputMappings", ResourceType = typeof(FlowBloxTexts), GroupName = "ProjectOutputIteratorFlowBlock_Groups_Mapping", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBlockDataGrid(IsMovable = true)]
        public ObservableCollection<ExecuteProjectOutputMappingEntry> OutputMappings { get; set; }

        public ProjectOutputIteratorFlowBlock()
        {
            OutputMappings = new ObservableCollection<ExecuteProjectOutputMappingEntry>();
        }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.playlist_play, 16, SKColors.MediumPurple);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.playlist_play, 32, SKColors.MediumPurple);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override List<FieldElement> Fields
        {
            get
            {
                return this.OutputMappings?
                    .Select(m => m.Field)
                    .ExceptNull()
                    .ToList();
            }
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                string responseJson = this.InputField.StringValue;
                if (string.IsNullOrWhiteSpace(responseJson))
                    throw new ValidationException("ResponseJson must not be empty.");

                if (string.IsNullOrWhiteSpace(OutputName))
                    throw new ValidationException("OutputName must not be empty.");

                var response = JsonSerializer.Deserialize<Runner.Contracts.RunnerResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response == null)
                    throw new InvalidOperationException("RunnerResponse could not be deserialized.");

                if (response.Outputs == null || 
                    !response.Outputs.TryGetValue(OutputName, out var datasets) || 
                    datasets == null)
                {
                    GenerateResult(runtime, new List<Dictionary<FieldElement, string>>());
                    return;
                }

                var resultMap = new List<Dictionary<FieldElement, string>>();

                foreach (var ds in datasets)
                {
                    var entry = new Dictionary<FieldElement, string>();

                    foreach (var mapping in OutputMappings)
                    {
                        if (mapping?.Field == null)
                            continue;

                        var key = mapping.OutputPropertyName;
                        if (string.IsNullOrWhiteSpace(key))
                            continue;

                        object val = null;
                        if (ds.Values != null)
                            ds.Values.TryGetValue(key, out val);

                        entry[mapping.Field] = val?.ToString() ?? "";
                    }

                    resultMap.Add(entry);
                }

                GenerateResult(runtime, resultMap);
                ExecuteNextFlowBlocks(runtime);
            });
        }
    }
}
