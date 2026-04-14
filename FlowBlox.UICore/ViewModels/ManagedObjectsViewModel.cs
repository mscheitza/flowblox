using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Events;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Provider;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.ManagedObjects;
using FlowBlox.UICore.ViewModels.PropertyWindow;
using FlowBlox.UICore.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using FlowBlox.Core.Provider;
using System.IO;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class ManagedObjectsViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SynchronizationContext? _uiContext;
        private readonly IFlowBloxMessageBoxService _messageBoxService;
        private readonly IDialogService _dialogService;

        private FlowBloxRegistry? _registry;
        private FlowBloxProject? _project;

        public ObservableCollection<ManagedObjectTypeNodeViewModel> TypeNodes { get; } = new();

        public ObservableCollection<ManagedObjectEntryViewModel> ManagedObjects { get; } = new();

        private ManagedObjectTypeNodeViewModel? _selectedTypeNode;
        public ManagedObjectTypeNodeViewModel? SelectedTypeNode
        {
            get => _selectedTypeNode;
            set
            {
                if (ReferenceEquals(_selectedTypeNode, value))
                    return;

                _selectedTypeNode = value;
                OnPropertyChanged();
                RefreshManagedObjects();
                InvalidateCommands();
            }
        }

        private ManagedObjectEntryViewModel? _selectedEntry;
        public ManagedObjectEntryViewModel? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (ReferenceEquals(_selectedEntry, value))
                    return;

                _selectedEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedEntryActions));
                OnPropertyChanged(nameof(HasSelectedEntryActions));
                InvalidateCommands();
            }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set
            {
                if (_isReadOnly == value)
                    return;

                _isReadOnly = value;
                OnPropertyChanged();
                InvalidateCommands();
            }
        }

        public IEnumerable<UIActionViewModel> SelectedEntryActions
            => SelectedEntry?.Actions ?? Enumerable.Empty<UIActionViewModel>();
        public bool HasSelectedEntryActions => SelectedEntry != null && SelectedEntryActions.Any();

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public ManagedObjectsViewModel()
        {
            _uiContext = SynchronizationContext.Current;
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();

            RefreshCommand = new RelayCommand(RefreshAll);
            AddCommand = new RelayCommand(AddManagedObject, CanAddManagedObject);
            EditCommand = new RelayCommand(EditSelectedManagedObject, CanEditManagedObject);
            DeleteCommand = new RelayCommand(DeleteSelectedManagedObject, CanDeleteManagedObject);

            FlowBloxProjectManager.Instance.ProjectChanged += ProjectManager_ProjectChanged;
            RebindAndRefresh();
        }

        public void SetRuntimeActive(bool isRuntimeActive)
        {
            IsReadOnly = isRuntimeActive;
            RebuildTypeTree();
            RefreshManagedObjects();
        }

        public void OnAfterUIRegistryInitialized()
        {
            RebindAndRefresh();
        }

        private void ProjectManager_ProjectChanged(object? sender, ProjectChangedEventArgs e)
        {
            RebindAndRefresh();
        }

        private void RebindAndRefresh()
        {
            Unsubscribe();

            _registry = FlowBloxRegistryProvider.GetRegistry();
            if (_registry != null)
            {
                _registry.OnManagedObjectAdded += Registry_OnManagedObjectAdded;
                _registry.OnManagedObjectRemoved += Registry_OnManagedObjectRemoved;
            }

            _project = FlowBloxProjectManager.Instance.ActiveProject;
            if (_project != null)
            {
                _project.ExtensionsReloaded += Project_ExtensionsReloaded;
                _project.BeforeUnloadExtension += Project_BeforeUnloadExtension;
            }

            RefreshAll();
        }

        private void Project_ExtensionsReloaded(object? sender, EventArgs e)
        {
            PostToUi(() =>
            {
                RebuildTypeTree();
                RefreshManagedObjects();
            });
        }

        private void Project_BeforeUnloadExtension(object? sender, AssemblyLoadContext loadContext)
        {
            var types = GetTypesFromLoadContext(loadContext)
                .Where(t => typeof(FlowBloxReactiveObject).IsAssignableFrom(t))
                .ToList();

            FlowBloxComponentIconCache.RemoveByTypes(types);
        }

        private void Registry_OnManagedObjectAdded(ManagedObjectAddedEventArgs eventArgs)
        {
            if (eventArgs?.AddedObject is FieldElement)
                return;

            PostToUi(() =>
            {
                RebuildTypeTree();
                RefreshManagedObjects();
            });
        }

        private void Registry_OnManagedObjectRemoved(ManagedObjectRemovedEventArgs eventArgs)
        {
            if (eventArgs?.RemovedObject is FieldElement)
                return;

            PostToUi(() =>
            {
                RebuildTypeTree();
                RefreshManagedObjects();
            });
        }

        private void RefreshAll()
        {
            RebuildTypeTree();
            RefreshManagedObjects();
            InvalidateCommands();
        }

        private void RebuildTypeTree()
        {
            var previouslySelectedType = SelectedTypeNode?.ManagedObjectType;

            var concreteManagedObjectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(IsSupportedManagedObjectType)
                .ToList();

            var includedTypes = new HashSet<Type>();
            foreach (var concreteType in concreteManagedObjectTypes)
            {
                var current = concreteType;
                while (current != null && current != typeof(object))
                {
                    if (!typeof(IManagedObject).IsAssignableFrom(current))
                        break;

                    if (current == typeof(FieldElement))
                        break;

                    if (current == typeof(ManagedObject))
                        break;

                    includedTypes.Add(current);

                    current = current.BaseType;
                }
            }

            var nodeByType = new Dictionary<Type, ManagedObjectTypeNodeViewModel>();
            foreach (var type in includedTypes.OrderBy(GetHierarchyDepth).ThenBy(t => t.Name))
            {
                nodeByType[type] = CreateTypeNode(type);
            }

            TypeNodes.Clear();
            var categorizedRootNodes = new List<ManagedObjectTypeNodeViewModel>();
            var uncategorizedRootNodes = new List<ManagedObjectTypeNodeViewModel>();

            foreach (var node in nodeByType.Values)
            {
                var parentType = node.ManagedObjectType.BaseType;
                if (parentType == null || !nodeByType.TryGetValue(parentType, out var parentNode))
                {
                    var isConcreteDirectManagedObject =
                        node.ManagedObjectType.BaseType == typeof(ManagedObject) &&
                        !node.ManagedObjectType.IsAbstract;

                    if (isConcreteDirectManagedObject)
                    {
                        uncategorizedRootNodes.Add(node);
                        continue;
                    }

                    if (node.ManagedObjectType != typeof(ManagedObject))
                        categorizedRootNodes.Add(node);

                    continue;
                }

                parentNode.Children.Add(node);
            }

            foreach (var node in categorizedRootNodes.OrderBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase))
                TypeNodes.Add(node);

            foreach (var node in uncategorizedRootNodes.OrderBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase))
                TypeNodes.Add(node);

            var selected = FindNodeByType(previouslySelectedType)
                ?? TypeNodes.FirstOrDefault(x => !x.IsCategoryOnly)
                ?? TypeNodes.FirstOrDefault();

            if (!ReferenceEquals(SelectedTypeNode, selected))
                SelectedTypeNode = selected;
        }

        private ManagedObjectTypeNodeViewModel CreateTypeNode(Type type)
        {
            var displayName = FlowBloxResourceUtil.GetPluralDisplayName(type);
            var icon = ResolveTypeIcon(type);
            var canCreateInstance = !IsReadOnly && !type.IsAbstract && HasDefaultConstructor(type);

            return new ManagedObjectTypeNodeViewModel(type, displayName, icon, canCreateInstance);
        }

        private static FlowBloxReactiveObject? CreateIconComponent(Type type)
        {
            try
            {
                return Activator.CreateInstance(type) as FlowBloxReactiveObject;
            }
            catch
            {
                return null;
            }
        }

        private ImageSource ResolveTypeIcon(Type type)
        {
            if (type.IsAbstract)
                return WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.FolderOutline, 16);

            var iconPng = FlowBloxComponentIconCache.GetOrCreateIcon16Png(type, () => CreateIconComponent(type));
            var image = ToImageSource(iconPng);
            if (image != null)
                return image;

            return WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.CubeOutline, 16);
        }

        private void RefreshManagedObjects()
        {
            var selectedTypeNode = SelectedTypeNode;
            var selectedType = selectedTypeNode?.ManagedObjectType;
            if (_registry == null || selectedType == null || selectedTypeNode.IsCategoryOnly)
            {
                ManagedObjects.Clear();
                SelectedEntry = null;
                OnPropertyChanged(nameof(SelectedEntryActions));
                OnPropertyChanged(nameof(HasSelectedEntryActions));
                return;
            }

            var selectedObjectRef = SelectedEntry?.ManagedObject;

            var items = _registry.GetManagedObjects(selectedType)
                .Where(x => x is not FieldElement)
                .OfType<IManagedObject>()
                .OrderBy(x => x.Name)
                .ToList();

            ManagedObjects.Clear();
            foreach (var managedObject in items)
            {
                var entry = BuildEntry(managedObject);
                ManagedObjects.Add(entry);
            }

            SelectedEntry = ManagedObjects.FirstOrDefault(x => ReferenceEquals(x.ManagedObject, selectedObjectRef))
                ?? ManagedObjects.FirstOrDefault();

            OnPropertyChanged(nameof(SelectedEntryActions));
            OnPropertyChanged(nameof(HasSelectedEntryActions));
        }

        private ManagedObjectEntryViewModel BuildEntry(IManagedObject managedObject)
        {
            var iconPng = FlowBloxComponentIconCache.GetOrCreateIcon16Png((FlowBloxReactiveObject)managedObject);
            var icon = ToImageSource(iconPng) ?? WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.CubeOutline, 16);

            var entry = new ManagedObjectEntryViewModel(managedObject, icon)
            {
                Name = managedObject.Name,
                DisplayableProperties = BuildDisplayableProperties(managedObject),
                UsedIn = BuildUsedIn(managedObject)
            };

            foreach (var action in BuildActions(managedObject))
            {
                entry.Actions.Add(action);
            }

            return entry;
        }

        private IEnumerable<UIActionViewModel> BuildActions(IManagedObject managedObject)
        {
            try
            {
                var provider = new WpfUIActionsProvider();
                return provider.GetToolStripItemsForComponent((IFlowBloxComponent)managedObject, includePropertyWindowOnlyActions: false);
            }
            catch
            {
                return Enumerable.Empty<UIActionViewModel>();
            }
        }

        private string BuildDisplayableProperties(IManagedObject managedObject)
        {
            var noneText = Resources.ManagedObjectsView.DisplayableProperties_None;
            var propertyNames = managedObject.GetDisplayableProperties()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, nameof(IFlowBloxComponent.Name), StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (!propertyNames.Any())
                return noneText;

            var values = new List<string>();
            foreach (var propertyName in propertyNames)
            {
                var property = managedObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    continue;

                var propertyValue = property.GetValue(managedObject);
                var propertyText = FormatDisplayablePropertyValue(propertyValue);
                if (string.IsNullOrWhiteSpace(propertyText))
                    continue;

                var propertyDisplayName = GetPropertyDisplayName(property);
                values.Add($"{propertyDisplayName}: {propertyText}");
            }

            if (!values.Any())
                return noneText;

            return string.Join(", ", values);
        }

        private static string FormatDisplayablePropertyValue(object? propertyValue)
        {
            if (propertyValue == null)
                return string.Empty;

            if (propertyValue is string s)
                return TextHelper.ShortenString(s, 30, true);

            if (propertyValue is Enum e)
                return e.GetDisplayName();

            if (propertyValue is IFlowBloxComponent component)
                return component.Name;

            if (propertyValue is IEnumerable<IFlowBloxComponent> components)
            {
                return string.Join(", ", components.Select(x => x?.Name).Where(x => !string.IsNullOrWhiteSpace(x)).Take(3));
            }

            if (propertyValue is System.Collections.IEnumerable enumerable and not string)
            {
                var values = new List<string>();
                foreach (var item in enumerable)
                {
                    if (item == null)
                        continue;

                    values.Add(item.ToString() ?? string.Empty);
                    if (values.Count >= 3)
                        break;
                }

                return string.Join(", ", values.Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            return propertyValue.ToString() ?? string.Empty;
        }

        private static string GetPropertyDisplayName(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            return FlowBloxResourceUtil.GetDisplayName(displayAttribute, false) ?? property.Name;
        }

        private string BuildUsedIn(IManagedObject target)
        {
            var noneText = Resources.ManagedObjectsView.UsedIn_None;
            if (_registry == null)
                return noneText;

            var usedIn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var originalTarget = _registry.GetOriginalRef(target) as IManagedObject ?? target;

            var flowBlocks = _registry.GetFlowBlocks().ToList();
            foreach (var flowBlock in flowBlocks)
            {
                if (ReferencesTarget(flowBlock, originalTarget))
                    usedIn.Add(flowBlock.Name);
            }

            foreach (var managedObject in _registry.GetManagedObjects().Where(x => x is not FieldElement))
            {
                if (ReferenceEquals(managedObject, target))
                    continue;

                if (ReferencesTarget(managedObject, originalTarget))
                    usedIn.Add(managedObject.Name);
            }

            if (!usedIn.Any())
                return noneText;

            return string.Join(", ", usedIn.OrderBy(x => x));
        }

        private bool ReferencesTarget(IFlowBloxComponent source, IManagedObject originalTarget)
        {
            if (_registry == null)
                return false;

            if (source is BaseFlowBlock flowBlock && flowBlock.DefinedManagedObjects.Any(x => IsSameManagedObject(x, originalTarget)))
                return true;

            return source.GetAssociatedManagedObjects().Any(x => IsSameManagedObject(x, originalTarget));
        }

        private bool IsSameManagedObject(IManagedObject? a, IManagedObject? b)
        {
            if (_registry == null || a == null || b == null)
                return false;

            if (ReferenceEquals(a, b))
                return true;

            var originalA = _registry.GetOriginalRef(a) as IManagedObject ?? a;
            var originalB = _registry.GetOriginalRef(b) as IManagedObject ?? b;
            return ReferenceEquals(originalA, originalB);
        }

        private void AddManagedObject()
        {
            var selectedType = SelectedTypeNode?.ManagedObjectType;
            if (_registry == null || selectedType == null)
                return;

            if (!HasDefaultConstructor(selectedType))
            {
                _messageBoxService.ShowMessageBox(
                    string.Format(Resources.ManagedObjectsView.Message_CannotCreateInstance_Description, selectedType.Name),
                    Resources.ManagedObjectsView.Message_CannotCreateInstance_Title,
                    FlowBloxMessageBoxTypes.Warning);
                return;
            }

            if (Activator.CreateInstance(selectedType) is not IManagedObject newManagedObject)
                return;

            _registry.PostProcessManagedObjectCreated(newManagedObject);

            var propertyWindow = new Views.PropertyWindow(new PropertyWindowArgs(newManagedObject, readOnly: false, isNew: true));
            var result = _dialogService.ShowWPFDialog(propertyWindow, isModal: true);
            if (result == true)
            {
                _registry.RegisterManagedObject(newManagedObject);
            }
        }

        private bool CanAddManagedObject()
            => !IsReadOnly && SelectedTypeNode?.CanCreateInstance == true;

        private void EditSelectedManagedObject()
        {
            var managedObject = SelectedEntry?.ManagedObject;
            if (managedObject == null)
                return;

            var propertyWindow = new Views.PropertyWindow(new PropertyWindowArgs(managedObject, readOnly: IsReadOnly));
            _dialogService.ShowWPFDialog(propertyWindow, isModal: true);
            RefreshManagedObjects();
        }

        private bool CanEditManagedObject()
            => SelectedEntry?.ManagedObject != null;

        private void DeleteSelectedManagedObject()
        {
            if (IsReadOnly)
                return;

            var managedObject = SelectedEntry?.ManagedObject;
            if (_registry == null || managedObject == null)
                return;

            if (!managedObject.IsDeletable(out var dependencies))
            {
                var dependencyText = string.Join(", ", dependencies.Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
                _messageBoxService.ShowMessageBox(
                    string.Format(Resources.ManagedObjectsView.Message_DeleteBlocked_Description, managedObject.Name, dependencyText),
                    Resources.ManagedObjectsView.Message_DeleteBlocked_Title,
                    FlowBloxMessageBoxTypes.Warning);
                return;
            }

            var confirmResult = _messageBoxService.ShowMessageBox(
                string.Format(Resources.ManagedObjectsView.Message_DeleteConfirm_Description, managedObject.Name),
                Resources.ManagedObjectsView.Message_DeleteConfirm_Title,
                FlowBloxMessageBoxTypes.Question);

            if (confirmResult != FlowBloxMessageBoxDialogResult.Yes)
                return;

            _registry.Unregister(managedObject);
        }

        private bool CanDeleteManagedObject()
            => !IsReadOnly && SelectedEntry?.ManagedObject != null;

        private ManagedObjectTypeNodeViewModel? FindNodeByType(Type? type)
        {
            if (type == null)
                return null;

            foreach (var root in TypeNodes)
            {
                var result = FindNodeByTypeRecursive(root, type);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static ManagedObjectTypeNodeViewModel? FindNodeByTypeRecursive(ManagedObjectTypeNodeViewModel node, Type type)
        {
            if (node.ManagedObjectType == type)
                return node;

            foreach (var child in node.Children)
            {
                var result = FindNodeByTypeRecursive(child, type);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static int GetHierarchyDepth(Type type)
        {
            var depth = 0;
            var current = type;
            while (current?.BaseType != null)
            {
                depth++;
                current = current.BaseType;
            }

            return depth;
        }

        private static bool HasDefaultConstructor(Type type)
            => type.GetConstructor(Type.EmptyTypes) != null;

        private static bool IsSupportedManagedObjectType(Type type)
        {
            if (type == null || !type.IsClass || type.IsInterface)
                return false;

            if (!typeof(IManagedObject).IsAssignableFrom(type))
                return false;

            if (typeof(FieldElement).IsAssignableFrom(type))
                return false;

            if (typeof(FlowBloxTestDefinition).IsAssignableFrom(type))
                return false;

            return true;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        private static IEnumerable<Type> GetTypesFromLoadContext(AssemblyLoadContext? loadContext)
        {
            if (loadContext == null)
                return Enumerable.Empty<Type>();

            return loadContext.Assemblies.SelectMany(GetLoadableTypes);
        }

        private static ImageSource? ToImageSource(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            using var stream = new MemoryStream(imageData);
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void InvalidateCommands()
        {
            AddCommand.Invalidate();
            EditCommand.Invalidate();
            DeleteCommand.Invalidate();
            OnPropertyChanged(nameof(SelectedEntryActions));
            OnPropertyChanged(nameof(HasSelectedEntryActions));
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

        private void Unsubscribe()
        {
            if (_registry != null)
            {
                _registry.OnManagedObjectAdded -= Registry_OnManagedObjectAdded;
                _registry.OnManagedObjectRemoved -= Registry_OnManagedObjectRemoved;
            }

            if (_project != null)
            {
                _project.ExtensionsReloaded -= Project_ExtensionsReloaded;
                _project.BeforeUnloadExtension -= Project_BeforeUnloadExtension;
            }
        }

        public void Dispose()
        {
            FlowBloxProjectManager.Instance.ProjectChanged -= ProjectManager_ProjectChanged;
            Unsubscribe();
            TypeNodes.Clear();
            ManagedObjects.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
