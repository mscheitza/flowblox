using FlowBlox.Core.Attributes;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.PropertyView.Converters;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.IconPacks;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class DataGridFactory : ListFactoryBase
    {
        private readonly FlowBlockDataGridAttribute _dataGridAttribute;

        private RelayCommand _addCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _editCommand;
        private RelayCommand _moveUpCommand;
        private RelayCommand _moveDownCommand;

        public DataGridFactory(Window window, PropertyInfo property, object target, bool readOnly)
            : base(window, property, target, readOnly)
        {
            _dataGridAttribute = _property.GetCustomAttribute<FlowBlockDataGridAttribute>();
        }

        private void SetCellStyle(DataGrid dataGrid)
        {
            var cellStyle = new Style(typeof(DataGridCell));
            var trigger = new Trigger
            {
                Property = Validation.HasErrorProperty,
                Value = true
            };
            trigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Red));
            trigger.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(3)));
            cellStyle.Triggers.Add(trigger);
            dataGrid.CellStyle = cellStyle;
        }

        private IEnumerable<PropertyInfo> GetDataGridProperties(Type listItemType)
        {
            var all = listItemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (_dataGridAttribute?.GridColumnMemberNames != null &&
                _dataGridAttribute.GridColumnMemberNames.Length > 0)
            {
                var map = all.ToDictionary(p => p.Name, p => p);

                foreach (var name in _dataGridAttribute.GridColumnMemberNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (map.TryGetValue(name, out var prop))
                        yield return prop;
                }

                yield break;
            }

            foreach (var p in all)
                yield return p;
        }

        private bool IsPropertyReadOnly(PropertyInfo propertyInfo, FlowBlockUIAttribute flowBlockUIAttribute)
        {
            return _readOnly ||
                   !propertyInfo.CanWrite ||
                   flowBlockUIAttribute?.ReadOnly == true;
        }

        public FrameworkElement Create()
        {
            // Property-Wert auslesen und Liste überprüfen
            var propertyValue = _property.GetValue(_target);
            if (!(propertyValue is IList list))
                throw new InvalidOperationException("DataGridFactory can only work with IList-based properties.");

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = _flowBlockUIAttribute?.ReadOnly ?? false,
                CanUserAddRows = false,
                CanUserDeleteRows = _flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Delete) ?? false,
                CanUserSortColumns = _dataGridAttribute?.IsMovable != true,
                SelectionMode = DataGridSelectionMode.Extended,
                ColumnWidth = DataGridLength.Auto
            };
            SetCellStyle(dataGrid);

            var emptyMessage = CreateEmptyMessage(list);

            // Binding an die Liste setzen
            dataGrid.ItemsSource = list;

            // Merken, ob eine FieldSelection aktiviert wurde
            bool enableFieldSelectionFound = false;

            // Merken, welche Column welches FlowBlockUIAttribute hat
            Dictionary<DataGridColumn, FlowBlockUIAttribute> columnAttributeMap = new();

            // Spalten aus der Listenelement-Type erstellen
            Type listItemType = _property.PropertyType.GetGenericArguments()[0];
            foreach (var childProperty in GetDataGridProperties(listItemType))
            {
                var displayAttr = childProperty.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr == null)
                    continue;

                var flowBlockUIAttribute = childProperty.GetCustomAttribute<FlowBlockUIAttribute>();

                // Wenn die Grid-Columms automatisch ermittelt wurden, werden nicht sichtbare Member nicht als Spalten angezeigt:
                if (_dataGridAttribute?.GridColumnMemberNames == null ||
                    _dataGridAttribute.GridColumnMemberNames.Length == 0)
                {
                    if (flowBlockUIAttribute?.Visible == false)
                        continue;
                }

                string headerText = FlowBloxResourceUtil.GetDisplayName(displayAttr);

                // Standard-Datentypen (inklusive Nullable<int>, Nullable<long>, etc.)
                var underlyingType = Nullable.GetUnderlyingType(childProperty.PropertyType);
                var propertyType = underlyingType ?? childProperty.PropertyType;
                var textAttribute = childProperty.GetCustomAttribute<FlowBlockTextBoxAttribute>();

                if (propertyType == typeof(string) &&
                    textAttribute?.Suggestions == true &&
                    !string.IsNullOrWhiteSpace(textAttribute.SuggestionMember))
                {
                    var suggestionItems = ResolveSuggestions(_target, textAttribute.SuggestionMember);

                    var comboStyle = new Style(typeof(ComboBox));
                    comboStyle.Setters.Add(new Setter(ComboBox.IsEditableProperty, true));
                    comboStyle.Setters.Add(new Setter(ComboBox.IsTextSearchEnabledProperty, true));

                    var column = new DataGridComboBoxColumn
                    {
                        Header = headerText,
                        ItemsSource = suggestionItems,
                        SelectedItemBinding = new Binding(childProperty.Name)
                        {
                            Mode = IsPropertyReadOnly(childProperty, flowBlockUIAttribute) ? BindingMode.OneWay : BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            TargetNullValue = string.Empty
                        },
                        IsReadOnly = IsPropertyReadOnly(childProperty, flowBlockUIAttribute),
                        EditingElementStyle = comboStyle
                    };

                    dataGrid.Columns.Add(column);
                    continue;
                }
                 
                if (propertyType == typeof(int) ||
                    propertyType == typeof(long) ||
                    propertyType == typeof(float) ||
                    propertyType == typeof(double) ||
                    propertyType == typeof(string))
                {
                    if (flowBlockUIAttribute?.UiOptions.HasFlag(UIOptions.EnableFieldSelection) == true)
                    {
                        enableFieldSelectionFound = true;
                    }

                    var column = new DataGridTextColumn
                    {
                        Header = headerText,
                        Binding = new Binding(childProperty.Name)
                        {
                            Mode = IsPropertyReadOnly(childProperty, flowBlockUIAttribute) ? BindingMode.OneWay : BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            TargetNullValue = string.Empty
                        },
                        IsReadOnly = IsPropertyReadOnly(childProperty, flowBlockUIAttribute)
                    };
                    ApplyCenteredTextColumnStyles(column);
                    dataGrid.Columns.Add(column);
                    columnAttributeMap[column] = flowBlockUIAttribute;
                }
                else if (propertyType.IsEnum)
                {
                    var itemSource = Enum.GetValues(propertyType)
                        .Cast<Enum>()
                        .Select(e => new { Display = e.GetDisplayName(), Value = e })
                        .ToList();

                    if (underlyingType != null)
                        itemSource.Insert(0, new { Display = string.Empty, Value = (Enum)null });

                    var column = new DataGridComboBoxColumn
                    {
                        Header = headerText,
                        ItemsSource = itemSource,
                        SelectedValuePath = "Value",
                        SelectedValueBinding = new Binding(childProperty.Name)
                        {
                            Mode = BindingMode.TwoWay
                        },
                        DisplayMemberPath = "Display"
                    };
                    dataGrid.Columns.Add(column);
                }
                // Selection-Filter
                else if (flowBlockUIAttribute?.Factory == UIFactory.ComboBox)
                {
                    var filterMethod = !string.IsNullOrEmpty(flowBlockUIAttribute?.SelectionFilterMethod) ?
                        _target.GetType().GetMethod(flowBlockUIAttribute?.SelectionFilterMethod) :
                        null;

                    if (filterMethod != null)
                    {
                        var originalItems = filterMethod.Invoke(_target, null) as IList;

                        IList items = null;
                        bool isRequired = childProperty.GetCustomAttribute<RequiredAttribute>() != null;

                        if (originalItems != null && !isRequired)
                        {
                            items = (IList)Activator.CreateInstance(originalItems.GetType());
                            items!.Add(null);

                            foreach (var item in originalItems)
                                items.Add(item);
                        }
                        else
                        {
                            items = originalItems;
                        }

                        var column = new DataGridComboBoxColumn
                        {
                            Header = headerText,
                            ItemsSource = items,
                            SelectedValueBinding = new Binding(childProperty.Name) { 
                                Mode = BindingMode.TwoWay 
                            },
                            DisplayMemberPath = flowBlockUIAttribute.SelectionDisplayMember
                        };
                        dataGrid.Columns.Add(column);
                    }
                    else
                    {
                        var fallbackColumn = new DataGridTextColumn
                        {
                            Header = headerText,
                            Binding = new Binding(childProperty.Name)
                            {
                                Mode = BindingMode.OneWay
                            },
                            IsReadOnly = true
                        };
                        ApplyCenteredTextColumnStyles(fallbackColumn);
                        dataGrid.Columns.Add(fallbackColumn);
                    }
                }

                // Association (Create/Edit Button)
                else if (flowBlockUIAttribute?.Factory == UIFactory.Association)
                {
                    var buttonColumn = new DataGridTemplateColumn
                    {
                        Header = headerText,
                        CellTemplate = new DataTemplate
                        {
                            VisualTree = CreateAssociationButtonTemplate(childProperty)
                        },
                        IsReadOnly = _readOnly
                    };
                    dataGrid.Columns.Add(buttonColumn);
                }

                // Checkbox
                else if (propertyType == typeof(bool))
                {
                    var column = new DataGridCheckBoxColumn
                    {
                        Header = headerText,
                        Binding = new Binding(childProperty.Name)
                        {
                            Mode = IsPropertyReadOnly(childProperty, flowBlockUIAttribute) ? BindingMode.OneWay : BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        },
                        IsReadOnly = IsPropertyReadOnly(childProperty, flowBlockUIAttribute)
                    };
                    dataGrid.Columns.Add(column);
                    columnAttributeMap[column] = flowBlockUIAttribute;
                }
            }

            // Commands für die Buttons
            _addCommand = new RelayCommand(() =>
            {
                AddNewItem(list, listItemType, dataGrid);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
            }, () => !_readOnly);

            _deleteCommand = new RelayCommand(async () =>
            {
                foreach (var item in dataGrid.SelectedItems.Cast<object>().ToList())
                {
                    if (await IsDeletableAsync(item, _window))
                    {
                        list.Remove(item);
                        FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                    }
                }
            }, () => dataGrid.SelectedItems.Count > 0 && !_readOnly);

            _editCommand = new RelayCommand(() =>
            {
                var item = dataGrid.SelectedItem;
                if (item != null)
                {
                    var success = WpfPropertyWindowProvider.CreatePropertyWindowAndShowDialog(_window, _target, item, _readOnly);
                    if (success)
                        FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                }
            }, () => dataGrid.SelectedItems.Count == 1 && !_readOnly);

            _moveUpCommand = new RelayCommand(() =>
            {
                MoveItem(list, dataGrid, -1);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
            }, () => dataGrid.SelectedIndex > 0 && !_readOnly);

            _moveDownCommand = new RelayCommand(() =>
            {
                MoveItem(list, dataGrid, 1);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
            }, () => dataGrid.SelectedIndex >= 0 && dataGrid.SelectedIndex < list.Count - 1 && !_readOnly);

            // Toolbar for Create, Delete, MoveUp, MoveDown
            var toolBar = new ToolBar();

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Create) == true)
            {
                var addButton = CreateButton(PackIconMaterialKind.Plus, FlowBloxResourceUtil.GetLocalizedString("Buttons_Add"), _addCommand, Brushes.Green);
                toolBar.Items.Add(addButton);
            }

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Edit) == true)
            {
                var editButton = CreateButton(PackIconMaterialKind.Pencil, FlowBloxResourceUtil.GetLocalizedString("Buttons_Edit"), _editCommand, Brushes.Orange);
                toolBar.Items.Add(editButton);
            }

            if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Delete) == true)
            {
                var removeButton = CreateButton(PackIconMaterialKind.Delete, FlowBloxResourceUtil.GetLocalizedString("Buttons_Remove"), _deleteCommand, Brushes.DarkRed);
                toolBar.Items.Add(removeButton);
            }

            if (_dataGridAttribute?.IsMovable == true)
            {
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.ArrowUp, FlowBloxResourceUtil.GetLocalizedString("Buttons_MoveUp"), _moveUpCommand, Brushes.Blue));
                toolBar.Items.Add(CreateButton(PackIconMaterialKind.ArrowDown, FlowBloxResourceUtil.GetLocalizedString("Buttons_MoveDown"), _moveDownCommand, Brushes.Blue));
            }

            if (enableFieldSelectionFound)
            {
                toolBar.Items.Add(new TextBlock
                {
                    Text = "Tipp: Mit Strg+F können Sie Feldnamen einfügen",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            var container = new System.Windows.Controls.Grid();
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // DataGrid
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // EmptyMessage
            container.Children.Add(emptyMessage);
            container.Children.Add(dataGrid);

            System.Windows.Controls.Grid.SetRow(dataGrid, 0);
            System.Windows.Controls.Grid.SetRow(emptyMessage, 1);

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(toolBar);
            stackPanel.Children.Add(container);

            // Event to activate/deactivate the buttons
            dataGrid.SelectionChanged += async (s, e) =>
            {
                _deleteCommand.Invalidate();
                _editCommand.Invalidate();
                _moveUpCommand.Invalidate();
                _moveDownCommand.Invalidate();
            };

            // Event for field selection
            dataGrid.PreparingCellForEdit += (s, e) =>
            {
                if (e.EditingElement is TextBox textBox && e.Column is DataGridTextColumn textColumn)
                {
                    if (columnAttributeMap.TryGetValue(textColumn, out var attrib) &&
                        attrib?.UiOptions.HasFlag(UIOptions.EnableFieldSelection) == true)
                    {
                        textBox.Tag = attrib;

                        textBox.PreviewKeyDown -= OnPreviewKeyDownForFieldSelection;
                        textBox.PreviewKeyDown += OnPreviewKeyDownForFieldSelection;
                    }
                }
            };

            // Fire Property-Change Event when the cell value has changed
            dataGrid.CellEditEnding += (s, e) =>
            {
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
            };

            UpdateEmptyMessageVisibilityOnCollectionChanged(emptyMessage, list);

            return ResizableControlContainer.Create(stackPanel, minHeight: 150);
        }

        private void ApplyCenteredTextColumnStyles(DataGridTextColumn column)
        {
            var elementStyle = new Style(typeof(TextBlock));
            elementStyle.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
            column.ElementStyle = elementStyle;
        }

        private static IEnumerable<string> ResolveSuggestions(object target, string suggestionMember)
        {
            var method = target?.GetType().GetMethod(suggestionMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null || method.GetParameters().Length != 0)
                return Array.Empty<string>();

            var result = method.Invoke(target, null);
            if (result is not IEnumerable enumerable)
                return Array.Empty<string>();

            return enumerable
                .Cast<object>()
                .Select(x => x?.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void OnPreviewKeyDownForFieldSelection(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (sender is TextBox textBox && textBox.Tag is FlowBlockUIAttribute attrib)
                {
                    var textBoxAdapter = new WpfTextBoxAdapter(textBox);
                    TextBoxHelper.ShowFieldSelectionDialog(_target, attrib, textBoxAdapter, _window);
                    e.Handled = true;
                }
            }
        }

        private Button CreateButton(PackIconMaterialKind iconKind, string tooltip, RelayCommand command, Brush color)
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
                Margin = new Thickness(5)
            };
            ButtonStyleHelper.SetButtonStyle(button, color, Brushes.Gray);
            return button;
        }

        private FrameworkElementFactory CreateAssociationButtonTemplate(PropertyInfo property)
        {
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, new Binding(property.Name)
            {
                Converter = new AssociationConverter()
            });
            factory.AddHandler(Button.ClickEvent, new RoutedEventHandler((s, e) =>
            {
                if (s is Button button && button.DataContext != null)
                {
                    var dataContext = button.DataContext;

                    var item = property.GetValue(dataContext, null);
                    if (item == null)
                    {
                        var newInstance = CreateNewInstance(_window, property.PropertyType);
                        if (newInstance == null)
                            return;

                        property.SetValue(dataContext, newInstance);
                        FlowBloxComponentHelper.RaisePropertyChanged(dataContext, property.Name);
                        item = newInstance;
                    }
                    var propertyView = new Views.PropertyWindow(new PropertyWindowArgs(item, readOnly: false));
                    propertyView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    propertyView.Owner = _window;
                    if (propertyView.ShowDialog() == true)
                    {
                        FlowBloxComponentHelper.RaisePropertyChanged(dataContext, property.Name);
                        FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                    }
                }
            }));
            return factory;
        }

        private void AddNewItem(IList list, Type listItemType, DataGrid dataGrid)
        {
            object newItem = CreateNewInstance(_window, listItemType);
            if (newItem != null)
                list.Add(newItem);
        }

        private void MoveItem(IList list, DataGrid dataGrid, int direction)
        {
            int index = dataGrid.SelectedIndex;
            if (index < 0 || (index == 0 && direction == -1) || (index == list.Count - 1 && direction == 1)) return;
            var item = list[index];
            list.RemoveAt(index);
            list.Insert(index + direction, item);
            dataGrid.SelectedIndex = index + direction;
        }
    }
}
