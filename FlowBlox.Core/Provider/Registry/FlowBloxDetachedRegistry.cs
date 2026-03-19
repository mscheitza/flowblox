using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Provider.Registry
{
    public class FlowBloxDetachedRegistry : FlowBloxRegistry
    {
        public FlowBloxDetachedRegistry(FlowBloxRegistry parentRegistry) : base(parentRegistry)
        {
        }

        public override void RegisterFlowBlock(BaseFlowBlock flowBlock)
        {
            // Detached mode: do not track flow-block registrations.
        }

        public override void RemoveFlowBlock(BaseFlowBlock flowBlock)
        {
            // Detached mode: do not track flow-block unregistrations.
        }

        public override void RegisterManagedObject(IManagedObject managedObject)
        {
            // Detached mode: do not track managed-object registrations.
        }

        public override void RemoveManagedbject(IManagedObject managedObject)
        {
            // Detached mode: do not track managed-object unregistrations.
        }

        public void Commit()
        {
            throw new InvalidOperationException($"{nameof(FlowBloxDetachedRegistry)} does not support commit.");
        }
    }
}
