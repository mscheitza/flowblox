using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Provider.Registry
{
    public class FlowBloxTransientRegistry : FlowBloxRegistry
    {
        private FlowBloxRegistry _parentRegistry;
        private HashSet<IManagedObject> _transientManagedObjects;
        private HashSet<IManagedObject> _unregisteredManagedObjects;
        private HashSet<BaseFlowBlock> _stagedDeletedFlowBlocks;
        private HashSet<IManagedObject> _stagedDeletedManagedObjects;
        private HashSet<BaseFlowBlock> _unregisteredFlowBlocks;

        public override bool SupportsDeletionStaging => true;

        public FlowBloxTransientRegistry(FlowBloxRegistry parentRegistry) : base(parentRegistry)
        {
            _parentRegistry = parentRegistry;
            _transientManagedObjects = new HashSet<IManagedObject>();
            _unregisteredManagedObjects = new HashSet<IManagedObject>();
            _stagedDeletedFlowBlocks = new HashSet<BaseFlowBlock>();
            _stagedDeletedManagedObjects = new HashSet<IManagedObject>();
            _unregisteredFlowBlocks = new HashSet<BaseFlowBlock>();
        }

        public override void StageDeletion(object target)
        {
            if (target is BaseFlowBlock flowBlock)
                _stagedDeletedFlowBlocks.Add(Reload(flowBlock));
            if (target is IManagedObject managedObject)
                _stagedDeletedManagedObjects.Add(Reload(managedObject));
        }

        public override IEnumerable<BaseFlowBlock> GetFlowBlocks()
        {
            return base.GetFlowBlocks()
                .Except(_stagedDeletedFlowBlocks)
                .Except(_unregisteredFlowBlocks);
        }

        public override void RemoveFlowBlock(BaseFlowBlock flowBlock)
        {
            _stagedDeletedFlowBlocks.Remove(flowBlock);
            _unregisteredFlowBlocks.Add(flowBlock);
        }

        public override void RegisterManagedObject(IManagedObject managedObject)
        {
            _transientManagedObjects.Add(managedObject);
        }

        public override void RemoveManagedbject(IManagedObject managedObject)
        {
            _transientManagedObjects.Remove(managedObject);
            _stagedDeletedManagedObjects.Remove(managedObject);
            _unregisteredManagedObjects.Add(managedObject);
        }

        public override IEnumerable<IManagedObject> GetManagedObjects()
        {
            return base.GetManagedObjects()
                .Concat(_transientManagedObjects)
                .Except(_stagedDeletedManagedObjects)
                .Except(_unregisteredManagedObjects);
        }

        public override IEnumerable<T> GetManagedObjects<T>()
        {
            return GetManagedObjects().OfType<T>();
        }

        public override void ReplaceRef(IManagedObject from, IManagedObject to)
        {
            base.ReplaceRef(from, to);

            if (_transientManagedObjects.Contains(from))
            {
                _transientManagedObjects.Remove(from);
                _transientManagedObjects.Add(to);
            }

            if (_unregisteredManagedObjects.Contains(from))
            {
                _unregisteredManagedObjects.Remove(from);
                _unregisteredManagedObjects.Add(to);
            }
        }

        public void Commit()
        {
            if (_parentRegistry == null)
                return;

            foreach (var obj in _transientManagedObjects)
            {
                _parentRegistry.RegisterManagedObject(obj);
            }
            foreach (var obj in _unregisteredManagedObjects)
            {
                _parentRegistry.RemoveManagedbject((ManagedObject)GetOriginalRef(obj));
            }
            foreach (var flowBlock in _unregisteredFlowBlocks)
            {
                _parentRegistry.RemoveFlowBlock((BaseFlowBlock)GetOriginalRef(flowBlock));
            }

            _transientManagedObjects.Clear();
            _unregisteredManagedObjects.Clear();
            _unregisteredFlowBlocks.Clear();
            _stagedDeletedManagedObjects.Clear();
            _stagedDeletedFlowBlocks.Clear();
        }
    }
}
