using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.ViewModels.FieldSelection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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
                OnPropertyChanged(nameof(IsProjectPropertiesMode));
                OnPropertyChanged(nameof(IsOptionsMode));
                OnPropertyChanged(nameof(IsInputFilesMode));
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
        public bool IsProjectPropertiesMode => SelectionMode == FieldSelectionMode.ProjectProperties;
        public bool IsOptionsMode => SelectionMode == FieldSelectionMode.Options;
        public bool IsInputFilesMode => SelectionMode == FieldSelectionMode.InputFiles;

        public bool ShowRequired => IsFieldsMode && !HideRequired;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value) return;
                _selectedTabIndex = value;
                OnPropertyChanged();

                // Tab order: 0 = Fields, 1 = ProjectProperties, 2 = Options, 3 = InputFiles
                SelectionMode =
                    _selectedTabIndex == 0 ? FieldSelectionMode.Fields :
                    _selectedTabIndex == 1 ? FieldSelectionMode.ProjectProperties :
                    _selectedTabIndex == 2 ? FieldSelectionMode.Options :
                    FieldSelectionMode.InputFiles;
            }
        }

        public bool CanSelectFields =>
            _args.AllowedFieldSelectionModes != null &&
            _args.AllowedFieldSelectionModes.Contains(FieldSelectionMode.Fields);

        public bool CanSelectProjectProperties =>
            _args.AllowedFieldSelectionModes != null &&
            _args.AllowedFieldSelectionModes.Contains(FieldSelectionMode.ProjectProperties);

        public bool CanSelectOptions =>
            _args.AllowedFieldSelectionModes != null &&
            _args.AllowedFieldSelectionModes.Contains(FieldSelectionMode.Options);

        public bool CanSelectInputFiles =>
            _args.AllowedFieldSelectionModes != null &&
            _args.AllowedFieldSelectionModes.Contains(FieldSelectionMode.InputFiles);

        public List<FieldRowViewModel> FieldRows { get; private set; } = new List<FieldRowViewModel>();
        public List<ProjectPropertyRowViewModel> ProjectPropertyRows { get; private set; } = new List<ProjectPropertyRowViewModel>();
        public List<OptionRowViewModel> OptionRows { get; private set; } = new List<OptionRowViewModel>();
        public List<InputFileRowViewModel> InputFileRows { get; private set; } = new List<InputFileRowViewModel>();

        public FieldSelectionWindowViewModel(Window ownerWindow, FieldSelectionWindowArgs args)
        {
            _ownerWindow = ownerWindow;
            _args = args ?? new FieldSelectionWindowArgs();
            _registry = FlowBloxRegistryProvider.GetRegistry();

            // Initialize state.
            IsRequired = _args.IsRequired;

            SelectedTabIndex =
                _args.SelectionMode == FieldSelectionMode.Fields ? 0 :
                _args.SelectionMode == FieldSelectionMode.ProjectProperties ? 1 :
                _args.SelectionMode == FieldSelectionMode.Options ? 2 :
                3;

            // Normalize tab selection based on allowed modes.
            NormalizeSelectedTabIndex();

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

        private void NormalizeSelectedTabIndex()
        {
            // Prefer current selection if allowed; otherwise choose the first allowed tab in display order.
            bool tabAllowed =
                (SelectedTabIndex == 0 && CanSelectFields) ||
                (SelectedTabIndex == 1 && CanSelectProjectProperties) ||
                (SelectedTabIndex == 2 && CanSelectOptions) ||
                (SelectedTabIndex == 3 && CanSelectInputFiles);

            if (tabAllowed)
                return;

            if (CanSelectFields) SelectedTabIndex = 0;
            else if (CanSelectProjectProperties) SelectedTabIndex = 1;
            else if (CanSelectOptions) SelectedTabIndex = 2;
            else if (CanSelectInputFiles) SelectedTabIndex = 3;
        }

        private void LoadRows()
        {
            LoadFieldRows();
            LoadProjectPropertyRows();
            LoadOptionRows();
            LoadInputFileRows();
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

            // Sorting:
            // - Connected fields first
            // - Then disconnected
            // - User fields at the end
            // Within each bucket: alphabetical by source and field name.
            FieldRows = fieldElements
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

            OnPropertyChanged(nameof(FieldRows));
        }

        private void LoadProjectPropertyRows()
        {
            IEnumerable<FlowBloxProjectPropertyElement> elements = _args.ProjectPropertyElements;

            if (elements == null)
            {
                var project = FlowBloxProjectManager.Instance.ActiveProject;
                elements = project?.GetProjectPropertyElements() ?? Enumerable.Empty<FlowBloxProjectPropertyElement>();
            }

            // Sorting: alphabetical by display name (fallback to key).
            ProjectPropertyRows = (elements ?? Enumerable.Empty<FlowBloxProjectPropertyElement>())
                .OrderBy(p => p?.DisplayName ?? "")
                .ThenBy(p => p?.Key ?? "")
                .Select(p => new ProjectPropertyRowViewModel(p))
                .ToList();

            OnPropertyChanged(nameof(ProjectPropertyRows));
        }

        private void LoadOptionRows()
        {
            IEnumerable<OptionElement> optionElements = _args.OptionElements;

            // If no option elements were passed, load them from FlowBloxOptions.
            if (optionElements == null)
                optionElements = FlowBloxOptions.GetOptionInstance()
                    .GetOptions()
                    .Where(o => o.IsPlaceholderEnabled) ?? Enumerable.Empty<OptionElement>();

            // Sorting: alphabetical by option name.
            OptionRows = (optionElements ?? Enumerable.Empty<OptionElement>())
                .OrderBy(o => o?.Name ?? "")
                .Select(o => new OptionRowViewModel(o))
                .ToList();

            OnPropertyChanged(nameof(OptionRows));
        }

        private void LoadInputFileRows()
        {
            IEnumerable<FlowBloxInputFilePlaceholderElement> inputFileElements = _args.InputFileElements
                ?? Enumerable.Empty<FlowBloxInputFilePlaceholderElement>();

            InputFileRows = inputFileElements
                .OrderBy(x => x?.DisplayName ?? string.Empty)
                .ThenBy(x => x?.Key ?? string.Empty)
                .Select(x => new InputFileRowViewModel(x))
                .ToList();

            OnPropertyChanged(nameof(InputFileRows));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
