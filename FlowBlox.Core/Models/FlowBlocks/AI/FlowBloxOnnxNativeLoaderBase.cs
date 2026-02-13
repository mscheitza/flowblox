using FlowBlox.Core.Enums;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Runtime;
using System.Runtime.InteropServices;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    public abstract class FlowBloxOnnxNativeLoaderBase
    {
        private readonly object _sync = new();

        private AiExecutionProviders? _loadedProvider;

        protected abstract string GetOnnxRuntimeLibraryFileName();

        protected abstract string DataSubFolder { get; }
        protected abstract string LoaderLabel { get; }

        public void EnsureLoaded(AiExecutionProviders provider, BaseRuntime runtime)
        {
            lock (_sync)
            {
                if (_loadedProvider.HasValue)
                {
                    // Prevent provider switching within the same process (per loader)
                    if (_loadedProvider.Value != provider)
                    {
                        throw new InvalidOperationException(
                            $"{LoaderLabel} provider already loaded: '{_loadedProvider.Value}'. " +
                            $"Switching to '{provider}' is not supported in the same process.");
                    }

                    return;
                }

                var rid = GetRid();
                var providerFolder = GetProviderFolder(provider);

                LoadNativeFolder(
                    dataSubFolder: DataSubFolder,
                    providerFolder: providerFolder,
                    rid: rid,
                    runtime: runtime,
                    label: LoaderLabel);

                _loadedProvider = provider;
                runtime.Report($"{LoaderLabel} native runtime loaded for provider: {provider} ({rid})");
            }
        }

        protected void LoadNativeFolder(
            string dataSubFolder,
            string providerFolder,
            string rid,
            BaseRuntime runtime,
            string label)
        {
            var nativeDir = Path.Combine(
                AppContext.BaseDirectory,
                "data",
                dataSubFolder,
                providerFolder,
                rid);

            if (!Directory.Exists(nativeDir))
                throw new DirectoryNotFoundException($"{label} directory not found: {nativeDir}");

            runtime.Report($"Loading {label} native runtime from: {nativeDir}");

            // Load core onnxruntime first
            var ortFileName = GetOnnxRuntimeLibraryFileName();
            var ortPath = Path.Combine(nativeDir, ortFileName);
            if (!File.Exists(ortPath))
            {
                throw new FileNotFoundException(
                    $"Core ONNX Runtime library not found for {label}: {ortFileName}", ortPath);
            }
            NativeLibrary.Load(ortPath);

            // Best-effort preload remaining native libraries
            foreach (var file in EnumerateNativeLibraries(nativeDir))
            {
                if (Path.GetFullPath(file).Equals(Path.GetFullPath(ortPath), StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    NativeLibrary.Load(file);
                }
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger()
                        .Error($"Failed to load optional native library '{Path.GetFileName(file)}' from '{label}'.", ex);
                }
            }

            runtime.Report($"{label} native runtime loaded from: {nativeDir}");
        }

        private static IEnumerable<string> EnumerateNativeLibraries(string folder)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Directory.EnumerateFiles(folder, "*.dll");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Directory.EnumerateFiles(folder, "*.so*");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Directory.EnumerateFiles(folder, "*.dylib");

            return Directory.EnumerateFiles(folder, "*.*")
                .Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase)
                         || f.Contains(".so", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetProviderFolder(AiExecutionProviders provider)
        {
            return provider switch
            {
                AiExecutionProviders.CUDA => "gpu",
                AiExecutionProviders.DirectML => "directml",
                AiExecutionProviders.OpenVINO => "openvino",
                _ => "cpu"
            };
        }

        private static string GetRid()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Environment.Is64BitProcess)
                    return "win-x86";

                return "win-x64";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Environment.Is64BitProcess ? "linux-x64" : "linux-x86";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";

            return Environment.Is64BitProcess ? "win-x64" : "win-x86";
        }
    }
}
