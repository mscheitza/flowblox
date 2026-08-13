using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Resources;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels.Options
{
    public sealed class OptionsWindowViewModel : INotifyPropertyChanged
    {
        private readonly FlowBloxOptions _options;
        private OptionTreeNodeViewModel _selectedNode;
        private OptionElement _selectedOption;
        private string _originalOptionName;
        private string _filterText;
        private string _name;
        private string _description;
        private string _value;
        private OptionElement.OptionType _selectedType;
        private bool _booleanValue;
        private bool _isPlaceholderEnabled;
        private bool _isDirty;

        public OptionsWindowViewModel()
            : this(null)
        {
        }

        public OptionsWindowViewModel(OptionElement preSelectedOption)
        {
            _options = FlowBloxOptions.GetOptionInstance();
            OptionTypes = Enum.GetValues(typeof(OptionElement.OptionType))
                .Cast<OptionElement.OptionType>()
                .ToList();

            SaveCommand = new RelayCommand(SaveSelectedOption, () => IsOptionSelected && IsDirty);
            DeleteCommand = new RelayCommand(DeleteSelectedOption, () => CanDeleteSelectedOption);
            RebuildTree(preSelectedOption);
        }

        public ObservableCollection<OptionTreeNodeViewModel> OptionNodes { get; } = new();

        public IReadOnlyList<OptionElement.OptionType> OptionTypes { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public event EventHandler<string> ErrorOccurred;

        public OptionTreeNodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (ReferenceEquals(_selectedNode, value))
                    return;

                _selectedNode = value;
                OnPropertyChanged();
                SelectOption(value?.OptionElement);
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText == value)
                    return;

                _filterText = value;
                OnPropertyChanged();
                RebuildTree(_selectedOption);
            }
        }

        public bool IsOptionSelected => _selectedOption != null;
        public bool IsNoOptionSelected => !IsOptionSelected;
        public bool CanEditMetadata => IsOptionSelected && !_selectedOption.SystemOption;
        public bool IsMetadataReadOnly => !CanEditMetadata;
        public bool CanDeleteSelectedOption => IsOptionSelected && !_selectedOption.SystemOption;
        public bool IsBooleanType => IsOptionSelected && SelectedType == OptionElement.OptionType.Boolean;
        public bool IsPasswordType => IsOptionSelected && SelectedType == OptionElement.OptionType.Password;
        public bool IsTextValueType => IsOptionSelected && !IsBooleanType && !IsPasswordType;

        public string Name
        {
            get => _name;
            set => SetDetailValue(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetDetailValue(ref _description, value);
        }

        public string Value
        {
            get => _value;
            set => SetDetailValue(ref _value, value);
        }

        public OptionElement.OptionType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType == value)
                    return;

                _selectedType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBooleanType));
                OnPropertyChanged(nameof(IsPasswordType));
                OnPropertyChanged(nameof(IsTextValueType));
                MarkDirty();
            }
        }

        public bool BooleanValue
        {
            get => _booleanValue;
            set => SetDetailValue(ref _booleanValue, value);
        }

        public bool IsPlaceholderEnabled
        {
            get => _isPlaceholderEnabled;
            set => SetDetailValue(ref _isPlaceholderEnabled, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;
                OnPropertyChanged();
                SaveCommand.Invalidate();
            }
        }

        public string AddOption(string optionName)
        {
            if (string.IsNullOrWhiteSpace(optionName))
                return null;

            optionName = optionName.Trim();
            if (_options.OptionCollection.ContainsKey(optionName))
                return string.Format(OptionsWindow.Validation_OptionAlreadyExists, optionName);

            var option = new OptionElement
            {
                Name = optionName,
                Value = string.Empty,
                Description = string.Empty,
                Type = OptionElement.OptionType.Text,
                IsPlaceholderEnabled = true
            };

            _options.OptionCollection[optionName] = option;
            _options.Save();
            RebuildTree(option);
            return null;
        }

        public void ResetOptions()
        {
            _options.InitDefaults(true);
            _options.Save();
            SelectOption(null);
            RebuildTree(null);
        }

        private void DeleteSelectedOption()
        {
            if (!CanDeleteSelectedOption)
                return;

            _options.OptionCollection.Remove(_selectedOption.Name);
            _options.Save();
            SelectOption(null);
            RebuildTree(null);
        }

        private void SaveSelectedOption()
        {
            try
            {
                SaveSelectedOptionCore();
            }
            catch (Exception exception)
            {
                ErrorOccurred?.Invoke(this, exception.Message);
            }
        }

        private void SaveSelectedOptionCore()
        {
            if (!IsOptionSelected)
                return;

            var newName = Name?.Trim();
            if (string.IsNullOrWhiteSpace(newName))
                throw new ValidationException(OptionsWindow.Validation_OptionNameRequired);

            if (!_selectedOption.SystemOption &&
                !string.Equals(_originalOptionName, newName, StringComparison.OrdinalIgnoreCase) &&
                _options.OptionCollection.ContainsKey(newName))
                throw new ValidationException(string.Format(OptionsWindow.Validation_OptionAlreadyExists, newName));

            _selectedOption.Name = newName;
            _selectedOption.Description = Description ?? string.Empty;
            _selectedOption.Type = SelectedType;
            _selectedOption.IsPlaceholderEnabled = IsPlaceholderEnabled;
            _selectedOption.Value = SelectedType == OptionElement.OptionType.Boolean
                ? BooleanValue.ToString().ToLowerInvariant()
                : Value ?? string.Empty;

            _selectedOption.Validate();

            if (!_selectedOption.SystemOption &&
                !string.Equals(_originalOptionName, _selectedOption.Name, StringComparison.Ordinal))
            {
                _options.OptionCollection.Remove(_originalOptionName);
                _options.OptionCollection[_selectedOption.Name] = _selectedOption;
                _originalOptionName = _selectedOption.Name;
            }

            _options.Save();
            IsDirty = false;
            RebuildTree(_selectedOption);
        }

        private void SelectOption(OptionElement option)
        {
            _selectedOption = option;
            _originalOptionName = option?.Name;

            if (option == null)
            {
                SetDetailSnapshot(null, null, null, OptionElement.OptionType.Text, false, false);
            }
            else
            {
                var optionType = ResolveOptionType(option);
                SetDetailSnapshot(
                    option.Name,
                    option.Description,
                    option.Value,
                    optionType,
                    option.GetValueBoolean(),
                    option.IsPlaceholderEnabled);
            }

            IsDirty = false;
            OnSelectionStateChanged();
        }

        private static OptionElement.OptionType ResolveOptionType(OptionElement option)
        {
            var looksBoolean = string.Equals(option.Value, "true", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(option.Value, "false", StringComparison.OrdinalIgnoreCase);

            return looksBoolean && option.Type != OptionElement.OptionType.Boolean
                ? OptionElement.OptionType.Boolean
                : option.Type;
        }

        private void SetDetailSnapshot(
            string name,
            string description,
            string value,
            OptionElement.OptionType type,
            bool booleanValue,
            bool isPlaceholderEnabled)
        {
            _name = name;
            _description = description;
            _value = value;
            _selectedType = type;
            _booleanValue = booleanValue;
            _isPlaceholderEnabled = isPlaceholderEnabled;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(SelectedType));
            OnPropertyChanged(nameof(BooleanValue));
            OnPropertyChanged(nameof(IsPlaceholderEnabled));
        }

        private void RebuildTree(OptionElement preSelectedOption)
        {
            OptionNodes.Clear();

            var filter = FilterText?.Trim();
            foreach (var option in _options.GetOptions())
            {
                if (!string.IsNullOrWhiteSpace(filter) &&
                    option.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                AddOptionToTree(option, expand: !string.IsNullOrWhiteSpace(filter));
            }

            if (preSelectedOption != null)
                SelectedNode = FindNodeByOption(OptionNodes, preSelectedOption);
        }

        private void AddOptionToTree(OptionElement option, bool expand)
        {
            var currentNodes = OptionNodes;
            OptionTreeNodeViewModel currentNode = null;
            var parts = option.Name.Split('.', StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var isLeaf = i == parts.Length - 1;
                var existing = currentNodes.FirstOrDefault(x => string.Equals(x.DisplayName, part, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    existing = new OptionTreeNodeViewModel(part, isLeaf ? option : null)
                    {
                        IsExpanded = expand
                    };
                    currentNodes.Add(existing);
                }

                if (expand)
                    existing.IsExpanded = true;

                currentNode = existing;
                currentNodes = existing.Children;
            }

            if (currentNode != null && expand)
                currentNode.IsExpanded = true;
        }

        private static OptionTreeNodeViewModel FindNodeByOption(IEnumerable<OptionTreeNodeViewModel> nodes, OptionElement option)
        {
            foreach (var node in nodes)
            {
                if (ReferenceEquals(node.OptionElement, option))
                    return node;

                var child = FindNodeByOption(node.Children, option);
                if (child != null)
                    return child;
            }

            return null;
        }

        private void SetDetailValue<T>(ref T storage, T value)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;

            storage = value;
            OnPropertyChanged();
            MarkDirty();
        }

        private void MarkDirty()
        {
            if (IsOptionSelected)
                IsDirty = true;
        }

        private void OnSelectionStateChanged()
        {
            OnPropertyChanged(nameof(IsOptionSelected));
            OnPropertyChanged(nameof(IsNoOptionSelected));
            OnPropertyChanged(nameof(CanEditMetadata));
            OnPropertyChanged(nameof(IsMetadataReadOnly));
            OnPropertyChanged(nameof(CanDeleteSelectedOption));
            OnPropertyChanged(nameof(IsBooleanType));
            OnPropertyChanged(nameof(IsPasswordType));
            OnPropertyChanged(nameof(IsTextValueType));
            DeleteCommand.Invalidate();
            SaveCommand.Invalidate();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
