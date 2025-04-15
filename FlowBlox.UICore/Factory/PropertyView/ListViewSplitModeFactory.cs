using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.PropertyView;
using FlowBlox.UICore.Views;
using System;
using System.Collections;
using System.Linq;
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

        public ListViewSplitModeFactory(Window window, PropertyInfo property, object target, bool readOnly)
            : base(window, property, target, readOnly)
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

            // Prepare PropertyView
            _propertyViewModel = new PropertyViewModel(_window);
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

            // Create Save button
            var saveButton = new Button
            {
                Content = FlowBloxResourceUtil.GetLocalizedString("Buttons_Apply"),
                Margin = new Thickness(10),
                Padding = new Thickness(10, 5, 10, 5),
                Background = Brushes.LightGreen,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            saveButton.Click += async (s, e) =>
            {
                if (_window is MahApps.Metro.Controls.MetroWindow metro)
                {
                    if (await _propertyViewModel.SaveAsync(metro) == true)
                        _propertyViewModel.Open(_listView.SelectedItem, deepCopy: true, readOnly: false);
                }
            };

            // Create a 2-row Grid for PropertyView and Save button
            var rightGrid = new System.Windows.Controls.Grid()
            {
                VerticalAlignment = VerticalAlignment.Stretch
            };
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // PropertyView 
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Button row

            // Add PropertyView (Row 0)
            System.Windows.Controls.Grid.SetRow(_propertyView, 0);
            System.Windows.Controls.Grid.SetRow(_noSelectionText, 0);
            rightGrid.Children.Add(_propertyView);
            rightGrid.Children.Add(_noSelectionText);

            // Add Button inside a right-aligned container (Row 1)
            var buttonContainer = new DockPanel { LastChildFill = false };
            DockPanel.SetDock(saveButton, Dock.Right);
            buttonContainer.Children.Add(saveButton);

            System.Windows.Controls.Grid.SetRow(buttonContainer, 1);
            rightGrid.Children.Add(buttonContainer);

            // Now put everything in the main horizontal Grid (2 Columns)
            _mainGrid = new System.Windows.Controls.Grid();
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // List
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Property

            System.Windows.Controls.Grid.SetColumn(stackPanel, 0);
            System.Windows.Controls.Grid.SetColumn(rightGrid, 1);

            _mainGrid.Children.Add(stackPanel);
            _mainGrid.Children.Add(rightGrid);

            return _mainGrid;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listView.SelectedItem != null)
            {
                _noSelectionText.Visibility = Visibility.Collapsed;
                _propertyView.Visibility = Visibility.Visible;
                _propertyViewModel.Open(_listView.SelectedItem, deepCopy: true, readOnly: false);
            }
            else if (_propertyViewModel != null)
            {
                _propertyViewModel.Cancel(keepComponent: true);
                _propertyView.Visibility = Visibility.Collapsed;
                _noSelectionText.Visibility = Visibility.Visible;
            }
        }

        protected override void ExecuteCreate()
        {
            var newInstance = CreateNewInstance(_window, _listItemType);
            if (newInstance != null)
            {
                _list.Add(newInstance);
                _property.SetValue(_target, _list);
                FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
                UpdateEmptyMessageVisibility(_emptyMessage, _list);
            }
        }

        protected override void ExecuteEdit(object item)
        {
            if (item != null)
                _propertyViewModel.Open(item, deepCopy: true, readOnly: false);
        }
    }
}