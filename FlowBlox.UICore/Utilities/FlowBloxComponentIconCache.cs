using FlowBlox.Core.Models.Base;
using SkiaSharp;
using System.Collections.Concurrent;

namespace FlowBlox.UICore.Utilities
{
    public static class FlowBloxComponentIconCache
    {
        private static readonly ConcurrentDictionary<string, byte[]> _iconPngByTypeKey = new(StringComparer.Ordinal);

        public static byte[] GetOrCreateIcon16Png(FlowBloxReactiveObject component)
        {
            if (component == null)
                return Array.Empty<byte>();

            return GetOrCreateIcon16Png(component.GetType(), () => component);
        }

        public static byte[] GetOrCreateIcon16Png(Type type, Func<FlowBloxReactiveObject> componentFactory)
        {
            if (type == null)
                return Array.Empty<byte>();

            var typeKey = GetTypeKey(type);
            if (string.IsNullOrWhiteSpace(typeKey))
                return Array.Empty<byte>();

            return _iconPngByTypeKey.GetOrAdd(typeKey, _ =>
            {
                var component = componentFactory?.Invoke();
                var icon = component?.Icon16;
                if (icon == null)
                    return Array.Empty<byte>();

                using var data = icon.Encode(SKEncodedImageFormat.Png, 100);
                return data?.ToArray() ?? Array.Empty<byte>();
            });
        }

        public static void RemoveByTypes(IEnumerable<Type> types)
        {
            if (types == null)
                return;

            foreach (var type in types)
            {
                var typeKey = GetTypeKey(type);
                if (!string.IsNullOrWhiteSpace(typeKey))
                    _iconPngByTypeKey.TryRemove(typeKey, out _);
            }
        }

        public static void Clear()
        {
            _iconPngByTypeKey.Clear();
        }

        private static string GetTypeKey(Type type)
            => type?.AssemblyQualifiedName ?? type?.FullName ?? string.Empty;
    }
}
