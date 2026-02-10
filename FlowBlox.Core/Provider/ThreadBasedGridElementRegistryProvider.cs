using FlowBlox.Core.Provider.Registry;

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
