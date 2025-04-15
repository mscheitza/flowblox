using FlowBlox.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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