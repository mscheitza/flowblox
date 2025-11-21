using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using System.Reflection;

namespace FlowBlox.UICore.Factory.PropertyView.HintTextResolver
{
    public class AssociatedFlowBlockResolvableHintTextResolver
    {
        private object _target;
        private PropertyInfo _property;

        public AssociatedFlowBlockResolvableHintTextResolver(object target, PropertyInfo property)
        {
            _target = target;
            _property = property;
        }

        public string ResolveHintText(AssociatedFlowBlockResolvableAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            if (_target == null)
                return null;

            var currentValue = _property.GetValue(_target);
            if (currentValue is BaseFlowBlock)
                return null;

            var method = typeof(BaseFlowBlock).GetMethod(
                GlobalConstants.GetPreviousFlowBlockOnPathMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                [typeof(BaseFlowBlock), typeof(Type[])],
                null);

            if (method != null && _target is BaseFlowBlock flowBlock)
            {
                var requiredType = _property.PropertyType;
                var result = method.Invoke(flowBlock, [flowBlock, new[] { requiredType }]);

                if (result is BaseFlowBlock resolvedViaPath)
                {
                    return string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("AssociationControlFactory_Resolvable", typeof(FlowBloxTexts)),
                        resolvedViaPath.Name);
                }
            }

            return FlowBloxResourceUtil.GetLocalizedString("AssociationControlFactory_NotResolvable", typeof(FlowBloxTexts));
        }
    }
}
