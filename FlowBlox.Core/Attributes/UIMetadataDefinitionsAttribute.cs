using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System;
using System.Reflection;
using System.Resources;

namespace FlowBlox.Core.Attributes
{
    public readonly struct UIIconDefinition
    {
        public UIIconDefinition(Type resourceType, string svgKey, string coloring, int size)
        {
            ResourceType = resourceType;
            SVGKey = svgKey ?? string.Empty;
            Coloring = coloring ?? string.Empty;
            Size = size;
        }

        public Type ResourceType { get; }
        public string SVGKey { get; }
        public string Coloring { get; }
        public int Size { get; }
        public bool IsDefined => ResourceType != null && !string.IsNullOrWhiteSpace(SVGKey) && Size > 0;

        public bool TryCreateSKImage(out SKImage image)
        {
            image = null;
            if (!IsDefined)
                return false;

            var resourceManager = ResolveResourceManager(ResourceType);
            if (resourceManager?.GetObject(SVGKey) is not byte[] svgData)
                return false;

            try
            {
                var color = TryParseColor(Coloring, out var parsedColor)
                    ? parsedColor
                    : SKColors.SlateGray;
                image = FlowBloxIconUtil.CreateFromSVG(svgData, Size, color);
                return image != null;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseColor(string hexColor, out SKColor color)
        {
            color = SKColors.SlateGray;
            if (string.IsNullOrWhiteSpace(hexColor))
                return false;

            return SKColor.TryParse(hexColor, out color);
        }

        private static ResourceManager ResolveResourceManager(Type resourceType)
        {
            var property = resourceType.GetProperty(
                "ResourceManager",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            return property?.GetValue(null) as ResourceManager;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UIMetadataDefinitionsAttribute : Attribute
    {
        public UIMetadataDefinitionsAttribute(Type uiIconResourceType, string uiIconSVGKey)
            : this(uiIconResourceType, uiIconSVGKey, "#546E7A", 16)
        {
        }

        public UIMetadataDefinitionsAttribute(Type uiIconResourceType, string uiIconSVGKey, string uiIconColoring)
            : this(uiIconResourceType, uiIconSVGKey, uiIconColoring, 16)
        {
        }

        public UIMetadataDefinitionsAttribute(Type uiIconResourceType, string uiIconSVGKey, string uiIconColoring, int uiIconSize)
        {
            UIIconDefinition = new UIIconDefinition(uiIconResourceType, uiIconSVGKey, uiIconColoring, uiIconSize);
        }

        public UIIconDefinition UIIconDefinition { get; }

        public bool TryCreateIcon(out SKImage image)
            => UIIconDefinition.TryCreateSKImage(out image);
    }
}
