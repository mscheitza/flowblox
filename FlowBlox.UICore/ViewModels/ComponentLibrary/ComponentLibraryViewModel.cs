using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using MahApps.Metro.IconPacks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Windows.Media;

namespace FlowBlox.UICore.ViewModels.ComponentLibrary
{
    public sealed class ComponentLibraryViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ImageSource _categoryIcon = WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.Folder, 16, new SolidColorBrush(Color.FromRgb(30, 136, 229)));
        private FlowBloxProject _project;
        private string _filterText;
        private bool _isRuntimeActive;

        public ObservableCollection<ComponentLibraryNodeViewModel> Nodes { get; } = new();
        public RelayCommand ManageExtensionsCommand { get; }
        public event EventHandler ManageExtensionsRequested;

        public ComponentLibraryViewModel()
        {
            ManageExtensionsCommand = new RelayCommand(() => ManageExtensionsRequested?.Invoke(this, EventArgs.Empty), () => _project != null);
            InitializeProject();
            RefreshLibrary();
            FlowBloxProjectManager.Instance.ProjectChanged += OnProjectChanged;
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
                RefreshLibrary();
            }
        }

        public bool IsLibraryEnabled => _project != null && !IsRuntimeActive;

        public bool IsRuntimeActive
        {
            get => _isRuntimeActive;
            private set
            {
                if (_isRuntimeActive == value)
                    return;

                _isRuntimeActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLibraryEnabled));
            }
        }

        public void UpdateRuntimeState(bool isRuntimeActive)
        {
            IsRuntimeActive = isRuntimeActive;
            ManageExtensionsCommand.Invalidate();
        }

        private static bool TypeMatchesFilter(Type type, string filter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (string.IsNullOrWhiteSpace(filter))
                return true;

            if (type.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute == null)
                return false;

            var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false);
            return !string.IsNullOrEmpty(displayName) &&
                displayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void InitializeProject()
        {
            _project = FlowBloxProjectManager.Instance.ActiveProject;

            if (_project != null)
            {
                _project.ExtensionsReloaded += OnExtensionsReloaded;
                _project.BeforeUnloadExtension += OnBeforeUnloadExtension;
            }

            OnPropertyChanged(nameof(IsLibraryEnabled));
            ManageExtensionsCommand.Invalidate();
        }

        private void OnProjectChanged(object sender, ProjectChangedEventArgs eventArgs)
        {
            DetachProject();
            _project = eventArgs.NewProject;

            if (_project != null)
            {
                _project.ExtensionsReloaded += OnExtensionsReloaded;
                _project.BeforeUnloadExtension += OnBeforeUnloadExtension;
            }

            RefreshLibrary();
            OnPropertyChanged(nameof(IsLibraryEnabled));
            ManageExtensionsCommand.Invalidate();
        }

        private void OnExtensionsReloaded(object sender, EventArgs e)
        {
            RefreshLibrary();
        }

        private void OnBeforeUnloadExtension(object sender, AssemblyLoadContext loadContext)
        {
            var flowBlockTypes = loadContext.Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BaseFlowBlock).IsAssignableFrom(t))
                .ToHashSet();

            RemoveFlowBlockTypes(Nodes, flowBlockTypes);
        }

        private static void RemoveFlowBlockTypes(ObservableCollection<ComponentLibraryNodeViewModel> nodes, HashSet<Type> flowBlockTypes)
        {
            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];

                if (node.FlowBlock != null && flowBlockTypes.Contains(node.FlowBlock.GetType()))
                {
                    nodes.RemoveAt(i);
                    continue;
                }

                RemoveFlowBlockTypes(node.Children, flowBlockTypes);

                if (!node.IsFlowBlock && node.Children.Count == 0)
                    nodes.RemoveAt(i);
            }
        }

        public void RefreshLibrary()
        {
            Nodes.Clear();

            if (_project == null)
                return;

            var flowBlocksByCategory = _project.CreateInstances<BaseFlowBlock>(type => TypeMatchesFilter(type, FilterText))
                .GroupBy(fb => fb.GetCategory())
                .ToDictionary(g => g.Key, g => g.ToList());

            var rootCategories = FlowBlockCategory.GetAll()
                .Where(c => c.ParentCategory == null)
                .OrderBy(c => c.DisplayName);

            foreach (var rootCategory in rootCategories)
            {
                var categoryNode = BuildCategoryNodeRecursive(rootCategory, flowBlocksByCategory);
                if (categoryNode != null)
                    Nodes.Add(categoryNode);
            }
        }

        private ComponentLibraryNodeViewModel BuildCategoryNodeRecursive(FlowBlockCategory category, Dictionary<FlowBlockCategory, List<BaseFlowBlock>> flowBlocksByCategory)
        {
            var categoryNode = new ComponentLibraryNodeViewModel
            {
                DisplayName = category.DisplayName,
                Category = category,
                Icon = _categoryIcon
            };

            var childCategories = FlowBlockCategory.GetAll()
                .Where(c => c.ParentCategory == category)
                .OrderBy(c => c.DisplayName);

            var hasFlowBlocks = flowBlocksByCategory.TryGetValue(category, out var blocks);
            if (!childCategories.Any() && !hasFlowBlocks)
                return null;

            foreach (var childCategory in childCategories)
            {
                var childNode = BuildCategoryNodeRecursive(childCategory, flowBlocksByCategory);
                if (childNode != null)
                    categoryNode.Children.Add(childNode);
            }

            if (hasFlowBlocks)
            {
                foreach (var block in blocks.OrderBy(FlowBloxComponentHelper.GetDisplayName))
                    categoryNode.Children.Add(CreateFlowBlockNode(block));
            }

            return categoryNode.Children.Count == 0 ? null : categoryNode;
        }

        private static ComponentLibraryNodeViewModel CreateFlowBlockNode(BaseFlowBlock flowBlock)
        {
            return new ComponentLibraryNodeViewModel
            {
                DisplayName = FlowBloxComponentHelper.GetDisplayName(flowBlock),
                FlowBlock = flowBlock,
                Icon = flowBlock.Icon16 != null
                    ? SkiaWpfImageHelper.ConvertToImageSource(flowBlock.Icon16)
                    : WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.CubeOutline, 16)
            };
        }

        private void DetachProject()
        {
            if (_project == null)
                return;

            _project.ExtensionsReloaded -= OnExtensionsReloaded;
            _project.BeforeUnloadExtension -= OnBeforeUnloadExtension;
        }

        public void Dispose()
        {
            FlowBloxProjectManager.Instance.ProjectChanged -= OnProjectChanged;
            DetachProject();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
