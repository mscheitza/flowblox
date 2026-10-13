using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    [FlowBloxSpecialExplanation("BaseMultiResultCollectorFlowBlock_SpecialExplanation_IterationScope", Icon = SpecialExplanationIcon.Information)]
    [FlowBloxSpecialExplanation("BaseMultiResultCollectorFlowBlock_SpecialExplanation_AutoIterationContext", Icon = SpecialExplanationIcon.Hint)]
    [FlowBloxSpecialExplanation("BaseMultiResultCollectorFlowBlock_SpecialExplanation_ManualOverride", Icon = SpecialExplanationIcon.Important)]
    public abstract class BaseMultiResultCollectorFlowBlock : BaseResultFlowBlock
    {
        private readonly List<Dictionary<FieldElement, string>> _stagedValues = [];

        [JsonIgnore]
        [DeepCopierIgnore]
        internal CollectorCore Collector { get; } = new CollectorCore();

        protected IReadOnlyList<Dictionary<FieldElement, string>> StagedValues => _stagedValues;

        protected virtual void ResetStagedValues() => _stagedValues.Clear();

        protected void StageValue(Dictionary<FieldElement, string> value)
        {
            if (value != null)
                _stagedValues.Add(new Dictionary<FieldElement, string>(value));
        }

        protected void StageValues(IEnumerable<Dictionary<FieldElement, string>> values)
        {
            if (values == null)
                return;

            foreach (var value in values)
                StageValue(value);
        }

        protected bool CanGenerateResult()
        {
            if (InputDatasets_Count <= 0)
                return true;

            return InputDatasets_CurrentIndex == InputDatasets_Count - 1;
        }

        public override bool CanDisplayAssociatedIterationContextHint() =>
            Collector.CanDisplayAssociatedIterationContextHint(this, base.CanDisplayAssociatedIterationContextHint());

        public override BaseFlowBlock IterationContext =>
            Collector.ResolveIterationContext(this, base.IterationContext);

        private void BaseMultiResultCollectorFlowBlock_OnBeforeInputProcessing()
        {
            ResetStagedValues();
        }

        public override void RuntimeStarted(Models.Runtime.BaseRuntime runtime)
        {
            OnBeforeInputProcessing -= BaseMultiResultCollectorFlowBlock_OnBeforeInputProcessing;
            OnBeforeInputProcessing += BaseMultiResultCollectorFlowBlock_OnBeforeInputProcessing;

            base.RuntimeStarted(runtime);
        }

        public override void RuntimeFinished(Models.Runtime.BaseRuntime runtime)
        {
            OnBeforeInputProcessing -= BaseMultiResultCollectorFlowBlock_OnBeforeInputProcessing;

            base.RuntimeFinished(runtime);
        }
    }
}
