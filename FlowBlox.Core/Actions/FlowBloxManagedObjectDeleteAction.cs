using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Actions
{
    public sealed class FlowBloxManagedObjectDeleteAction : FlowBloxBaseAction
    {
        public IManagedObject ManagedObject { get; set; }
        public IManagedObject HistoryManagedObject { get; set; }

        public override void Undo()
        {
            FlowBloxRegistryProvider.GetRegistry().RegisterManagedObject(HistoryManagedObject ?? ManagedObject);
            base.Undo();
        }

        public override void Invoke()
        {
            base.Invoke();
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var target = registry.GetManagedObjects().Contains(ManagedObject)
                ? ManagedObject
                : HistoryManagedObject ?? ManagedObject;
            registry.RemoveManagedbject(target);
        }
    }
}
