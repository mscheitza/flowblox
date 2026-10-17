using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.UICore.ViewModels.ProjectPanel
{
    internal sealed class FlowBlockNodePropertyRenderer
    {
        private readonly BaseFlowBlock _flowBlock;

        public FlowBlockNodePropertyRenderer(BaseFlowBlock flowBlock)
        {
            _flowBlock = flowBlock ?? throw new ArgumentNullException(nameof(flowBlock));
        }

        public void Render(ICollection<FlowBlockRenderRowViewModel> rows)
        {
            ArgumentNullException.ThrowIfNull(rows);

            RenderProperties(rows);
            RenderFields(rows);
            RenderRequiredFields(rows);
            RenderActivationConditions(rows);
        }

        private void RenderProperties(ICollection<FlowBlockRenderRowViewModel> rows)
        {
            foreach (var propertyName in _flowBlock.GetDisplayableProperties())
            {
                var property = _flowBlock.GetType().GetProperty(propertyName);
                if (property == null)
                    continue;

                var propertyValue = FlowBloxFieldHelper.GetPropertyValueOrSelectedField(_flowBlock, property);
                if (propertyValue == null)
                    continue;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
                    property.PropertyType.IsGenericType)
                {
                    RenderListProperties(rows, property, propertyValue);
                    continue;
                }

                var displayValue = CleanValue(propertyValue.ToString());
                if (string.IsNullOrWhiteSpace(displayValue))
                    continue;

                rows.Add(new FlowBlockRenderRowViewModel(
                    "property",
                    ResolveDisplayName(property, propertyName),
                    displayValue,
                    propertyValue.ToString(),
                    _flowBlock,
                    propertyName));
            }
        }

        private void RenderListProperties(
            ICollection<FlowBlockRenderRowViewModel> rows,
            PropertyInfo property,
            object propertyValue)
        {
            var list = (System.Collections.IEnumerable)propertyValue;
            var itemType = propertyValue.GetType().GetGenericArguments()[0];
            var propertyName = property.Name;
            var cardListAttribute = property.GetCustomAttribute<FlowBloxCardListAttribute>();
            var itemDisplayName = ResolveListItemDisplayName(property, itemType, cardListAttribute);
            var index = 0;

            foreach (var item in list)
            {
                var renderedProperties = new List<FlowBlockRenderRowViewModel>();
                foreach (var itemProperty in GetCardItemProperties(itemType, cardListAttribute))
                {
                    var displayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttribute == null)
                        continue;

                    var itemPropertyValue = itemProperty.GetValue(item);
                    if (itemPropertyValue == null)
                        continue;

                    var displayValue = CleanValue(itemPropertyValue.ToString());
                    if (string.IsNullOrWhiteSpace(displayValue))
                        continue;

                    renderedProperties.Add(new FlowBlockRenderRowViewModel(
                        "property",
                        FlowBloxResourceUtil.GetDisplayName(displayAttribute, true),
                        displayValue,
                        itemPropertyValue.ToString(),
                        _flowBlock,
                        $"{propertyName}[{index}].{itemProperty.Name}",
                        item));
                }

                if (renderedProperties.Count > 0)
                {
                    rows.Add(new FlowBlockRenderRowViewModel(
                        "header",
                        $"{itemDisplayName} #{index + 1}",
                        string.Empty));

                    foreach (var renderedProperty in renderedProperties)
                        rows.Add(renderedProperty);
                }

                index++;
            }
        }

        private static string ResolveListItemDisplayName(
            PropertyInfo listProperty,
            Type itemType,
            FlowBloxCardListAttribute cardListAttribute)
        {
            if (string.IsNullOrEmpty(cardListAttribute?.ItemDisplayName))
            {
                var itemDisplayAttribute = itemType.GetCustomAttribute<DisplayAttribute>();
                return itemDisplayAttribute != null
                    ? FlowBloxResourceUtil.GetDisplayName(itemDisplayAttribute, true)
                    : listProperty.Name;
            }

            return FlowBloxResourceUtil.GetDisplayName(new DisplayAttribute
            {
                Name = cardListAttribute.ItemDisplayName,
                ResourceType = listProperty.GetCustomAttribute<DisplayAttribute>()?.ResourceType
            });
        }

        private static IEnumerable<PropertyInfo> GetCardItemProperties(
            Type itemType,
            FlowBloxCardListAttribute cardListAttribute)
        {
            var properties = itemType.GetProperties();
            if (cardListAttribute?.DisplayPropertyNames == null ||
                cardListAttribute.DisplayPropertyNames.Length == 0)
            {
                return properties;
            }

            var propertyMap = properties.ToDictionary(property => property.Name, StringComparer.Ordinal);
            return cardListAttribute.DisplayPropertyNames
                .Where(propertyName => !string.IsNullOrWhiteSpace(propertyName))
                .Distinct(StringComparer.Ordinal)
                .Select(propertyName => propertyMap.GetValueOrDefault(propertyName))
                .OfType<PropertyInfo>();
        }

        private void RenderFields(ICollection<FlowBlockRenderRowViewModel> rows)
        {
            if (_flowBlock is not BaseResultFlowBlock resultFlowBlock)
                return;

            foreach (var field in resultFlowBlock.Fields)
            {
                rows.Add(new FlowBlockRenderRowViewModel(
                    "field",
                    string.Empty,
                    field.Name,
                    field.FullyQualifiedName,
                    field,
                    parent: _flowBlock));

                foreach (var modifier in field.Modifiers)
                {
                    rows.Add(new FlowBlockRenderRowViewModel(
                        "modifier",
                        "mod",
                        CleanValue(modifier.ToString()),
                        modifier.ToString(),
                        modifier,
                        parent: _flowBlock));
                }

                foreach (var condition in field.Conditions)
                {
                    rows.Add(new FlowBlockRenderRowViewModel(
                        "condition",
                        "cond",
                        CleanValue(condition.ShortDisplayName),
                        condition.DisplayName,
                        condition,
                        parent: _flowBlock));
                }
            }
        }

        private void RenderRequiredFields(ICollection<FlowBlockRenderRowViewModel> rows)
        {
            foreach (var requiredFieldContext in _flowBlock.GetRequiredFieldContexts())
            {
                rows.Add(new FlowBlockRenderRowViewModel(
                    "required",
                    "req",
                    requiredFieldContext.FieldElement.Name,
                    requiredFieldContext.FieldElement.FullyQualifiedName,
                    requiredFieldContext.FlowBloxComponent,
                    nameof(FlowBloxComponent.RequiredFields),
                    requiredFieldContext.FieldElement));
            }
        }

        private void RenderActivationConditions(ICollection<FlowBlockRenderRowViewModel> rows)
        {
            foreach (var condition in _flowBlock.ActivationConditions)
            {
                rows.Add(new FlowBlockRenderRowViewModel(
                    "activation",
                    "if",
                    CleanValue(condition.ShortDisplayName),
                    condition.DisplayName,
                    condition,
                    parent: _flowBlock));
            }
        }

        private static string ResolveDisplayName(MemberInfo member, string fallback)
        {
            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute != null
                ? FlowBloxResourceUtil.GetDisplayName(displayAttribute, true)
                : fallback;
        }

        private static string CleanValue(string value)
            => string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace(Environment.NewLine, " ").Trim();
    }
}
