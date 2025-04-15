using FlowBlox.Core.Provider.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Provider
{
    public class ThreadBasedGridElementRegistryProvider : BaseThreadProvider<FlowBloxRegistry>
    {
        public ThreadBasedGridElementRegistryProvider(FlowBloxRegistry registry) : base(registry)
        {
        }

        public static FlowBloxRegistry GetManagedObject() => GetManagedObject(typeof(FlowBloxRegistry));
    }
}
