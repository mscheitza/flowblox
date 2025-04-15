using FlowBlox.Core.Attributes;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Converters;
using FlowBlox.UICore.Factory.PropertyView;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class ListViewFactory : ListFactoryBase
    {
        protected readonly FlowBlockListViewAttribute _listViewAttribute;
        protected IList _list;
        protected Type _listItemType;
        private IDialogService _dialogService;
        protected FrameworkElement _emptyMessage;

        public ListViewFactory(Window window, PropertyInfo property, object target, bool readOnly)
            : base(window, property, target, readOnly)
        {
            _listViewAttribute = property.GetCustomAttribute<FlowBlockListViewAttribute>()
                ?? throw new InvalidOperationException("Missing FlowBlockListViewAttribute.");

            if (!(_property.GetValue(_target) is IList list))
                throw new InvalidOperationException("Property is not IList.");

            _list = list;
            _listItemType = _property.PropertyType.GetGenericArguments()[0];
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
        }

        public FrameworkElement Create()
        {
            var listView = new ListView
            {
                Margin = new Thickness(0, 5, 0, 10)
            };

            var gridView = new GridView();
            listView.View = gridView;

            var listViewProperties = _listItemType
                .GetProperties()
                .Where(x => _listViewAttribute.LVColumnMemberNames.Contains(x.Name));

            foreach (var prop in listViewProperties)
            {
                var header = prop.GetCustomAttribute<DisplayAttribute>() is DisplayAttribute displayAttr
                    ? FlowBloxResourceUtil.GetDisplayName(displayAttr)
                    : prop.Name;

                var template = new DataTemplate();

                if (prop.PropertyType == typeof(bool))
                {
                    var factory = new FrameworkElementFactory(typeof(PackIconMaterial));
                    factory.SetBinding(PackIconMaterial.KindProperty, new Binding(prop.Name)
                    {
                        Converter = new BooleanToPackIconKindConverter()
                    });
                    factory.SetValue(PackIconMaterial.WidthProperty, 14.0);
                    factory.SetValue(PackIconMaterial.HeightProperty, 14.0);
                    factory.SetValue(PackIconMaterial.ForegroundProperty, Brushes.Gray);

                    template.VisualTree = factory;
                }
                else
                {
                    var factory = new FrameworkElementFactory(typeof(TextBlock));
                    factory.SetValue(TextBlock.MinWidthProperty, 150.0);
                    factory.SetBinding(TextBlock.TextProperty, new Binding(prop.Name));
                    template.VisualTree = factory;
                }

                gridView.Columns.Add(new GridViewColumn
                {
                    Header = header,
                    CellTemplate = template
                });
            }

            listView.ItemsSource = _list;

            // Set selection to preselectedInstance (if available)
            if (_preselectedInstance != null && _list.Contains(_preselectedInstance))
            {
                listView.SelectedItem = _preselectedInstance;
                listView.Dispatcher.InvokeAsync(() =>
                {
                    listView.ScrollIntoView(_preselectedInstance);
                }, DispatcherPriority.ApplicationIdle);
            }

            _emptyMessage = CreateEmptyMessage(_list);

            var stackPanel = new StackPanel();
            var toolBar = new ToolBar();

            var container = new System.Windows.Controls.Grid();
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.Children.Add(_emptyMessage);
            container.Children.Add(listView);

            System.Windows.Controls.Grid.SetRow(listView, 0);
            System.Windows.Controls.Grid.SetRow(_emptyMessage, 1);

            stackPanel.Children.Add(toolBar);
            stackPanel.Children.Add(container);

            // RelayCommands mit Enable-Logik
            var addCommand = new RelayCommand(ExecuteCreate, () => !_readOnly);
            var editCommand = new RelayCommand(() => ExecuteEdit(listView.SelectedItem), () => listView.SelectedItem != null);
            var deleteCommand = new RelayCommand(() => ExecuteDelete(listView.SelectedItem), () => listView.SelectedItem != null && !_readOnly);
            var unlinkCommand = new RelayCommand(() => ExecuteUnlink(listView.SelectedItem), () => listView.SelectedItem != null && !_readOnly);
            var linkCommand = new RelayCommand(async () => await ExecuteLink(), () => !_readOnly);

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Create) == true)
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.Plus, FlowBloxResourceUtil.GetLocalizedString("Buttons_Add"), addCommand, Brushes.Green));

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Link) == true)
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.Link, FlowBloxResourceUtil.GetLocalizedString("Buttons_Link"), linkCommand, Brushes.Blue));

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Edit) == true)
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.Pencil, FlowBloxResourceUtil.GetLocalizedString("Buttons_Edit"), editCommand, Brushes.Orange));

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Delete) == true)
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.Delete, FlowBloxResourceUtil.GetLocalizedString("Buttons_Remove"), deleteCommand, Brushes.DarkRed));

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Unlink) == true)
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.LinkOff, FlowBloxResourceUtil.GetLocalizedString("Buttons_Unlink"), unlinkCommand, Brushes.DarkRed));

            var moveUpCommand = new RelayCommand(() => MoveItem(listView, -1), () => listView.SelectedIndex > 0 && !_readOnly);
            var moveDownCommand = new RelayCommand(() => MoveItem(listView, 1), () => listView.SelectedIndex >= 0 && listView.SelectedIndex < _list.Count - 1 && !_readOnly);

            if (_listViewAttribute?.IsMovable == true)
            {
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.ArrowUp, FlowBloxResourceUtil.GetLocalizedString("Buttons_MoveUp"), moveUpCommand, Brushes.Blue));
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.ArrowDown, FlowBloxResourceUtil.GetLocalizedString("Buttons_MoveDown"), moveDownCommand, Brushes.Blue));
            }

            // Refresh Commands bei Selektion
            listView.SelectionChanged += (s, e) =>
            {
                editCommand.Invalidate();
                deleteCommand.Invalidate();
                unlinkCommand.Invalidate();
            };

            var frameworkElement = CreateFrameworkElement(stackPanel, listView);
            return ResizableControlContainer.Create(frameworkElement);
        }

        protected virtual FrameworkElement CreateFrameworkElement(StackPanel stackPanel, ListView listView)
        {
            return stackPanel;
        }

        private Button CreateButton(PackIconMaterialKind iconKind, string tooltip, RelayCommand command, Brush activeColor)
        {
            var button = new Button
            {
                ToolTip = tooltip,
                Command = command,
                Content = new PackIconMaterial
                {
                    Kind = iconKind,
                    Width = 12,
                    Height = 12
                },
                Width = 24,
                Height = 24,
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Set style with active and disabled color
            ButtonStyleHelper.SetButtonStyle(button, activeColor, Brushes.Gray);

            return button;
        }

        protected override object CreateNewInstance(Type type)
        {
            if (_listViewAttribute.LVItemFactory != null)
            {
                var factoryType = _listViewAttribute.LVItemFactory;
                var factoryInstance = (IItemFactory<IFlowBloxComponent>)Activator.CreateInstance(factoryType);
                var result = factoryInstance.Create();

                if (result == null || !type.IsInstanceOfType(result))
                    throw new InvalidOperationException($"Factory returned an incompatible instance. Expected type: {type.FullName}, actual: {result?.GetType().FullName ?? "null"}");

                return result;
            }
            else
            {
                return base.CreateNewInstance(type);
            }
        }

        protected virtual void ExecuteCreate()
        {
            var newInstance = CreateNewInstance(_window, _listItemType);
            if (newInstance != null)
            {
                var success = WpfPropertyWindowProvider.CreatePropertyWindowAndShowDialog(_window, _target, newInstance, _readOnly);
                if (success)
                {
                    _list.Add(newInstance);
                    _property.SetValue(_target, _list);
                    FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                    UpdateEmptyMessageVisibility(_emptyMessage, _list);
                }
            }
        }

        private async Task ExecuteLink()
        {
            if (string.IsNullOrEmpty(_flowBlockUIAttribute?.SelectionFilterMethod))
            {
                await MessageBoxHelper.ShowMessageBoxAsync(
                    (MetroWindow)_window,
                    FlowBloxResourceUtil.GetLocalizedString("Global_MissingFilterMethod_Title"),
                    string.Format(FlowBloxResourceUtil.GetLocalizedString("Global_MissingFilterMethod_Message"), _property.Name));
                return;
            }

            var filterMethod = GetSelectionFilterMethod(_target, _flowBlockUIAttribute.SelectionFilterMethod);
            if (filterMethod == null) 
                return;

            var items = filterMethod.Invoke(_target, null) as IList;
            if (items == null || items.Count == 0)
            {
                await MessageBoxHelper.ShowMessageBoxAsync(
                    (MetroWindow)_window,
                    FlowBloxResourceUtil.GetLocalizedString("Global_NoItemsFound_Title"),
                    FlowBloxResourceUtil.GetLocalizedString("Global_NoItemsFound_Message"));
                return;
            }

            if (_listItemType == typeof(FieldElement))
            {
                var fieldSelectionResult = _dialogService.InvokeFieldSelection(_target, _flowBlockUIAttribute, items, _window);
                if (fieldSelectionResult.Success)
                {
                    FlowBlockHelper.ApplyFieldSelectionRequiredOption((BaseFlowBlock)_target, fieldSelectionResult.SelectedFields, fieldSelectionResult.IsRequired);
                    var inst = fieldSelectionResult.SelectedFields.Single();
                    _list.Add(inst);
                    _property.SetValue(_target, _list);
                    FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                    UpdateEmptyMessageVisibility(_emptyMessage, _list);
                }
            }
            else
            {
                var dialog = new MultiValueSelectionDialog(
                   FlowBloxResourceUtil.GetLocalizedString("Global_SelectionDialog_Title"),
                   FlowBloxResourceUtil.GetLocalizedString("Global_SelectionDialog_Text"),
                   new GenericSelectionHandler<object>(items.Cast<object>(), x => x.ToString()))
                    {
                        Owner = _window,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                if (dialog.ShowDialog() == true)
                {
                    _list.Add(dialog.SelectedItem.Value);
                    _property.SetValue(_target, _list);
                    FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                    UpdateEmptyMessageVisibility(_emptyMessage, _list);
                }
            }
        }

        protected virtual void ExecuteEdit(object item)
        {
            if (item != null)
            {
                var success = WpfPropertyWindowProvider.CreatePropertyWindowAndShowDialog(_window, _target, item, _readOnly);
                if (success)
                    FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
            }
        }

        private async void ExecuteDelete(object item)
        {
            if (item != null && await IsDeletableAsync(item, _window))
            {
                _list.Remove(item);
                _property.SetValue(_target, _list);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                DeleteInstance(item);
                UpdateEmptyMessageVisibility(_emptyMessage, _list);
            }
        }

        private void ExecuteUnlink(object item)
        {
            if (item != null)
            {
                _list.Remove(item);
                _property.SetValue(_target, _list);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                UpdateEmptyMessageVisibility(_emptyMessage, _list);
            }
        }

        private MethodInfo GetSelectionFilterMethod(object target, string methodName)
        {
            return target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void MoveItem(ListView listView, int direction)
        {
            int index = listView.SelectedIndex;
            if (index < 0 || (index == 0 && direction == -1) || (index == _list.Count - 1 && direction == 1)) 
                return;

            var item = _list[index];
            _list.RemoveAt(index);
            _list.Insert(index + direction, item);

            listView.SelectedIndex = index + direction;
            FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
        }
    }
}
