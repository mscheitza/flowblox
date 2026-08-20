using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Reflection;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxPropertyChangeAction : FlowBloxBaseAction
    {
        public object Target { get; set; }

        public string PropertyName { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public override void Undo()
        {
            SetValue(OldValue);
            base.Undo();
        }

        public override void Invoke()
        {
            SetValue(NewValue);
            base.Invoke();
        }

        private void SetValue(object value)
        {
            var property = Target?.GetType().GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property?.CanWrite != true)
                return;

            property.SetValue(Target, value);

            if (Target is BaseFlowBlock flowBlock)
                flowBlock.PropertyValuesChanged();
        }
    }
}
