using FlowBlox.Core.Interfaces;

namespace FlowBlox.UICore.Interfaces
{
    public abstract class ComponentUIActions<T> where T : IFlowBloxComponent
    {
        public T Component { get; set; }

        protected ComponentUIActions(T component)
        {
            Component = component;
        }
    }
}
