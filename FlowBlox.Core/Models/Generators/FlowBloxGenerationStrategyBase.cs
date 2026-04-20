using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.Selection;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Util.Fields;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Generators
{
    public abstract class FlowBloxGenerationStrategyBase : FlowBloxReactiveObject
    {
        protected FlowBloxGenerationStrategyBase()
        {
        }

        protected FlowBloxGenerationStrategyBase(BaseFlowBlock flowBlock)
        {
            Name = GetNameInContextOf(flowBlock);
            Source = flowBlock;
        }

        private string GetNameInContextOf(BaseFlowBlock flowBlock)
        {
            string baseName = GetType().Name;
            string name = baseName + "_0";
            int counter = 0;

            while (flowBlock.GenerationStrategies.Any(gs => gs.Name == name))
            {
                counter++;
                name = baseName + "_" + counter;
            }

            return name;
        }

        [Display(Name = "Global_Name", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        public string Name { get; set; }

        [Display(Name = "Global_InputField", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(FlowBloxComponent.GetPossibleFieldElements))]
        [Required()]
        public virtual FieldElement InputField { get; set; }

        private BaseFlowBlock _source;

        public BaseFlowBlock Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                OnAfterSourceChanged();
            }
        }

        protected virtual void OnAfterSourceChanged()
        {
            if (Source == null)
                return;

            if (Source is BasePipeFlowBlock)
            {
                var pipeFlowBlock = (BasePipeFlowBlock)Source;

                if (pipeFlowBlock.InputField != null)
                    InputField = pipeFlowBlock.InputField;

                pipeFlowBlock.PropertyChanged -= pipeFlowBlock_PropertyChange;
                pipeFlowBlock.PropertyChanged += pipeFlowBlock_PropertyChange;
            }
        }

        private void pipeFlowBlock_PropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SequenceDetectionFlowBlock.InputField))
            {
                var pipeFlowBlock = (BasePipeFlowBlock)Source;
                InputField = pipeFlowBlock.InputField;
            }
        }

        public abstract bool CanExecute(out Dictionary<FlowBloxTestDefinition, List<string>> testDefinitionToMessages, out List<string> messages);


        public abstract object Execute(BaseRuntime runtime, Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults);

        public abstract void Assign(object value);
    }
}
