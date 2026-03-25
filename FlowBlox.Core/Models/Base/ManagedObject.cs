using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Grid.Elements.Util;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Base
{
    [Display(Name = "TypeNames_ManagedObject", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("TypeNames_ManagedObject_Plural", typeof(FlowBloxTexts))]
    public abstract class ManagedObject : FlowBloxComponent, IManagedObject
    {
        public override bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            base.IsDeletable(out dependencies);

            var registry = FlowBloxRegistryProvider.GetRegistry();

            foreach (var flowBlock in registry.GetFlowBlocks())
            {
                if (flowBlock.GetAssociatedManagedObjects().Contains(registry.GetOriginalRef(this)))
                    dependencies.AddIfNotExists(flowBlock);
            }

            foreach (var managedObject in registry.GetManagedObjects())
            {
                if (managedObject.GetAssociatedManagedObjects().Contains(registry.GetOriginalRef(this)))
                    dependencies.AddIfNotExists(managedObject);
            }

            return !dependencies.Any();
        }

        public override string ToString() => $"{FlowBloxComponentHelper.GetDisplayName(this)} \"{this.Name}\"";
    }
}
