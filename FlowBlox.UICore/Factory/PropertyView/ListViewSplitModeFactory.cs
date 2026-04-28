using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.ViewModels.PropertyView;
using MahApps.Metro.IconPacks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class ListViewSplitModeFactory : ListViewFactory
    {
        private ListView _listView;
        private Views.PropertyView _propertyView;
        private PropertyViewModel _propertyViewModel;
        private System.Windows.Controls.Grid _mainGrid;
        private TextBlock _noSelectionText;
        private Button _saveButton;
        private bool _enableSaveForNextSelection;

        public ListViewSplitModeFactory(Window window, PropertyInfo property, object target, bool readOnly, object parent = null)
            : base(window, property, target, readOnly, parent)
        {
            window.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_propertyViewModel != null)
                _propertyViewModel.Cancel(false);
        }

        protected override FrameworkElement CreateFrameworkElement(StackPanel stackPanel, ListView listView)
        {
            _listView = listView;
            listView.SelectionChanged += ListView_SelectionChanged;
            listView.HorizontalAlignment = HorizontalAlignment.Stretch;
            listView.VerticalAlignment = VerticalAlignment.Stretch;

            // In split mode, avoid StackPanel sizing-to-content behavior on the left side.
            // Rehost toolbar + list container in a docked grid (Auto + *) so the list fills available space.
            var leftPanel = BuildDockedLeftPanel(stackPanel);

            // Prepare PropertyView
            _propertyViewModel = new PropertyViewModel(_window);
            _propertyViewModel.PropertyChanged += PropertyViewModel_PropertyChanged;
            _propertyView = new Views.PropertyView
            {
                DataContext = _propertyViewModel
            };

            // NoSelectionText
            _noSelectionText = new TextBlock
            {
                Text = FlowBloxResourceUtil.GetLocalizedString("ListViewSplitModeFactory_NoElementSelected_Text"),
                FontSize = 16,
                FontWeight = FontWeights.UltraLight,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Visibility = Visibility.Visible
            };

            // Create Save button (toolbar style)
            var saveButton = CreateSaveToolbarButton();
            _saveButton = saveButton;
            saveButton.Click += async (s, e) =>
            {
                if (_window is MahApps.Metro.Controls.MetroWindow metro)
                {
                    if (await _propertyViewModel.SaveAsync(metro) == true)
                    {
                        _propertyViewModel.Open(_listView.SelectedItem, _target, deepCopy: true, readOnly: false);
                        _propertyViewModel.IsDirty = false;
                        UpdateSaveButtonState();
                    }
                }
            };

            // Create a 2-row Grid for toolbar and PropertyView
            var rightGrid = new System.Windows.Controls.Grid()
            {
                VerticalAlignment = VerticalAlignment.Stretch
            };
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Toolbar row
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // PropertyView row

            var rightToolBar = new ToolBar
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };
            rightToolBar.Items.Add(saveButton);
            var rightToolbarHost = CreateEmbeddedToolbarHost(rightToolBar, compact: true);
            System.Windows.Controls.Grid.SetRow(rightToolbarHost, 0);
            rightGrid.Children.Add(rightToolbarHost);

            // Add PropertyView (Row 1)
            System.Windows.Controls.Grid.SetRow(_propertyView, 1);
            System.Windows.Controls.Grid.SetRow(_noSelectionText, 1);
            rightGrid.Children.Add(_propertyView);
            rightGrid.Children.Add(_noSelectionText);

            // Now put everything in the main horizontal Grid (2 Columns)
            _mainGrid = new System.Windows.Controls.Grid();
            _mainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            _mainGrid.VerticalAlignment = VerticalAlignment.Stretch;
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // List
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Property

            System.Windows.Controls.Grid.SetColumn(leftPanel, 0);
            System.Windows.Controls.Grid.SetColumn(rightGrid, 1);

            _mainGrid.Children.Add(leftPanel);
            _mainGrid.Children.Add(rightGrid);

            return _mainGrid;
        }

        private static System.Windows.Controls.Grid BuildDockedLeftPanel(StackPanel stackPanel)
        {
            var leftGrid = new System.Windows.Controls.Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            if (stackPanel.Children.Count > 0)
            {
                var toolbar = stackPanel.Children[0] as UIElement;
                if (toolbar != null)
                {
                    stackPanel.Children.RemoveAt(0);
                    var toolbarHost = CreateEmbeddedToolbarHost(toolbar);
                    System.Windows.Controls.Grid.SetRow(toolbarHost, 0);
                    leftGrid.Children.Add(toolbarHost);
                }
            }

            if (stackPanel.Children.Count > 0)
            {
                var listContainer = stackPanel.Children[0] as UIElement;
                if (listContainer != null)
                {
                    stackPanel.Children.RemoveAt(0);
                    if (listContainer is System.Windows.Controls.Grid containerGrid
                        && containerGrid.RowDefinitions.Count > 0)
                    {
                        containerGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                        containerGrid.VerticalAlignment = VerticalAlignment.Stretch;
                        containerGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }

                    System.Windows.Controls.Grid.SetRow(listContainer, 1);
                    leftGrid.Children.Add(listContainer);
                }
            }

            return leftGrid;
        }

        private static Border CreateEmbeddedToolbarHost(UIElement toolbar, bool compact = false)
        {
            var verticalPadding = compact ? 3 : 4;
            return new Border
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#DDE7F3"),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 1, 0, 1),
                Padding = new Thickness(6, verticalPadding, 6, verticalPadding),
                Child = toolbar
            };
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listView.SelectedItem != null)
            {
                _noSelectionText.Visibility = Visibility.Collapsed;
                _propertyView.Visibility = Visibility.Visible;
                _propertyViewModel.Open(_listView.SelectedItem, _target, deepCopy: true, readOnly: false);
                _propertyViewModel.IsDirty = _enableSaveForNextSelection;
                _enableSaveForNextSelection = false;
                UpdateSaveButtonState();
            }
            else if (_propertyViewModel != null)
            {
                _propertyViewModel.Cancel(keepComponent: true);
                _enableSaveForNextSelection = false;
                _propertyView.Visibility = Visibility.Collapsed;
                _noSelectionText.Visibility = Visibility.Visible;
                if (_saveButton != null)
                    _saveButton.IsEnabled = false;
            }
        }

        private void PropertyViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyViewModel.IsDirty))
            {
                UpdateSaveButtonState();
            }
        }

        private void UpdateSaveButtonState()
        {
            if (_saveButton == null)
                return;

            var hasSelection = _listView?.SelectedItem != null;
            if (!hasSelection)
            {
                _saveButton.IsEnabled = false;
                return;
            }

            _saveButton.IsEnabled = _propertyViewModel?.IsDirty == true;
        }

        protected override void ExecuteCreate()
        {
            var newInstance = CreateNewInstance(_window, _listItemType);
            if (newInstance != null)
            {
                _list.Add(newInstance);
                _property.SetValue(_target, _list);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);

                _enableSaveForNextSelection = true;
                if (_listView != null)
                    _listView.SelectedItem = newInstance;

                UpdateSaveButtonState();
            }
        }

        protected override void ExecuteEdit(object item)
        {
            if (item != null)
                _propertyViewModel.Open(item, _target, deepCopy: true, readOnly: false);
        }

        private static Button CreateSaveToolbarButton()
        {
            var button = new Button
            {
                Margin = new Thickness(3, 0, 3, 0),
                Padding = new Thickness(6, 3, 6, 3),
                MinWidth = 80,
                IsEnabled = false,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new PackIconMaterial
                        {
                            Kind = PackIconMaterialKind.ContentSave,
                            Width = 14,
                            Height = 14,
                            Margin = new Thickness(0, 0, 5, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = FlowBloxResourceUtil.GetLocalizedString("Buttons_Apply"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            };

            if (Application.Current?.TryFindResource(ToolBar.ButtonStyleKey) is Style toolbarButtonStyle)
                button.Style = toolbarButtonStyle;

            return button;
        }
    }
}
