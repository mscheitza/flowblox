using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using System.ComponentModel;
using System.Reflection;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public class FlowBlockHelper
    {
        public static List<FieldElement> GetFieldElementsOfAccoiatedFlowBlocks(BaseFlowBlock flowBlock)
        {
            var result = new List<FieldElement>();
            var referendedFlowBlock = flowBlock.ReferencedFlowBlocks.SingleOrDefault();
            if (referendedFlowBlock is BaseResultFlowBlock)
                result.AddRange(((BaseResultFlowBlock)referendedFlowBlock).Fields);
            return result;
        }

        public static string GetDescription(BaseFlowBlock flowBlock)
        {
            var type = flowBlock.GetType();
            string description = null;
            var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                description = FlowBloxResourceUtil.GetLocalizedString(descriptionAttribute.Description);

            if (!string.IsNullOrEmpty(description))
                return description;
            else
                return string.Format(FlowBloxResourceUtil.GetLocalizedString("BaseFlowBlock_DefaultDescription"), [FlowBloxComponentHelper.GetDisplayName(flowBlock)]);
        }

        public static void ApplyFieldSelectionRequiredOption(object target, IEnumerable<FieldElement> fieldElements, bool isRequired)
        {
            if (target == null)
                return;

            foreach (var fieldElement in fieldElements)
            {
                if (fieldElement == null)
                    continue;

                if (target is BaseFlowBlock flowBlock)
                {
                    flowBlock.SetFieldRequirement(fieldElement, isRequired);
                }
                else if (target is ManagedObject managedObject && managedObject.HandleRequirements)
                {
                    managedObject.SetFieldRequirement(fieldElement, isRequired);
                }
            }
        }
    }
}
