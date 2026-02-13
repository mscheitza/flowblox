using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Provider;
using FlowBlox.UICore.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.ViewModels.FieldSelection;
using FlowBlox.Core.Util;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class FieldSelectionWindowViewModel : INotifyPropertyChanged
    {
        private readonly Window _ownerWindow;
        private readonly FlowBloxRegistry _registry;
        private readonly FieldSelectionWindowArgs _args;

        private bool _isRequired;
        private int _selectedTabIndex;

        public RelayCommand OkCommand { get; }
        public RelayCommand BackCommand { get; }

        public FieldSelectionMode SelectionMode
        {
            get => _args.SelectionMode;
            set
            {
                if (_args.SelectionMode == value) return;
                _args.SelectionMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFieldsMode));
                OnPropertyChanged(nameof(IsOptionsMode));
                OnPropertyChanged(nameof(ShowRequired));
            }
        }

        public bool MultiSelect => _args.MultiSelect;

        public bool IsRequired
        {
            get => _isRequired;
            set
            {
                if (_isRequired == value) return;
                _isRequired = value;
                OnPropertyChanged();
            }
        }

        public bool HideRequired => _args.HideRequired;
        public bool IsFieldsMode => SelectionMode == FieldSelectionMode.Fields;
        public bool IsOptionsMode => SelectionMode == FieldSelectionMode.Options;

        public bool ShowRequired => IsFieldsMode && !HideRequired;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value) return;
                _selectedTabIndex = value;
                OnPropertyChanged();

                SelectionMode = _selectedTabIndex == 0 ? 
                    FieldSelectionMode.Fields : 
                    FieldSelectionMode.Options;
            }
        }

        public bool CanSelectFields =>
            _args.AllowedFieldSelectionModes != null &&
            _args.AllowedFieldSelectionModes.Contains(FieldSelectionMode.Fields);

        public bool CanSelectOptions =>
            _args.AllowedFieldSelectionModes != null &&
            _args.AllowedFieldSelectionModes.Contains(FieldSelectionMode.Options);

        public List<FieldRowViewModel> FieldRows { get; private set; } = new List<FieldRowViewModel>();
        public List<OptionRowViewModel> OptionRows { get; private set; } = new List<OptionRowViewModel>();

        public FieldSelectionWindowViewModel(Window ownerWindow, FieldSelectionWindowArgs args)
        {
            _ownerWindow = ownerWindow;
            _args = args ?? new FieldSelectionWindowArgs();
            _registry = FlowBloxRegistryProvider.GetRegistry();

            // Initialize state.
            IsRequired = _args.IsRequired;
            SelectedTabIndex = _args.SelectionMode == FieldSelectionMode.Fields ? 0 : 1;

            if (SelectedTabIndex == 0 && !CanSelectFields && CanSelectOptions)
                SelectedTabIndex = 1;
            else if (SelectedTabIndex == 1 && !CanSelectOptions && CanSelectFields)
                SelectedTabIndex = 0;

            OkCommand = new RelayCommand(() =>
            {
                _ownerWindow.DialogResult = true;
                _ownerWindow.Close();
            });

            BackCommand = new RelayCommand(() =>
            {
                _ownerWindow.DialogResult = false;
                _ownerWindow.Close();
            });

            LoadRows();
        }

        private void LoadRows()
        {
            LoadFieldRows();
            LoadOptionRows();
        }

        private void LoadFieldRows()
        {
            IEnumerable<FieldElement> fieldElements = _args.FieldElements;

            var flowBlock = _args.FlowBlock;

            // If no field elements were passed, load them from the registry result flow blocks.
            if (fieldElements == null)
            {
                IEnumerable<BaseResultFlowBlock> resultElements = _registry.GetResultFlowBlocks();

                if (flowBlock != null)
                    resultElements = resultElements.Except(new[] { flowBlock }).Cast<BaseResultFlowBlock>();

                fieldElements = resultElements.SelectMany(x => x.Fields);
            }

            // Append user fields (registry) and ensure distinct.
            fieldElements = (fieldElements ?? Enumerable.Empty<FieldElement>())
                .Concat(_registry.GetUserFields() ?? Enumerable.Empty<FieldElement>())
                .Distinct();

            // Determine connection state.
            bool IsConnected(FieldElement fe)
                => flowBlock?.ReferencedFlowBlocks?.Contains(fe.Source) == true;

            // Sorting requirement:
            // - Connected fields first
            // - Then disconnected
            // - User fields at the end
            // Within each bucket, use a stable alphabetical ordering for better UX.
            var sorted = fieldElements
                .Select(fe => new
                {
                    Element = fe,
                    Connected = IsConnected(fe),
                    User = fe?.UserField == true
                })
                .OrderBy(x => x.User ? 2 : (x.Connected ? 0 : 1))
                .ThenBy(x => x.Element?.Source?.Name ?? "")
                .ThenBy(x => x.Element?.Name ?? "")
                .Select(x => new FieldRowViewModel(x.Element, x.Connected))
                .ToList();

            FieldRows = sorted;
            OnPropertyChanged(nameof(FieldRows));
        }

        private void LoadOptionRows()
        {
            IEnumerable<OptionElement> optionElements = _args.OptionElements;

            // If no option elements were passed, load them from FlowBlockOptions.
            if (optionElements == null)
                optionElements = FlowBloxOptions.GetOptionInstance()
                    .GetOptions()
                    .Where(o => o.IsPlaceholderEnabled) ?? Enumerable.Empty<OptionElement>();

            // Sorting requirement: alphabetical by option name.
            OptionRows = (optionElements ?? Enumerable.Empty<OptionElement>())
                .OrderBy(o => o?.Name ?? "")
                .Select(o => new OptionRowViewModel(o))
                .ToList();

            OnPropertyChanged(nameof(OptionRows));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}