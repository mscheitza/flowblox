using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace FlowBlox.Core.Models.Project
{
    public class FlowBloxProjectLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly string _pluginPath;
        private readonly IFlowBloxUIEvaluationService _uiEvaluationService;

        public FlowBloxProjectLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
            _pluginPath = pluginPath;
            _uiEvaluationService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxUIEvaluationService>();
        }

        public void LoadAssembliesFromDirectory()
        {
            if (!Directory.Exists(_pluginPath))
                return;

            var dllFiles = Directory.GetFiles(_pluginPath, "*.dll");

            Parallel.ForEach(dllFiles, dll =>
            {
                try
                {
                    if (!_uiEvaluationService.IsUISupported() && IsFlowBloxUIExtension(dll))
                    {
                        FlowBloxLogManager.Instance.GetLogger().Info($"Skipping UI assembly (headless context): {dll}");
                        return;
                    }

                    var assemblyName = AssemblyName.GetAssemblyName(dll);

                    var loadedAssembly = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .FirstOrDefault(a => a.GetName().FullName == assemblyName.FullName);

                    if (loadedAssembly != null)
                    {
                        FlowBloxLogManager.Instance.GetLogger().Info($"Assembly already loaded: {dll}");
                        return;
                    }

                    LoadFromAssemblyPath(dll);
                }
                catch (Exception e)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error($"Failed to load assembly: {dll}", e);
                }
            });
        }

        private bool IsFlowBloxUIExtension(string dllPath)
        {
            try
            {
                var depsJsonPath = Path.ChangeExtension(dllPath, ".deps.json");
                if (!File.Exists(depsJsonPath))
                    return false;

                var depsJsonContent = File.ReadAllText(depsJsonPath);
                var metadata = ExtensionContentMetadataExtractor.ParseDepsJson(depsJsonContent);

                if (metadata?.Dependencies == null)
                    return false;

                return metadata.Dependencies.Any(dep => dep.Name.Equals(GlobalConstants.FlowBloxUICoreAssemblyName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error("Could not evaluate UI framework dependencies.", ex);
                return false;
            }
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            try
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                {
                    return LoadFromAssemblyPath(assemblyPath);
                }
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Error($"Failed to load assembly: {assemblyName}", e);
            }
            return null;
        }
    }
}