using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.ComponentModel;

namespace FlowBlox.UICore.Utilities
{
    internal sealed class FlowBloxDefinedManagedObjectsChangeSubscription : IDisposable
    {
        private readonly BaseFlowBlock _flowBlock;
        private readonly Action<IManagedObject> _managedObjectChanged;
        private readonly HashSet<INotifyPropertyChanged> _subscribedObjects = new();
        private bool _disposed;

        public FlowBloxDefinedManagedObjectsChangeSubscription(
            BaseFlowBlock flowBlock,
            Action<IManagedObject> managedObjectChanged)
        {
            _flowBlock = flowBlock ?? throw new ArgumentNullException(nameof(flowBlock));
            _managedObjectChanged = managedObjectChanged ?? throw new ArgumentNullException(nameof(managedObjectChanged));
            Rebind();
        }

        public void Rebind()
        {
            if (_disposed)
                return;

            var currentObjects = _flowBlock.DefinedManagedObjects
                .OfType<INotifyPropertyChanged>()
                .ToHashSet();

            foreach (var managedObject in _subscribedObjects.Except(currentObjects).ToList())
                Unsubscribe(managedObject);

            foreach (var managedObject in currentObjects)
                Subscribe(managedObject);
        }

        private void Subscribe(INotifyPropertyChanged managedObject)
        {
            if (_subscribedObjects.Add(managedObject))
                managedObject.PropertyChanged += ManagedObject_PropertyChanged;
        }

        private void Unsubscribe(INotifyPropertyChanged managedObject)
        {
            managedObject.PropertyChanged -= ManagedObject_PropertyChanged;
            _subscribedObjects.Remove(managedObject);
        }

        private void ManagedObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_disposed && sender is IManagedObject managedObject)
                _managedObjectChanged(managedObject);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var managedObject in _subscribedObjects.ToList())
                Unsubscribe(managedObject);
        }
    }
}
