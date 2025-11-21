using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using System.Reflection;

namespace FlowBlox.UICore.Factory.PropertyView.HintTextResolver
{
    public class AssociatedFlowBlockResolvableCustomHintTextResolver
    {
        private object _target;
        private PropertyInfo _property;

        public AssociatedFlowBlockResolvableCustomHintTextResolver(object target, PropertyInfo property)
        {
            _target = target;
            _property = property;
        }

        public string ResolveHintText(AssociatedFlowBlockResolvableCustomAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            if (_target == null)
                return null;

            if (!string.IsNullOrWhiteSpace(attribute.DisplayCondition))
            {
                var conditionMember = _target.GetType().GetProperty(
                                          attribute.DisplayCondition,
                                          BindingFlags.Public | 
                                          BindingFlags.NonPublic | 
                                          BindingFlags.Instance)
                                      as MemberInfo
                                      ?? _target.GetType().GetMethod(
                                          attribute.DisplayCondition,
                                          BindingFlags.Public | 
                                          BindingFlags.NonPublic | 
                                          BindingFlags.Instance,
                                          null,
                                          Type.EmptyTypes,
                                          null);

                if (conditionMember == null)
                {
                    throw new InvalidOperationException(
                        $"DisplayCondition member '{attribute.DisplayCondition}' was not found on type '{_target.GetType().FullName}'.");
                }

                bool shouldDisplay = false;
                if (conditionMember is PropertyInfo condProp)
                {
                    if (condProp.PropertyType != typeof(bool))
                        throw new InvalidOperationException(
                            $"DisplayCondition property '{attribute.DisplayCondition}' must be of type bool.");

                    shouldDisplay = (bool)(condProp.GetValue(_target) ?? false);
                }
                else if (conditionMember is MethodInfo condMethod)
                {
                    if (condMethod.ReturnType != typeof(bool))
                        throw new InvalidOperationException(
                            $"DisplayCondition method '{attribute.DisplayCondition}' must return bool.");

                    shouldDisplay = (bool)(condMethod.Invoke(_target, null) ?? false);
                }

                if (!shouldDisplay)
                    return null;
            }

            var currentValue = _property.GetValue(_target);
            if (currentValue is BaseFlowBlock)
                return null;

            if (string.IsNullOrWhiteSpace(attribute.MemberName))
                throw new InvalidOperationException("CustomFlowBlockResolvableAttribute.MemberName must not be null or empty.");

            var member = _target.GetType().GetProperty(
                             attribute.MemberName,
                             BindingFlags.Public | 
                             BindingFlags.NonPublic | 
                             BindingFlags.Instance)
                         as MemberInfo
                         ?? _target.GetType().GetMethod(
                             attribute.MemberName,
                             BindingFlags.Public | 
                             BindingFlags.NonPublic | 
                             BindingFlags.Instance,
                             null,
                             Type.EmptyTypes,
                             null);

            if (member == null)
            {
                throw new InvalidOperationException(
                    $"Member '{attribute.MemberName}' was not found on type '{_target.GetType().FullName}'.");
            }

            object resolved = null;

            if (member is PropertyInfo propInfo)
                resolved = propInfo.GetValue(_target);
            else if (member is MethodInfo methodInfo)
                resolved = methodInfo.Invoke(_target, null);

            if (resolved is BaseFlowBlock resolvedFlowBlock)
            {
                return string.Format(
                    FlowBloxResourceUtil.GetLocalizedString("AssociationControlFactory_Resolvable", typeof(FlowBloxTexts)),
                    resolvedFlowBlock.Name);
            }

            return FlowBloxResourceUtil.GetLocalizedString("AssociationControlFactory_NotResolvable", typeof(FlowBloxTexts));
        }
    }
}
