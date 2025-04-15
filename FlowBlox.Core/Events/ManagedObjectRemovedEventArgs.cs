using FlowBlox.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
