using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.UICore.Actions
{
    public class FlowBloxEditAction : FlowBloxBaseAction
    {
        public object Target { get; set; }
        public object RestoreObject { get; set; }
        public object RepetitionObject { get; set; }
        
        public override void Undo()
        {
            var deepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(this.Target));
            deepCopier.Copy(this.RestoreObject, this.Target);
            OnAfterExecute();
            base.Undo();
        }

        private void OnAfterExecute()
        {
            if (Target is BaseFlowBlock)
                OnAfterExecute((BaseFlowBlock)Target);
        }

        private void OnAfterExecute(BaseFlowBlock target)
        {
            target.OnAfterSave();
        }

        public override void Invoke()
        {
            var deepCopier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(this.Target));
            deepCopier.Copy(this.RepetitionObject, this.Target);
            OnAfterExecute();
            base.Invoke();
        }
    }
}