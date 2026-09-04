using FlowBlox.Core.Interfaces;

namespace FlowBlox.UICore.Utilities
{
    internal sealed class FlowBloxComponentChangeSubscription : IDisposable
    {
        private readonly IFlowBloxComponent _rootComponent;
        private readonly Action<IFlowBloxComponent> _componentChanged;
        private readonly HashSet<IFlowBloxComponent> _subscribedComponents = new();
        private bool _disposed;

        public FlowBloxComponentChangeSubscription(
            IFlowBloxComponent rootComponent,
            Action<IFlowBloxComponent> componentChanged)
        {
            _rootComponent = rootComponent ?? throw new ArgumentNullException(nameof(rootComponent));
            _componentChanged = componentChanged ?? throw new ArgumentNullException(nameof(componentChanged));

            Rebind();
        }

        public void Rebind()
        {
            if (_disposed)
                return;

            var components = new HashSet<IFlowBloxComponent> { _rootComponent };
            foreach (var managedObject in _rootComponent.GetAssociatedManagedObjects().Where(x => x != null))
                components.Add(managedObject);

            foreach (var component in _subscribedComponents.Except(components).ToList())
                Unsubscribe(component);

            foreach (var component in components)
                Subscribe(component);
        }

        private void Subscribe(IFlowBloxComponent component)
        {
            if (!_subscribedComponents.Add(component))
                return;

            component.ComponentChanged -= Component_ComponentChanged;
            component.ComponentChanged += Component_ComponentChanged;
        }

        private void Unsubscribe(IFlowBloxComponent component)
        {
            component.ComponentChanged -= Component_ComponentChanged;
            _subscribedComponents.Remove(component);
        }

        private void Component_ComponentChanged(object? sender, EventArgs e)
        {
            if (_disposed)
                return;

            if (ReferenceEquals(sender, _rootComponent))
                Rebind();

            if (sender is IFlowBloxComponent component)
                _componentChanged(component);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var component in _subscribedComponents.ToList())
                Unsubscribe(component);
        }
    }
}