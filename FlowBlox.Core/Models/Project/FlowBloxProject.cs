using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Factories;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Runtime.Loader;
using System.Text;
using static FlowBlox.Core.ExternalServices.FlowBloxWebApi.FlowBloxWebApiService;

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

        [JsonIgnore()]
        public string ProjectSpaceGuid { get; set; }

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
            FlowBloxOptions.GetOptionInstance().InitDefaults(false);
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
        private const string ProjectSpaceMetadataSuffix = ".prjspace";

        public static FlowBloxProject FromJsonContents(
            string projectJson,
            string extensionsJson,
            string projectSpaceGuid = null,
            string fileNameForAdjustments = null)
        {
            _logger.Info($"Loading project from JSON content (SpaceGuid='{projectSpaceGuid ?? ""}', File='{fileNameForAdjustments ?? ""}')");

            try
            {
                // Extensions
                Dictionary<string, AssemblyLoadContext> loadContexts = null;
                List<FlowBloxProjectExtension> extensions = null;

                if (!string.IsNullOrWhiteSpace(extensionsJson))
                {
                    extensions = JsonConvert.DeserializeObject<List<FlowBloxProjectExtension>>(extensionsJson);

                    if (extensions != null && extensions.Count > 0)
                    {
                        loadContexts = LoadExtensions(extensions);
                        _logger.Info($"Extensions loaded from JSON. Count={extensions.Count}");
                    }
                }

                // Project JSON (same settings logic)
                var settings = JsonSettings.ProjectImport(loadContexts);

                var projectFileContent = projectJson ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(fileNameForAdjustments))
                    AdjustFileContentBeforeDeserialization(fileNameForAdjustments, ref projectFileContent);

                var project = JsonConvert.DeserializeObject<FlowBloxProject>(projectFileContent, settings);

                if (project == null)
                    throw new Exception("Failed to deserialize FlowBloxProject from JSON.");

                if (extensions != null && extensions.Count > 0)
                {
                    project.Extensions.AddRange(extensions);

                    if (loadContexts != null)
                    {
                        foreach (var lc in loadContexts)
                            project._loadContexts.Add(lc.Key, lc.Value);
                    }
                }

                if (!string.IsNullOrWhiteSpace(projectSpaceGuid))
                    project.ProjectSpaceGuid = projectSpaceGuid;

                _logger.Info("Project loaded successfully from JSON content.");
                return project;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load project from JSON content.", ex);
                throw;
            }
        }

        public static FlowBloxProject FromFile(string fileName)
        {
            _logger.Info($"Loading project from file: {fileName}");
            try
            {
                // Extensions JSON
                var extensionsFilePath = Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + ExtensionFileSuffix);

                string extensionsJson = null;
                if (File.Exists(extensionsFilePath))
                {
                    extensionsJson = File.ReadAllText(extensionsFilePath);
                    _logger.Info($"Extensions JSON loaded from {extensionsFilePath}");
                }

                // Project Space metadata
                var projectSpaceFilePath = Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + ProjectSpaceMetadataSuffix);

                string projectSpaceGuid = null;
                if (File.Exists(projectSpaceFilePath))
                {
                    var metaJson = File.ReadAllText(projectSpaceFilePath);
                    var metadata = JsonConvert.DeserializeObject<FlowBloxProjectSpaceMetadata>(metaJson);
                    projectSpaceGuid = metadata?.ProjectGuid;
                }

                // Project JSON
                var projectFileContent = File.ReadAllText(fileName);

                // Central loading
                var project = FromJsonContents(
                    projectJson: projectFileContent,
                    extensionsJson: extensionsJson,
                    projectSpaceGuid: projectSpaceGuid,
                    fileNameForAdjustments: fileName);

                _logger.Info($"Project loaded successfully from {fileName}");
                return project;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load project from file: {fileName}", ex);
                throw;
            }
        }

        private static (string ProjectJson, string ExtensionsJson) ExtractProjectSpaceZip(byte[] zipBytes)
        {
            using (var ms = new MemoryStream(zipBytes))
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false))
            {
                var projectEntry = archive.GetEntry("project_file.json");
                var extensionEntry = archive.GetEntry("extension_file.json");

                if (projectEntry == null)
                    throw new Exception("ZIP does not contain 'project_file.json'.");

                string ReadEntry(ZipArchiveEntry e)
                {
                    using (var s = e.Open())
                    using (var r = new StreamReader(s, Encoding.UTF8))
                        return r.ReadToEnd();
                }

                var projectJson = ReadEntry(projectEntry);
                var extensionsJson = extensionEntry != null ? ReadEntry(extensionEntry) : null;

                return (projectJson, extensionsJson);
            }
        }

        public static async Task<FlowBloxProject> FromProjectSpaceGuidAsync(string projectSpaceGuid, string userToken, FlowBloxWebApiService webApi)
        {
            _logger.Info($"Loading project from Project Space. Guid={projectSpaceGuid}");

            if (string.IsNullOrWhiteSpace(projectSpaceGuid))
                throw new ArgumentException("projectSpaceGuid is required.", nameof(projectSpaceGuid));

            try
            {
                // Fetch project metadata from API (no content anymore)
                var remoteResp = await webApi.GetProjectAsync(new FbProjectRequest { Guid = projectSpaceGuid }, userToken);
                if (!remoteResp.Success || remoteResp.ResultObject == null)
                    throw new InvalidOperationException("Project not found in Project Space.");

                // Fetch project content via separate endpoint
                var contentResp = await webApi.GetProjectContentAsync(userToken, Guid.Parse(projectSpaceGuid));
                if (!contentResp.Success)
                    throw new InvalidOperationException($"Failed to retrieve project content from Project Space. {contentResp.ErrorMessage}");

                if (string.IsNullOrWhiteSpace(contentResp.ResultObject))
                    throw new Exception("Project content is missing in Project Space response.");

                var zipBytes = Convert.FromBase64String(contentResp.ResultObject);
                var extracted = ExtractProjectSpaceZip(zipBytes);

                // Load via central method. No file adjustments for ProjectSpace.
                var project = FromJsonContents(
                    projectJson: extracted.ProjectJson,
                    extensionsJson: extracted.ExtensionsJson,
                    projectSpaceGuid: projectSpaceGuid,
                    fileNameForAdjustments: null);

                project.ProjectSpaceGuid = projectSpaceGuid;

                _logger.Info($"Project loaded successfully from Project Space. Guid={projectSpaceGuid}");
                return project;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load project from Project Space. Guid={projectSpaceGuid}", ex);
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

        private const string ProjectSpaceProjectFileName = "project_file.json";
        private const string ProjectSpaceExtensionsFileName = "extension_file.json";

        private string CreateProjectSpaceZipBase64()
        {
            // Collect current state before export
            this.FlowBlocks = [.. this.FlowBloxRegistry.GetFlowBlocks()];
            this.ManagedObjects = [.. this.FlowBloxRegistry.GetManagedObjects()];
            this.UserFields = [.. this.FlowBloxRegistry.GetUserFields()];

            // Serialize project + extensions
            var projectFileContent = JsonConvert.SerializeObject(this, JsonSettings.ProjectExport());
            var extensionsFileContent = JsonConvert.SerializeObject(
                this.Extensions ?? new List<FlowBloxProjectExtension>());

            using (var memoryStream = new MemoryStream())
            {
                // Create ZIP archive in memory
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    // project_file.json
                    var projectEntry = archive.CreateEntry(
                        ProjectSpaceProjectFileName,
                        CompressionLevel.Optimal);

                    using (var entryStream = projectEntry.Open())
                    using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                    {
                        writer.Write(projectFileContent);
                    }

                    // extension_file.json
                    var extensionEntry = archive.CreateEntry(
                        ProjectSpaceExtensionsFileName,
                        CompressionLevel.Optimal);

                    using (var entryStream = extensionEntry.Open())
                    using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                    {
                        writer.Write(extensionsFileContent);
                    }
                }

                // Convert ZIP bytes to Base64
                var zipBytes = memoryStream.ToArray();
                return Convert.ToBase64String(zipBytes);
            }
        }

        public async Task<ApiResponse> SaveToProjectSpaceAsync(string projectGuid, string userToken, FlowBloxWebApiService webApi)
        { 
            if (string.IsNullOrWhiteSpace(projectGuid))
            {
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = "ProjectGuid is missing."
                };
            }

            try
            {
                // Create ZIP payload (project + extensions) and send as base64
                var base64Zip = CreateProjectSpaceZipBase64();

                var request = new FbProjectChangeRequest
                {
                    ProjectGuid = projectGuid,
                    ContentBase64 = base64Zip
                };

                var result = await webApi.UpdateProjectAsync(userToken, request);
                if (result?.Success == true)
                    ProjectSpaceGuid = projectGuid;

                return result;
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }

        public void Save(string fileName)
        {
            _logger.Info($"Saving project to file: {fileName}");
            try
            {
                this.FlowBlocks = [.. this.FlowBloxRegistry.GetFlowBlocks()];
                this.ManagedObjects = [.. this.FlowBloxRegistry.GetManagedObjects()];
                this.UserFields = [.. this.FlowBloxRegistry.GetUserFields()];

                var projectFileContent = JsonConvert.SerializeObject(this, JsonSettings.ProjectExport());
                AdjustFileContentAfterSerialization(fileName, ref projectFileContent);
                File.WriteAllText(fileName, projectFileContent);

                var extensionsFilePath = Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + ExtensionFileSuffix);

                JsonHelper.SerializeToFile(extensionsFilePath, Extensions != null ?
                    Extensions :
                    new List<FlowBloxProjectExtension>());

                var projectSpaceMetadataPath = Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + ProjectSpaceMetadataSuffix);

                JsonHelper.SerializeToFile(extensionsFilePath, Extensions != null ?
                   Extensions :
                   new List<FlowBloxProjectExtension>());

                JsonHelper.SerializeToFile(projectSpaceMetadataPath, new FlowBloxProjectSpaceMetadata()
                {
                    ProjectGuid = ProjectSpaceGuid
                });

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
