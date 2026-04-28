using FlowBlox.Core.Actions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.DeepCopier;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxEditAction : FlowBloxBaseAction
    {
        public object Target { get; set; }
        public object RestoreObject { get; set; }
        public object RepetitionObject { get; set; }

        public override void Undo()
        {
            var deepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(Target));
            deepCopier.Copy(RestoreObject, Target);
            OnAfterExecute();
            base.Undo();
        }

        public override void Invoke()
        {
            var deepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(Target));
            deepCopier.Copy(RepetitionObject, Target);
            OnAfterExecute();
            base.Invoke();
        }

        private void OnAfterExecute()
        {
            if (Target is BaseFlowBlock flowBlock)
                flowBlock.OnAfterSave();
        }
    }
}
