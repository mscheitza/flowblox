using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Resources;
using System.Collections.Generic;
using System.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    [FlowBloxSpecialExplanation("BaseSingleResultCollectorFlowBlock_SpecialExplanation_IterationScope", Icon = SpecialExplanationIcon.Information)]
    [FlowBloxSpecialExplanation("BaseSingleResultCollectorFlowBlock_SpecialExplanation_AutoIterationContext", Icon = SpecialExplanationIcon.Hint)]
    [FlowBloxSpecialExplanation("BaseSingleResultCollectorFlowBlock_SpecialExplanation_ManualOverride", Icon = SpecialExplanationIcon.Important)]
    public abstract class BaseSingleResultCollectorFlowBlock : BaseSingleResultFlowBlock
    {
        private readonly List<string> _stagedValues = new List<string>();

        protected IReadOnlyList<string> StagedValues => _stagedValues;

        protected void ResetStagedValues() => _stagedValues.Clear();

        protected void StageValue(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                _stagedValues.Add(value);
        }

        protected void StageValues(IEnumerable<string> values)
        {
            foreach (var value in values)
                StageValue(value);
        }

        protected bool CanGenerateResult()
        {
            if (InputDatasets_Count <= 0)
                return true;

            return InputDatasets_CurrentIndex == InputDatasets_Count - 1;
        }

        public override bool CanDisplayAssociatedIterationContextHint()
        {
            if (ReferencedFlowBlocks.OfType<BaseResultFlowBlock>().Count() == 1 &&
                AssociatedIterationContext == null)
            {
                return true;
            }

            return base.CanDisplayAssociatedIterationContextHint();
        }

        public override BaseFlowBlock IterationContext
        {
            get
            {
                if (AssociatedIterationContext != null)
                    return AssociatedIterationContext;

                if (ReferencedFlowBlocks.OfType<BaseResultFlowBlock>().Count() == 1)
                {
                    var referencedFlowBlock = ReferencedFlowBlocks.OfType<BaseResultFlowBlock>().Single();
                    if (referencedFlowBlock.ReferencedFlowBlocks.Count == 1)
                        return referencedFlowBlock.ReferencedFlowBlocks.Single();

                    return referencedFlowBlock.IterationContext;
                }

                return base.IterationContext;
            }
        }

        private void BaseSingleResultCollectorFlowBlock_OnBeforeInputProcessing()
        {
            ResetStagedValues();
        }

        public override void RuntimeStarted(Models.Runtime.BaseRuntime runtime)
        {
            OnBeforeInputProcessing -= BaseSingleResultCollectorFlowBlock_OnBeforeInputProcessing;
            OnBeforeInputProcessing += BaseSingleResultCollectorFlowBlock_OnBeforeInputProcessing;

            base.RuntimeStarted(runtime);
        }

        public override void RuntimeFinished(Models.Runtime.BaseRuntime runtime)
        {
            OnBeforeInputProcessing -= BaseSingleResultCollectorFlowBlock_OnBeforeInputProcessing;

            base.RuntimeFinished(runtime);
        }
    }
}