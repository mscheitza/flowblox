using FlowBlox.AppWindow.Contents;
using FlowBlox.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow
{
    internal static class DockContentIconResolver
    {
        private static readonly Dictionary<Type, string> _resourceKeysByDockType = new()
        {
            [typeof(ProjectPanel)] = nameof(FlowBloxMainUIImages.ProjectPanel_16),
            [typeof(ComponentLibraryPanel)] = nameof(FlowBloxMainUIImages.ComponentLibraryPanel_16),
            [typeof(FieldView)] = nameof(FlowBloxMainUIImages.FieldView_16),
            [typeof(ManagedObjectsView)] = nameof(FlowBloxMainUIImages.ManagedObjectsView_16),
            [typeof(AIAssistantView)] = nameof(FlowBloxMainUIImages.AIAssistantView_16)
        };

        private static readonly Dictionary<Type, string> _resourceKeysByWrappedViewType = new()
        {
            [typeof(FlowBlox.Views.RuntimeView)] = nameof(FlowBloxMainUIImages.RuntimeView_16),
            [typeof(FlowBlox.Views.ProblemsView)] = nameof(FlowBloxMainUIImages.ProblemsView_16)
        };

        private const string FallbackResourceKey = nameof(FlowBloxMainUIImages.DockContent_16);

        public static Image Resolve(DockContent dockContent)
        {
            if (dockContent == null)
                return null;

            if (_resourceKeysByDockType.TryGetValue(dockContent.GetType(), out var dockTypeResourceKey) &&
                TryResolveResourceImage(dockTypeResourceKey, out var dockTypeImage))
            {
                return dockTypeImage;
            }

            var wrappedViewType = ResolveWrappedViewType(dockContent.GetType());
            if (wrappedViewType != null &&
                _resourceKeysByWrappedViewType.TryGetValue(wrappedViewType, out var wrappedViewResourceKey) &&
                TryResolveResourceImage(wrappedViewResourceKey, out var wrappedViewImage))
            {
                return wrappedViewImage;
            }

            if (TryResolveResourceImage($"{dockContent.GetType().Name}_16", out var dockTypeConventionImage))
                return dockTypeConventionImage;

            if (TryResolveResourceImage($"{dockContent.Name}_16", out var dockNameConventionImage))
                return dockNameConventionImage;

            if (TryResolveResourceImage(FallbackResourceKey, out var fallbackImage))
                return fallbackImage;

            return null;
        }

        private static Type ResolveWrappedViewType(Type dockType)
        {
            if (dockType == null || !dockType.IsGenericType)
                return null;

            if (dockType.GetGenericTypeDefinition() != typeof(DockContentUserControlWrapper<>))
                return null;

            return dockType.GetGenericArguments().FirstOrDefault();
        }

        private static bool TryResolveResourceImage(string resourceKey, out Image image)
        {
            image = null;
            if (string.IsNullOrWhiteSpace(resourceKey))
                return false;

            image = FlowBloxMainUIImages.ResourceManager.GetObject(resourceKey) as Image;
            return image != null;
        }
    }
}

