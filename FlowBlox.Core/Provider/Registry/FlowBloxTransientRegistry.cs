using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Provider.Registry
{
    public class FlowBloxTransientRegistry : FlowBloxRegistry
    {
        private FlowBloxRegistry _parentRegistry;
        private HashSet<IManagedObject> _transientManagedObjects;
        private HashSet<IManagedObject> _unregisteredManagedObjects;

        public FlowBloxTransientRegistry(FlowBloxRegistry parentRegistry) : base(parentRegistry)
        {
            _parentRegistry = parentRegistry;
            _transientManagedObjects = new HashSet<IManagedObject>();
            _unregisteredManagedObjects = new HashSet<IManagedObject>();
        }

        public override void RegisterManagedObject(IManagedObject managedObject)
        {
            _transientManagedObjects.Add(managedObject);
        }

        public override void RemoveManagedbject(IManagedObject managedObject)
        {
            _transientManagedObjects.Remove(managedObject);
            _unregisteredManagedObjects.Add(managedObject);
        }

        public override IEnumerable<IManagedObject> GetManagedObjects()
        {
            return base.GetManagedObjects()
                .Concat(_transientManagedObjects)
                .Except(_unregisteredManagedObjects);
        }

        public override IEnumerable<T> GetManagedObjects<T>()
        {
            return base.GetManagedObjects<T>()
                .Concat(_transientManagedObjects.OfType<T>())
                .Except(_unregisteredManagedObjects.OfType<T>());
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
            foreach (var obj in _transientManagedObjects)
            {
                _parentRegistry.RegisterManagedObject(obj);
            }
            foreach (var obj in _unregisteredManagedObjects)
            {
                _parentRegistry.RemoveManagedbject(obj);
            }

            _transientManagedObjects.Clear();
            _unregisteredManagedObjects.Clear();
        }
    }
}
