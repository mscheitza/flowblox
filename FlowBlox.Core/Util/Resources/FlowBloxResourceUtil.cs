using FlowBlox.Core.Attributes;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace FlowBlox.Core.Util.Resources
{
    public static class FlowBloxResourceUtil
    {
        private const string GlobalPrefix = "Global";
        public static Type DefaultResourceType = typeof(FlowBloxTexts);

        private static string GetConcattedResourceName(params string[] nameParts) => string.Join("_", nameParts);

        public static string GetLocalizedString(params string[] nameParts) => GetLocalizedString(GetConcattedResourceName(nameParts));

        public static string GetLocalizedString(string name, Type resourceType = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (resourceType == null)
                resourceType = DefaultResourceType;

            ResourceManager resourceManager = new ResourceManager(resourceType);
            var localizedString = resourceManager.GetString(name);
            if (string.IsNullOrEmpty(localizedString))
                localizedString = resourceManager.GetString(GetConcattedResourceName(GlobalPrefix, name));
            return localizedString;
        }

        public static string GetDisplayName(DisplayAttribute displayAttribute, bool requireDisplayName = true)
        {
            if (displayAttribute == null)
                throw new ArgumentNullException(nameof(displayAttribute));

            string displayName;
            if (!string.IsNullOrEmpty(displayAttribute.Name))
            {
                if (displayAttribute.ResourceType != null)
                {
                    ResourceManager resourceManager = new ResourceManager(displayAttribute.ResourceType);
                    displayName = resourceManager.GetString(displayAttribute.Name);
                }
                else
                {
                    displayName = displayAttribute.Name;
                }
            }
            else
            {
                if (requireDisplayName)
                    throw new InvalidOperationException("The must be a name defined in display attribute.");
                else
                    return default;
            }
            return displayName;
        }

        public static string GetDescription(DisplayAttribute displayAttribute)
        {
            if (displayAttribute == null)
                throw new ArgumentNullException(nameof(displayAttribute));

            string description = string.Empty;
            if (!string.IsNullOrEmpty(displayAttribute.Description))
            {
                if (displayAttribute.ResourceType != null)
                {
                    ResourceManager resourceManager = new ResourceManager(displayAttribute.ResourceType);
                    description = resourceManager.GetString(displayAttribute.Description);
                }
            }
            return description;
        }

        public static string GetPluralDisplayName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var pluralAttribute = type.GetCustomAttributes(typeof(PluralDisplayNameAttribute), inherit: false)
                .OfType<PluralDisplayNameAttribute>()
                .FirstOrDefault();

            if (pluralAttribute != null && !string.IsNullOrWhiteSpace(pluralAttribute.Name))
            {
                var localized = GetLocalizedString(pluralAttribute.Name, pluralAttribute.ResourceType);
                if (!string.IsNullOrWhiteSpace(localized))
                    return localized;
            }

            var displayAttribute = type.GetCustomAttributes(typeof(DisplayAttribute), inherit: false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            if (displayAttribute != null)
            {
                var singular = GetDisplayName(displayAttribute, requireDisplayName: false);
                if (!string.IsNullOrWhiteSpace(singular))
                    return $"{singular} {GetPluralFallbackSuffix()}";
            }

            return $"{type.Name} {GetPluralFallbackSuffix()}";
        }

        private static string GetPluralFallbackSuffix()
        {
            const string suffixResourceName = "FlowBloxResourceUtil_FallbackPluralSuffix";
            var suffix = GetLocalizedString(suffixResourceName, DefaultResourceType);
            return string.IsNullOrWhiteSpace(suffix) ? "Objects" : suffix;
        }

        public static SKImage LoadSKImageFromBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Image data is null or empty.", nameof(data));

            return SKImage.FromEncodedData(data);
        }
    }
}
