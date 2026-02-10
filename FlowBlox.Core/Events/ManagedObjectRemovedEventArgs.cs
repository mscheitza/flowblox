using FlowBlox.Core.Interfaces;

namespace FlowBlox.Core.Events
{
    public class ManagedObjectRemovedEventArgs : EventArgs
    {
        public IManagedObject RemovedObject { get; }

        public ManagedObjectRemovedEventArgs(IManagedObject removedObject)
        {
            RemovedObject = removedObject;
        }
    }
}
