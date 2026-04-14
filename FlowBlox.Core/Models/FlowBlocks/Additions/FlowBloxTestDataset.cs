using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    public class FlowBlockTestDataset : FlowBloxReactiveObject
    {
        private BaseFlowBlock _flowBlock;
        private bool _execute;
        private bool _uIRequiredForExecution;
        private List<FlowBloxFieldTestConfiguration> _flowBloxTestConfigurations;

        [JsonIgnore]
        public FlowBloxTestDefinition ParentTestDefinition { get; set; }

        [Display(Name = "FlowBlockTestDataset_FlowBlock", Description = "FlowBlockTestDataset_FlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
        public BaseFlowBlock FlowBlock
        {
            get => _flowBlock;
            set
            {
                if (_flowBlock != value)
                {
                    _flowBlock = value;
                    OnPropertyChanged();
                }
            }
        }

        [Display(Name = "FlowBlockTestDataset_Execute", Description = "FlowBlockTestDataset_Execute_Description", ResourceType = typeof(FlowBloxTexts))]
        public bool Execute
        {
            get => _execute;
            set
            {
                if (_execute != value)
                {
                    _execute = value;
                    ParentTestDefinition?.RecalculateRequiredFlagsAcrossDefinition();
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "FlowBlockTestDataset_UIRequiredForExecution", Description = "FlowBlockTestDataset_UIRequiredForExecution_Description", ResourceType = typeof(FlowBloxTexts))]
        public bool UIRequiredForExecution
        {
            get => _uIRequiredForExecution;
            set
            {
                if (_uIRequiredForExecution != value)
                {
                    _uIRequiredForExecution = value;
                    OnPropertyChanged();
                }
            }
        }

        [Display(Name = "FlowBlockTestDataset_FieldTestConfigurations", Description = "FlowBlockTestDataset_FieldTestConfigurations_Description", ResourceType = typeof(FlowBloxTexts))]
        public List<FlowBloxFieldTestConfiguration> FlowBloxTestConfigurations
        {
            get => _flowBloxTestConfigurations;
            set
            {
                if (_flowBloxTestConfigurations != value)
                {
                    _flowBloxTestConfigurations = value;
                    OnPropertyChanged();
                }
            }
        }

        public FlowBlockTestDataset()
        {
            FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>();
        }
    }
}
