using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Resources;
using MahApps.Metro.IconPacks;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public abstract class ListFactoryBase : WpfPropertyViewControlFactory
    {
        public ListFactoryBase(Window window, PropertyInfo property, object target, bool readOnly)
            : base(window, property, target, readOnly)
        {

        }

        protected object _preselectedInstance;

        public void SetPreselectedInstance(object instance)
        {
            _preselectedInstance = instance;
        }

        protected object ResolvePreselectedInstanceInCurrentTransaction(IList currentList)
        {
            if (_preselectedInstance == null || currentList == null || currentList.Count == 0)
            {
                return null;
            }

            if (_target is not FlowBloxComponent flowBloxComponent)
            {
                return currentList.Contains(_preselectedInstance) ? _preselectedInstance : null;
            }

            var originalTarget = _registry.GetOriginalRef(flowBloxComponent) as FlowBloxComponent;
            var originalList = originalTarget != null ? _property.GetValue(originalTarget) as IList : null;
            var originalIndex = originalList?.IndexOf(_preselectedInstance) ?? -1;

            if (originalIndex >= 0 && originalIndex < currentList.Count)
            {
                return currentList[originalIndex];
            }

            return currentList.Contains(_preselectedInstance) ? _preselectedInstance : null;
        }

        protected void UpdateEmptyMessageVisibilityOnCollectionChanged(FrameworkElement emptyMessage, IList list)
        {
            if (list is INotifyCollectionChanged incc)
            {
                var weakEmptyMessage = new WeakReference<FrameworkElement>(emptyMessage);
                var weakList = new WeakReference<IList>(list);
                NotifyCollectionChangedEventHandler handler = (s, e) =>
                {
                    if (!weakEmptyMessage.TryGetTarget(out var emptyMessage) ||
                        !weakList.TryGetTarget(out var list))
                    {
                        return;
                    }
                    UpdateEmptyMessageVisibility(emptyMessage, list);
                };
                incc.CollectionChanged += handler;
            }
        }

        protected void UpdateEmptyMessageVisibility(FrameworkElement frameworkElement, IList list)
        {
            frameworkElement.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected FrameworkElement CreateEmptyMessage(IList list)
        {
            var emptyMessage = new System.Windows.Controls.Grid
            {
                Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed,
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            emptyMessage.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            emptyMessage.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var icon = new PackIconMaterial
            {
                Kind = PackIconMaterialKind.AlertCircleOutline,
                Width = 16,
                Height = 16,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center
            };

            System.Windows.Controls.Grid.SetColumn(icon, 0);

            var text = new TextBlock
            {
                Text = FlowBloxResourceUtil.GetLocalizedString("ListFactoryBase_NoDataAvailable_Text"),
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            System.Windows.Controls.Grid.SetColumn(text, 1);

            emptyMessage.Children.Add(icon);
            emptyMessage.Children.Add(text);

            return emptyMessage;
        }
    }
}
