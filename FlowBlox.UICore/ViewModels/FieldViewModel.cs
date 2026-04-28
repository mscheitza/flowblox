using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Events;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.FieldView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class FieldViewModel : INotifyPropertyChanged, IDisposable
    {
        private const int DefaultFieldViewMaxDisplayLength = 4000;
        private const string FieldViewMaxDisplayLengthOptionName = "FieldView.MaxDisplayLength";
        private const string FieldViewShowFlowBlockOptionName = "FieldView.ShowFlowBlock";
        private const string FieldViewSingleLineFieldValuesOptionName = "FieldView.SingleLineFieldValues";

        private readonly IFlowBloxProjectComponentProvider _componentProvider;
        private readonly IFlowBloxMessageBoxService _messageBoxService;
        private readonly SynchronizationContext? _uiContext;

        private readonly Dictionary<FieldElement, FieldEntryViewModel> _rowsByField = new();
        private readonly List<FieldEntryViewModel> _allRows = new();
        private readonly HashSet<IFlowBloxUIElement> _registeredUiElements = new();

        private readonly List<FieldEntryViewModel> _selectedRows = new();

        private FlowBloxRegistry? _registry;
        private IFlowBloxUIRegistry? _uiRegistry;

        private string _filterText = string.Empty;
        private bool _showFlowBlock;
        private bool _isSingleLineFieldValues;
        private int _maxDisplayLength = DefaultFieldViewMaxDisplayLength;

        public ObservableCollection<FieldEntryViewModel> Fields { get; } = new();

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CopyCommand { get; }
        public RelayCommand OpenFieldValueCommand { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText == value)
                    return;

                _filterText = value ?? string.Empty;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public bool ShowFlowBlock
        {
            get => _showFlowBlock;
            set
            {
                if (_showFlowBlock == value)
                    return;

                _showFlowBlock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FlowBlockColumnWidth));
                SaveFieldViewBooleanOption(FieldViewShowFlowBlockOptionName, value);
            }
        }

        public bool IsSingleLineFieldValues
        {
            get => _isSingleLineFieldValues;
            set
            {
                if (_isSingleLineFieldValues == value)
                    return;

                _isSingleLineFieldValues = value;
                OnPropertyChanged();
                UpdateFieldValuePresentation();
                SaveFieldViewBooleanOption(FieldViewSingleLineFieldValuesOptionName, value);
            }
        }

        public double FlowBlockColumnWidth => ShowFlowBlock ? 180d : 0d;

        public bool HasFields => _allRows.Count > 0;

        public FieldViewModel()
        {
            _uiContext = SynchronizationContext.Current;
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<IFlowBloxProjectComponentProvider>();
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
            _showFlowBlock = ResolveFieldViewShowFlowBlock();
            _isSingleLineFieldValues = ResolveFieldViewSingleLineFieldValues();

            RefreshCommand = new RelayCommand(Refresh);
            CopyCommand = new RelayCommand(CopySelection, () => _selectedRows.Count > 0);
            OpenFieldValueCommand = new RelayCommand(OpenFieldValue, () => _selectedRows.Count == 1);

            RebindAndRefresh();
        }

        public void OnAfterUIRegistryInitialized() => RebindAndRefresh();

        public void UpdateSelection(IEnumerable<FieldEntryViewModel> selectedRows)
        {
            _selectedRows.Clear();
            if (selectedRows != null)
                _selectedRows.AddRange(selectedRows.Where(x => x?.FieldElement != null));

            CopyCommand.Invalidate();
            OpenFieldValueCommand.Invalidate();
        }

        private void Refresh()
        {
            ReloadFields();
            UpdateFlowBlockSelectionState();
        }

        private void RebindAndRefresh()
        {
            UnsubscribeAll();

            _registry = FlowBloxRegistryProvider.GetRegistry();
            if (_registry != null)
            {
                _registry.OnManagedObjectAdded += Registry_OnManagedObjectAdded;
                _registry.OnManagedObjectRemoved += Registry_OnManagedObjectRemoved;
            }

            _uiRegistry = _componentProvider.GetCurrentUIRegistry();
            if (_uiRegistry != null)
            {
                _uiRegistry.UIElementRegistered += UiRegistry_UIElementRegistered;
                foreach (var uiElement in _uiRegistry.UIElements)
                {
                    RegisterElementSelectionEvent(uiElement);
                }
            }

            ReloadFields();
            UpdateFlowBlockSelectionState();
        }

        private void ReloadFields()
        {
            _maxDisplayLength = ResolveFieldViewMaxDisplayLength();

            foreach (var field in _rowsByField.Keys.ToList())
            {
                UnsubscribeFieldEvents(field);
            }

            _rowsByField.Clear();
            _allRows.Clear();

            if (_registry != null)
            {
                var fields = new List<FieldElement>();
                fields.AddRange(_registry.GetUserFields());
                fields.AddRange(_registry.GetRuntimeFields(true));

                foreach (var field in fields)
                {
                    AddFieldRow(field);
                }
            }

            ApplyFilter();
        }

        private void Registry_OnManagedObjectAdded(ManagedObjectAddedEventArgs eventArgs)
        {
            if (eventArgs?.AddedObject is not FieldElement fieldElement)
                return;

            PostToUi(() =>
            {
                AddFieldRow(fieldElement);
                ApplyFilter();
                UpdateFlowBlockSelectionState();
            });
        }

        private void Registry_OnManagedObjectRemoved(ManagedObjectRemovedEventArgs eventArgs)
        {
            if (eventArgs?.RemovedObject is not FieldElement fieldElement)
                return;

            PostToUi(() =>
            {
                RemoveFieldRow(fieldElement);
                ApplyFilter();
            });
        }

        private void UiRegistry_UIElementRegistered(object? sender, FlowBloxUIElementRegisteredEventArgs e)
        {
            RegisterElementSelectionEvent(e?.UIElement);
            PostToUi(UpdateFlowBlockSelectionState);
        }

        private void RegisterElementSelectionEvent(IFlowBloxUIElement? uiElement)
        {
            if (uiElement == null || !_registeredUiElements.Add(uiElement))
                return;

            uiElement.ElementSelectedChangedByUser -= UiElement_ElementSelectedChangedByUser;
            uiElement.ElementSelectedChangedByUser += UiElement_ElementSelectedChangedByUser;
        }

        private void UiElement_ElementSelectedChangedByUser(object? sender, EventArgs e)
            => PostToUi(UpdateFlowBlockSelectionState);

        private void AddFieldRow(FieldElement fieldElement)
        {
            if (fieldElement == null || _rowsByField.ContainsKey(fieldElement))
                return;

            SubscribeFieldEvents(fieldElement);

            var row = new FieldEntryViewModel(fieldElement);
            row.SetSingleLineFieldValues(IsSingleLineFieldValues);
            row.SetMaxDisplayLength(_maxDisplayLength);
            _rowsByField[fieldElement] = row;
            _allRows.Add(row);

            if (fieldElement.IsRegularField() && _uiRegistry != null)
            {
                var uiElement = _uiRegistry.GetUIElementToGridElement(fieldElement.Source);
                RegisterElementSelectionEvent(uiElement);
            }
        }

        private void RemoveFieldRow(FieldElement fieldElement)
        {
            if (fieldElement == null)
                return;

            UnsubscribeFieldEvents(fieldElement);

            if (_rowsByField.TryGetValue(fieldElement, out var row))
            {
                _rowsByField.Remove(fieldElement);
                _allRows.Remove(row);
            }
        }

        private void SubscribeFieldEvents(FieldElement fieldElement)
        {
            fieldElement.OnNameChanged -= FieldElement_OnNameChanged;
            fieldElement.OnNameChanged += FieldElement_OnNameChanged;

            fieldElement.OnValueChanged -= FieldElement_OnValueChanged;
            fieldElement.OnValueChanged += FieldElement_OnValueChanged;

            fieldElement.PropertyChanged -= FieldElement_PropertyChanged;
            fieldElement.PropertyChanged += FieldElement_PropertyChanged;
        }

        private void UnsubscribeFieldEvents(FieldElement fieldElement)
        {
            fieldElement.OnNameChanged -= FieldElement_OnNameChanged;
            fieldElement.OnValueChanged -= FieldElement_OnValueChanged;
            fieldElement.PropertyChanged -= FieldElement_PropertyChanged;
        }

        private void FieldElement_OnNameChanged(FieldElement field, string oldName, string newName)
        {
            PostToUi(() =>
            {
                if (_rowsByField.TryGetValue(field, out var row))
                {
                    row.UpdateName();
                    ApplyFilter();
                }
            });
        }

        private void FieldElement_OnValueChanged(FieldElement field, string oldValue, string newValue)
        {
            PostToUi(() =>
            {
                if (_rowsByField.TryGetValue(field, out var row))
                {
                    row.UpdateValue(newValue, field.Pending);
                }
            });
        }

        private void FieldElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FieldElement field)
                return;

            if (e.PropertyName != nameof(FieldElement.IsPassword))
                return;

            PostToUi(() =>
            {
                if (_rowsByField.TryGetValue(field, out var row))
                    row.UpdateValue(field.StringValue, field.Pending);
            });
        }

        private void UpdateFlowBlockSelectionState()
        {
            HashSet<FieldElement> selectedFields = new();

            if (_uiRegistry != null)
            {
                selectedFields = _uiRegistry.UIElements
                    .Where(x => x.ElementSelected)
                    .SelectMany(x => (x.InternalFlowBlock as BaseResultFlowBlock)?.Fields ?? Enumerable.Empty<FieldElement>())
                    .ToHashSet();
            }

            foreach (var row in _allRows)
            {
                bool isSelectedByFlowBlock = selectedFields.Contains(row.FieldElement);
                row.IsFlowBlockSelected = isSelectedByFlowBlock;
                row.IsAutoSelected = isSelectedByFlowBlock;
            }
        }

        private void UpdateFieldValuePresentation()
        {
            foreach (var row in _allRows)
            {
                row.SetSingleLineFieldValues(IsSingleLineFieldValues);
            }
        }

        private void ApplyFilter()
        {
            var filter = FilterText?.Trim() ?? string.Empty;

            IEnumerable<FieldEntryViewModel> filtered = _allRows;
            if (!string.IsNullOrEmpty(filter))
            {
                filtered = filtered.Where(x =>
                    x.FullyQualifiedName.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }

            var snapshot = filtered.ToList();

            Fields.Clear();
            foreach (var row in snapshot)
            {
                Fields.Add(row);
            }

            OnPropertyChanged(nameof(HasFields));
            CopyCommand.Invalidate();
            OpenFieldValueCommand.Invalidate();
        }

        private void CopySelection()
        {
            if (_selectedRows.Count == 0)
                return;

            try
            {
                var builder = new StringBuilder();
                foreach (var row in _selectedRows)
                {
                    builder.AppendLine(row.FieldElement?.StringValue ?? string.Empty);
                }

                Clipboard.SetText(builder.ToString());
            }
            catch (Exception ex)
            {
                _messageBoxService?.ShowMessageBox(
                    string.Format(FlowBlox.UICore.Resources.FieldView.Message_CopyFailed_Description, ex.Message),
                    FlowBlox.UICore.Resources.FieldView.Message_CopyFailed_Title,
                    FlowBloxMessageBoxTypes.Error);
            }
        }

        private void OpenFieldValue()
        {
            var selectedRow = _selectedRows.Count == 1 ? _selectedRows[0] : null;
            if (selectedRow?.FieldElement == null)
                return;

            FlowBloxEditingHelper.OpenUsingEditor(
                selectedRow.FieldElement.StringValue,
                selectedRow.FieldElement.FullyQualifiedName);
        }

        private int ResolveFieldViewMaxDisplayLength()
        {
            try
            {
                var option = FlowBloxOptions.GetOptionInstance().GetOption(FieldViewMaxDisplayLengthOptionName);
                if (option?.Type == OptionElement.OptionType.Integer)
                {
                    var value = option.GetValueInt();
                    if (value > 0)
                        return value;
                }
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Error("Failed to resolve FieldView max display length option.", e);
            }

            return DefaultFieldViewMaxDisplayLength;
        }

        private bool ResolveFieldViewShowFlowBlock()
            => ResolveFieldViewBooleanOption(FieldViewShowFlowBlockOptionName, defaultValue: false);

        private bool ResolveFieldViewSingleLineFieldValues()
            => ResolveFieldViewBooleanOption(FieldViewSingleLineFieldValuesOptionName, defaultValue: false);

        private static bool ResolveFieldViewBooleanOption(string optionName, bool defaultValue)
        {
            var option = FlowBloxOptions.GetOptionInstance().GetOption(optionName);
            if (option?.Type == OptionElement.OptionType.Boolean && bool.TryParse(option.Value, out var parsed))
                return parsed;

            return defaultValue;
        }

        private static void SaveFieldViewBooleanOption(string optionName, bool value)
        {
            var options = FlowBloxOptions.GetOptionInstance();
            var option = options.GetOption(optionName);
            if (option?.Type != OptionElement.OptionType.Boolean)
                return;

            option.Value = value.ToString().ToLowerInvariant();
            _ = Task.Run(() =>
            {
                try
                {
                    options.Save();
                }
                catch (Exception e)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error($"Failed to persist FieldView option '{optionName}'.", e);
                }
            });
        }

        private void PostToUi(Action action)
        {
            if (action == null)
                return;

            if (_uiContext != null && _uiContext != SynchronizationContext.Current)
            {
                _uiContext.Post(_ => action(), null);
                return;
            }

            action();
        }

        private void UnsubscribeAll()
        {
            if (_registry != null)
            {
                _registry.OnManagedObjectAdded -= Registry_OnManagedObjectAdded;
                _registry.OnManagedObjectRemoved -= Registry_OnManagedObjectRemoved;
            }

            if (_uiRegistry != null)
            {
                _uiRegistry.UIElementRegistered -= UiRegistry_UIElementRegistered;
            }

            foreach (var uiElement in _registeredUiElements.ToList())
            {
                uiElement.ElementSelectedChangedByUser -= UiElement_ElementSelectedChangedByUser;
            }

            _registeredUiElements.Clear();
        }

        public void Dispose()
        {
            UnsubscribeAll();

            foreach (var fieldElement in _rowsByField.Keys.ToList())
            {
                UnsubscribeFieldEvents(fieldElement);
            }

            _rowsByField.Clear();
            _allRows.Clear();
            Fields.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
