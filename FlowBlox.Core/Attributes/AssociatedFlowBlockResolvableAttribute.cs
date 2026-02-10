using FlowBlox.Core.Constants;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.Core.Attributes
{
    /// <summary>
    /// Indicates that a property referencing a flow block outside of the flow logic must be resolvable either directly (by being set)
    /// or indirectly through the current flow path using <see cref="BaseFlowBlock.GetPreviousFlowBlockOnPath{T}(BaseFlowBlock)"/>.
    /// 
    /// This attribute is intended for associations to flow blocks that are not connected via visual flow logic
    /// but must still be resolvable at runtime for execution or validation purposes.
    ///
    /// <example>
    /// Example usage inside the <see cref="BaseFlowBlock.Execute" /> method:
    /// <code>
    /// var associatedFlowBlock = this.AssociatedFlowBlock ?? GetPreviousFlowBlockOnPath&lt;FlowBlockType&gt;(this);
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AssociatedFlowBlockResolvableAttribute : ValidationAttribute
    {
        public AssociatedFlowBlockResolvableAttribute()
        {
            
        }

        private Type ResolvePropertyType(ValidationContext context)
        {
            if (context.MemberName == null)
                return null;

            var property = context.ObjectType.GetProperty(context.MemberName);
            return property?.PropertyType;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var requiredFlowBlockType = ResolvePropertyType(validationContext);

            if (!typeof(BaseFlowBlock).IsAssignableFrom(requiredFlowBlockType))
                throw new InvalidOperationException("Type must be assignable to BaseFlowBlock.");

            if (value != null)
                return ValidationResult.Success;

            var method = typeof(BaseFlowBlock)
                .GetMethod(GlobalConstants.GetPreviousFlowBlockOnPathMethodName, 
                    BindingFlags.NonPublic | BindingFlags.Instance, 
                    [typeof(BaseFlowBlock), typeof(Type[])]);

            if (method != null)
            {
                var result = method.Invoke(validationContext.ObjectInstance, [validationContext.ObjectInstance, new[] { requiredFlowBlockType }]);
                if (result is BaseFlowBlock)
                    return ValidationResult.Success;
            }

            var displayName = validationContext.DisplayName ?? validationContext.MemberName;
            var message = FlowBloxResourceUtil.GetLocalizedString("AssociatedFlowBlockRequired_ValidationMessage", typeof(FlowBloxTexts));

            return new ValidationResult(string.Format(message, displayName, requiredFlowBlockType.Name), [validationContext.MemberName]);
        }
    }
}
