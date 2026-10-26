using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Actions
{
    public sealed class FlowBloxReactiveObjectDeleteAction : FlowBloxBaseAction
    {
        public IReadOnlyList<IManagedObject> ManagedObjects { get; init; } = Array.Empty<IManagedObject>();
        public IReadOnlyList<IManagedObject> HistoryManagedObjects { get; init; } = Array.Empty<IManagedObject>();

        public override void Undo()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            foreach (var managedObject in HistoryManagedObjects.Reverse())
                registry.RegisterManagedObject(managedObject);
            base.Undo();
        }

        public override void Invoke()
        {
            base.Invoke();
            var registry = FlowBloxRegistryProvider.GetRegistry();
            for (var i = 0; i < ManagedObjects.Count; i++)
            {
                var current = ManagedObjects[i];
                var history = HistoryManagedObjects[i];
                registry.RemoveManagedbject(registry.GetManagedObjects().Contains(current) ? current : history);
            }
        }
    }
}
