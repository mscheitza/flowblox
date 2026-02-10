using FlowBlox.Core.Interfaces;

namespace FlowBlox.Core.Events
{
    public class ManagedObjectAddedEventArgs : EventArgs
    {
        public IManagedObject AddedObject { get; }

        public ManagedObjectAddedEventArgs(IManagedObject addedObject)
        {
            AddedObject = addedObject;
        }
    }
}