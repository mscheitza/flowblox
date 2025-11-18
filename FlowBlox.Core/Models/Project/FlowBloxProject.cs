using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Factories;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Migration;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Json;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Loader;

namespace FlowBlox.Core.Models.Project
{
    [Serializable()]
    public class FlowBloxProject
    {
        public Guid ProjectGuid { get; set; }
        public string ProjectName { get; set; }
        public string Author { get; set; }
        public string ProjectDescription { get; set; }
        public string Notice { get; set; }
        public int GridSizeX { get; set; }
        public int GridSizeY { get; set; }

        [JsonIgnore()]
        public FlowBloxRegistry FlowBloxRegistry { get; }

        public List<BaseFlowBlock> FlowBlocks { get; set; }

        public List<IManagedObject> ManagedObjects { get; set; }

        public List<FieldElement> UserFields { get; set; }

        public List<IProjectDependendData> ProjectDependendDataObjects { get; set; }

        [JsonIgnore]
        private Dictionary<string, AssemblyLoadContext> _loadContexts = new Dictionary<string, AssemblyLoadContext>();

        [JsonIgnore()]
        public List<FlowBloxProjectExtension> Extensions { get; set; }

        private static readonly ILogger _logger = FlowBloxLogManager.Instance.GetLogger();

        public FlowBloxProject()
        {
            this.ProjectGuid = Guid.NewGuid();
            this.GridSizeX = GlobalConstants.GridSizeX;
            this.GridSizeX = GlobalConstants.GridSizeY;
            this.FlowBloxRegistry = new FlowBloxRegistry();
            this.Extensions = new List<FlowBloxProjectExtension>();
            this.ProjectDependendDataObjects = new List<IProjectDependendData>();
            _logger.Info("FlowBloxProject instance created.");
        }

        private static Dictionary<string, AssemblyLoadContext> LoadExtensions(IEnumerable<FlowBloxProjectExtension> extensions)
        {
            var loadContexts = new Dictionary<string, AssemblyLoadContext>();
            foreach (var extension in extensions)
            {
                LoadExtension(extension, loadContexts);
            }
            return loadContexts;
        }

        public event EventHandler ExtensionsReloaded;

        public FlowBloxProjectReloadResult ReloadExtensions()
        {
            var currentExtensionDirectories = new HashSet<string>(
                this.Extensions.Select(e => e.LocalExtensionDirectory),
                StringComparer.OrdinalIgnoreCase);

            var directoriesToUnload = _loadContexts.Keys
                .Where(key => !currentExtensionDirectories.Contains(key))
                .ToList();

            return ReloadExtensions(directoriesToUnload, this.Extensions);
        }

        public FlowBloxProjectReloadResult ReloadExtensions(IEnumerable<string> directoriesToUnload)
        {
            List<FlowBloxProjectExtension> extensionsToLoad = new List<FlowBloxProjectExtension>();
            return ReloadExtensions(directoriesToUnload, extensionsToLoad);
        }

        public FlowBloxProjectReloadResult ReloadExtensions(IEnumerable<string> directoriesToUnload, List<FlowBloxProjectExtension> extensionsToLoad)
        {
            var reloadResult = new FlowBloxProjectReloadResult();

            _logger.Info("Reloading extensions...");
            
            var weakReferences = new Dictionary<WeakReference, List<string>>();

            foreach (var directory in directoriesToUnload)
            {
                if (_loadContexts.TryGetValue(directory, out var loadContext))
                {
                    var remainingAssemblies = loadContext.Assemblies
                        .Select(a => a.FullName)
                        .ToList();

                    var weakReference = new WeakReference(loadContext);
                    weakReferences[weakReference] = remainingAssemblies!;
                    UnloadExtension(directory, loadContext);
                    loadContext = null;
                }
            }

            foreach (var extension in extensionsToLoad)
            {
                var loadContext = LoadExtension(extension, _loadContexts);

                if (loadContext == null || !loadContext.Assemblies.Any())
                {
                    reloadResult.Success = false;
                    reloadResult.UnloadableExtensions.Add($"{extension.Name}/{extension.Version}");
                    _logger.Warn($"Failed to load extension: {extension.Name} Version: {extension.Version}");
                }
            }


            foreach (var kvp in weakReferences)
            {
                for (int i = 0; kvp.Key.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                if (kvp.Key.IsAlive)
                {
                    reloadResult.Success = false;
                    reloadResult.RemainingAssemblies.AddRange(kvp.Value);
                }
            }

            if (reloadResult.Success)
            {
                _logger.Info("Extensions reloaded successfully.");
            }
            else
            {
                if (reloadResult.RemainingAssemblies.Any())
                {
                    _logger.Warn($"Extensions could not be reloaded successfully. Remaining assemblies: {string.Join(", ", reloadResult.RemainingAssemblies)}.");
                }

                if (reloadResult.UnloadableExtensions.Any())
                {
                    _logger.Warn($"Failed to load the following extensions: {string.Join(", ", reloadResult.UnloadableExtensions)}.");
                }
            }

            ExtensionsReloaded?.Invoke(this, EventArgs.Empty);

            return reloadResult;
        }


        private static AssemblyLoadContext LoadExtension(FlowBloxProjectExtension extension, Dictionary<string, AssemblyLoadContext> loadContexts)
        {
            string directoryPath = extension.LocalExtensionDirectory;
            if (!loadContexts.ContainsKey(directoryPath))
            {
                _logger.Info($"Loading extension from {directoryPath}.");
                var loadContext = new FlowBloxProjectLoadContext(directoryPath);
                loadContexts.Add(directoryPath, loadContext);
                loadContext.LoadAssembliesFromDirectory();
                OnAfterExtensionLoaded(loadContext);
                _logger.Info($"Extension loaded successfully from {directoryPath}.");
                return loadContext;
            }
            return null;
        }

        private static void OnAfterExtensionLoaded(AssemblyLoadContext loadContext)
        {
            FlowBloxServiceLocator.Instance.RegisterServices(loadContext);
            FlowBlockCategory.InvokeRegistration();
            FlowBloxToolboxCategory.InvokeRegistration();
        }

        public event EventHandler<AssemblyLoadContext> BeforeUnloadExtension;

        private void UnloadExtension(string directoryPath, AssemblyLoadContext loadContext)
        {
            FlowBlockCategory.InvokeDeregistration(loadContext);
            FlowBloxServiceLocator.Instance.UnregisterServices(loadContext);
            BeforeUnloadExtension?.Invoke(this, loadContext);
            _logger.Info($"Unloading extension at {directoryPath}.");
            loadContext.Unload();
            _loadContexts.Remove(directoryPath);
            _logger.Info($"Extension unloaded successfully from {directoryPath}.");
        }

        public IEnumerable<T> CreateInstances<T>(Func<Type, bool> typeFilter = null) => AppDomainInstanceFactory.CreateInstances<T>(_loadContexts.Values, typeFilter);

        private void CreateProjectDependendDataObjectsIfNotExist()
        {
            var dependendDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IProjectDependendData).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var dependendDataType in dependendDataTypes)
            {
                if (!this.ProjectDependendDataObjects.Any(x => x.GetType() == dependendDataType))
                {
                    var instance = (IProjectDependendData)Activator.CreateInstance(dependendDataType);
                    this.ProjectDependendDataObjects.Add(instance);
                    _logger.Info($"Created instance of {dependendDataType.FullName} as project dependend data.");
                }
            }
        }

        internal void OnProjectLoaded()
        {
            _logger.Info("Loading project...");

            var options = FlowBloxOptions.GetOptionInstance();
            options.InitDefaults(false);

            // ProjectDependendData
            CreateProjectDependendDataObjectsIfNotExist();

            List<IFlowBloxComponent> loadedComponents = new List<IFlowBloxComponent>();

            // FlowBlocks
            if (this.FlowBlocks != null)
            {
                foreach (var flowBlock in this.FlowBlocks.ExceptNull())
                {
                    this.FlowBloxRegistry.RegisterFlowBlock(flowBlock);
                    loadedComponents.Add(flowBlock);
                }
            }

            // ManagedObjects
            if (this.ManagedObjects != null)
            {
                foreach (var managedObject in this.ManagedObjects
                    .ExceptNull()
                    .Where(x => x is not FieldElement))
                {
                    this.FlowBloxRegistry.Register(managedObject);
                    loadedComponents.Add(managedObject);
                }
            }

            // UserFields
            if (this.UserFields != null)
            {
                foreach (var userField in this.UserFields)
                {
                    this.FlowBloxRegistry.Register(userField);
                    loadedComponents.Add(userField);
                }
            }

            // Process OnAfterLoad-Events
            foreach (var loadedComponent in loadedComponents)
            {
                loadedComponent.OnAfterLoad();
            }

            _logger.Info("Project loaded successfully.");
        }

        internal void OnProjectClosed()
        {
            _logger.Info("Closing project...");

            var reloadResult = ReloadExtensions(_loadContexts.Keys);

            if (!reloadResult.Success)
                throw new ProjectExtensionsUnloadException(reloadResult.RemainingAssemblies, reloadResult.UnloadableExtensions);

            _logger.Info("Project closed successfully.");
        }

        private const string ExtensionFileSuffix = ".extensions";

        public static FlowBloxProject FromFile(string fileName)
        {
            _logger.Info($"Loading project from file: {fileName}");
            try
            {
                var extensionsFilePath = Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + ExtensionFileSuffix);

                Dictionary<string, AssemblyLoadContext> loadContexts = null;
                List<FlowBloxProjectExtension> extensions = null;
                if (File.Exists(extensionsFilePath))
                {
                    extensions = JsonHelper.DeserializeJsonFromFile<List<FlowBloxProjectExtension>>(extensionsFilePath);
                    loadContexts = LoadExtensions(extensions);
                    _logger.Info($"Extensions loaded from {extensionsFilePath}");
                }
                var settings = JsonSettings.ProjectImport(loadContexts);
                var projectFileContent = File.ReadAllText(fileName);
                AdjustFileContentBeforeDeserialization(fileName, ref projectFileContent);
                var project = JsonConvert.DeserializeObject<FlowBloxProject>(projectFileContent, settings);

                if (extensions != null)
                {
                    project.Extensions.AddRange(extensions);

                    foreach (var loadContext in loadContexts)
                    {
                        project._loadContexts.Add(loadContext.Key, loadContext.Value);
                    }
                }

                _logger.Info($"Project loaded successfully from {fileName}");
                return project;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load project from file: {fileName}", ex);
                throw;
            }
        }

        private const string ProjectDirectoryPlaceholder = "${ProjectDir}";

        private static string MigrateJson(string json)
        {
            JObject rootObject = JObject.Parse(json);
            var migrationStrategies = FlowBloxServiceLocator.Instance.GetServices<IFlowBloxMigrationStrategy>();
            MigrateComponentList(rootObject, "FlowBlocks", migrationStrategies);
            MigrateComponentList(rootObject, "ManagedObjects", migrationStrategies);
            MigrateComponentList(rootObject, "UserFields", migrationStrategies);
            return rootObject.ToString();
        }

        private static void MigrateComponentList(JObject rootObject, string propertyName, IEnumerable<IFlowBloxMigrationStrategy> strategies)
        {
            if (!rootObject.TryGetValue(propertyName, out JToken token))
                return;

            JArray? jsonComponents = token switch
            {
                JArray arr => arr,
                JObject obj when obj.TryGetValue("$values", out var valuesToken) && valuesToken is JArray array => array,
                _ => null
            };

            if (jsonComponents == null)
                return;

            for (int i = 0; i < jsonComponents.Count; i++)
            {
                JObject componentJson = (JObject)jsonComponents[i];
                string typeName = componentJson["$type"]?.ToString();
                string versionString = componentJson["Version"]?.ToString() ?? "1.0.0.0";

                if (typeName == null)
                    continue;

                Type componentType = TypeNameHelper.FindTypeByFullOrSimpleName(typeName);
                if (componentType == null)
                    continue;

                Version componentVersion = new Version(versionString);
                Version assemblyVersion = componentType.Assembly.GetName().Version;

                if (componentVersion > assemblyVersion)
                    continue;

                var applicableStrategies = strategies
                    .Where(strategy => strategy.ComponentType.IsAssignableFrom(componentType) &&
                                       strategy.Version >= componentVersion)
                    .OrderBy(strategy => strategy.Version)
                    .ToList();

                foreach (var strategy in applicableStrategies)
                {
                    strategy.Migrate(componentJson);
                }

                componentJson["Version"] = assemblyVersion.ToString();
            }
        }

        private static void AdjustFileContentBeforeDeserialization(string fileName, ref string json)
        {
            string directoryPath = Path.GetDirectoryName(fileName);
            json = json.Replace(ProjectDirectoryPlaceholder, directoryPath.Replace(@"\", @"\\"));
            json = MigrateJson(json);
        }

        private static void AdjustFileContentAfterSerialization(string fileName, ref string json)
        {
            string directoryPath = Path.GetDirectoryName(fileName);
            json = json.Replace(directoryPath.Replace(@"\", @"\\"), ProjectDirectoryPlaceholder);
        }

        public void Save(string fileName)
        {
            _logger.Info($"Saving project to file: {fileName}");
            try
            {
                var extensionsFilePath = Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + ExtensionFileSuffix);

                this.FlowBlocks = new List<BaseFlowBlock>();
                this.FlowBlocks.AddRange(this.FlowBloxRegistry.GetFlowBlocks());

                this.ManagedObjects = new List<IManagedObject>();
                this.ManagedObjects.AddRange(this.FlowBloxRegistry.GetManagedObjects());

                this.UserFields = new List<FieldElement>();
                this.UserFields.AddRange(this.FlowBloxRegistry.GetUserFields());

                var projectFileContent = JsonConvert.SerializeObject(this, JsonSettings.ProjectExport());
                AdjustFileContentAfterSerialization(fileName, ref projectFileContent);
                File.WriteAllText(fileName, projectFileContent);

                JsonHelper.SerializeToFile(extensionsFilePath, Extensions != null ? 
                    Extensions : 
                    new List<FlowBloxProjectExtension>());

                _logger.Info($"Project saved successfully to {fileName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to save project to file: {fileName}", ex);
                throw;
            }
        }
    }
}
