using FlowBlox.Core.Models.Components;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnusedRequiredFieldsResources = FlowBlox.UICore.Resources.UnusedRequiredFieldsWindow;

namespace FlowBlox.UICore.Views
{
    public partial class UnusedRequiredFieldsWindow : MetroWindow
    {
        private bool _isUpdatingSelectionState;

        public ObservableCollection<UnusedRequiredFieldRow> FieldRows { get; } = new();

        public UnusedRequiredFieldsWindow(IEnumerable<FieldElement> unusedFieldElements)
        {
            InitializeComponent();
            DataContext = this;

            if (unusedFieldElements != null)
            {
                foreach (var fieldElement in unusedFieldElements
                    .OrderBy(x => x.Source?.Name)
                    .ThenBy(x => x.Name))
                {
                    var row = new UnusedRequiredFieldRow(fieldElement);
                    row.PropertyChanged += Row_PropertyChanged;
                    FieldRows.Add(row);
                }
            }

            RefreshSelectionState();
        }

        public IReadOnlyList<FieldElement> GetSelectedFieldElements()
        {
            return FieldRows
                .Where(x => x.IsSelected)
                .Select(x => x.FieldElement)
                .ToList();
        }

        private void SelectAllButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetAllSelections(true);
        }

        private void DeselectAllButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetAllSelections(false);
        }

        private void SelectAllCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_isUpdatingSelectionState)
                return;

            SetAllSelections(true);
        }

        private void SelectAllCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_isUpdatingSelectionState)
                return;

            SetAllSelections(false);
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ContinueButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UnusedRequiredFieldRow.IsSelected))
                RefreshSelectionState();
        }

        private void SetAllSelections(bool isSelected)
        {
            _isUpdatingSelectionState = true;

            foreach (var row in FieldRows)
                row.IsSelected = isSelected;

            _isUpdatingSelectionState = false;
            RefreshSelectionState();
        }

        private void RefreshSelectionState()
        {
            _isUpdatingSelectionState = true;

            var selectedCount = FieldRows.Count(x => x.IsSelected);
            var totalCount = FieldRows.Count;

            SelectAllButton.IsEnabled = totalCount > 0 && selectedCount < totalCount;
            DeselectAllButton.IsEnabled = selectedCount > 0;

            ContinueButtonText.Text = selectedCount > 0
                ? UnusedRequiredFieldsResources.Button_RemoveAndContinue
                : UnusedRequiredFieldsResources.Button_Continue;

            _isUpdatingSelectionState = false;
        }

        public class UnusedRequiredFieldRow : INotifyPropertyChanged
        {
            private bool _isSelected;

            public UnusedRequiredFieldRow(FieldElement fieldElement)
            {
                FieldElement = fieldElement ?? throw new ArgumentNullException(nameof(fieldElement));
                _isSelected = false;
            }

            public FieldElement FieldElement { get; }

            public string FieldName => FieldElement.Name;

            public string FullyQualifiedName => FieldElement.FullyQualifiedName;

            public string FieldType => FieldElement.FieldType?.FieldType.ToString() ?? string.Empty;

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value)
                        return;

                    _isSelected = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
