using FlowBlox.AppWindow.Contents;
using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Utilities;
using FlowBlox.Views;
using FlowBlox.Views.PropertyView;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
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
            [typeof(AIAssistantView)] = nameof(FlowBloxMainUIImages.AIAssistantView_16)
        };

        private static readonly Dictionary<Type, string> _resourceKeysByWrappedViewType = new()
        {
            [typeof(FlowBlox.Views.RuntimeView)] = nameof(FlowBloxMainUIImages.RuntimeView_16),
            [typeof(FlowBlox.Views.ProblemsView)] = nameof(FlowBloxMainUIImages.ProblemsView_16)
        };

        private static readonly Dictionary<Type, string> _resourceKeysByObjectManagerType = new()
        {
            [typeof(FlowBlox.Core.Models.ObjectManager.DataObjectManager)] = nameof(FlowBloxMainUIImages.DataObjectManager_16),
            [typeof(FlowBlox.Core.Models.ObjectManager.DataTableManager)] = nameof(FlowBloxMainUIImages.DataTableManager_16)
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

            var objectManagerType = ResolveObjectManagerType(dockContent);
            if (objectManagerType != null)
            {
                if (_resourceKeysByObjectManagerType.TryGetValue(objectManagerType, out var objectManagerResourceKey) &&
                    TryResolveResourceImage(objectManagerResourceKey, out var objectManagerResourceImage))
                {
                    return objectManagerResourceImage;
                }

                if (TryResolveObjectManagerMetadataIcon(objectManagerType, out var objectManagerMetadataImage))
                    return objectManagerMetadataImage;

                if (TryResolveResourceImage($"{objectManagerType.Name}_16", out var objectManagerConventionalImage))
                    return objectManagerConventionalImage;
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

        private static Type ResolveObjectManagerType(Control parentControl)
        {
            if (parentControl == null)
                return null;

            foreach (Control control in parentControl.Controls)
            {
                if (control is PropertyViewTabControl propertyViewTabControl &&
                    propertyViewTabControl.Target != null)
                {
                    return propertyViewTabControl.Target.GetType();
                }

                var childResult = ResolveObjectManagerType(control);
                if (childResult != null)
                    return childResult;
            }

            return null;
        }

        private static bool TryResolveObjectManagerMetadataIcon(Type objectManagerType, out Image image)
        {
            image = null;
            if (objectManagerType == null)
                return false;

            var metadata = objectManagerType.GetCustomAttribute<UIMetadataDefinitionsAttribute>(inherit: false);
            if (metadata == null || !metadata.UIIconDefinition.IsDefined)
                return false;

            try
            {
                if (!metadata.TryCreateIcon(out var skImage))
                    return false;

                image = SkiaToSystemDrawingHelper.ToSystemDrawingImage(skImage);
                return image != null;
            }
            catch
            {
                return false;
            }
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

