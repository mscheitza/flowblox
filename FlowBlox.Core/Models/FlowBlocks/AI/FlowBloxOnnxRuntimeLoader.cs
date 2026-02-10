using FlowBlox.Core.Enums;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Runtime;
using System.Runtime.InteropServices;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    /// <summary>
    /// Loads native ONNX Runtime binaries from:
    /// AppContext.BaseDirectory/data/onnxruntimes/<provider>/<rid>/
    ///
    /// Important:
    /// Once a provider is loaded, switching to another provider within the same process is forbidden.
    /// </summary>
    public sealed class FlowBloxOnnxRuntimeLoader
    {
        public static FlowBloxOnnxRuntimeLoader Instance { get; } = new();

        private readonly object _sync = new();

        private AiExecutionProviders? _loadedProvider;

        private FlowBloxOnnxRuntimeLoader() { }

        public void EnsureLoaded(AiExecutionProviders provider, BaseRuntime runtime)
        {
            lock (_sync)
            {
                if (_loadedProvider.HasValue)
                {
                    // Prevent provider switching within the same process
                    if (_loadedProvider.Value != provider)
                    {
                        throw new InvalidOperationException(
                            $"ONNX Runtime provider already loaded: '{_loadedProvider.Value}'. " +
                            $"Switching to '{provider}' is not supported in the same process.");
                    }

                    return;
                }

                var rid = GetRid();
                var providerFolder = GetProviderFolder(provider);

                var nativeDir = Path.Combine(
                    AppContext.BaseDirectory,
                    "data",
                    "onnxruntimes",
                    providerFolder,
                    rid);

                if (!Directory.Exists(nativeDir))
                    throw new DirectoryNotFoundException($"ONNX runtime directory not found: {nativeDir}");

                runtime.Report($"Loading ONNX native runtime from: {nativeDir}");

                // Load onnxruntime first (platform-specific file name)
                var ortPath = Path.Combine(nativeDir, GetOnnxRuntimeLibraryFileName());
                if (!File.Exists(ortPath))
                {
                    throw new FileNotFoundException(
                        $"Core ONNX Runtime library not found: {GetOnnxRuntimeLibraryFileName()}", ortPath);
                }

                NativeLibrary.Load(ortPath);

                // Best-effort preload remaining native libraries in the folder.
                foreach (var file in EnumerateNativeLibraries(nativeDir))
                {
                    // Skip the core library (already loaded)
                    if (Path.GetFullPath(file).Equals(Path.GetFullPath(ortPath), StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        NativeLibrary.Load(file);
                    }
                    catch (Exception ex)
                    {
                        FlowBloxLogManager.Instance.GetLogger().Error($"Failed to load optional native library '{Path.GetFileName(file)}'.", ex);

                        // Intentionally ignored:
                        // Some native libraries may be optional or depend on system components.
                        // If a required dependency is missing, execution provider initialization
                        // will fail later with a meaningful exception.
                    }
                }

                _loadedProvider = provider;
                runtime.Report($"ONNX native runtime loaded for provider: {provider} ({rid})");
            }
        }

        private static IEnumerable<string> EnumerateNativeLibraries(string folder)
        {
            // Windows: *.dll, Linux: *.so (and *.so.*), macOS: *.dylib
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Directory.EnumerateFiles(folder, "*.dll");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Directory.EnumerateFiles(folder, "*.so*");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Directory.EnumerateFiles(folder, "*.dylib");

            // Fallback: try common extensions
            return Directory.EnumerateFiles(folder, "*.*")
                .Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase)
                         || f.Contains(".so", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetOnnxRuntimeLibraryFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "onnxruntime.dll";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "libonnxruntime.so";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "libonnxruntime.dylib";

            // Fallback: most likely Windows naming
            return "onnxruntime.dll";
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
            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Environment.Is64BitProcess)
                    return "win-x86";

                // If you want to support Windows ARM64 later:
                // if (RuntimeInformation.OSArchitecture == Architecture.Arm64) return "win-arm64";
                return "win-x64";
            }

            // Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Most realistic target for server setups
                return Environment.Is64BitProcess ? "linux-x64" : "linux-x86";
            }

            // macOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Typically arm64 nowadays, but keep it simple unless you package both
                return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
            }

            // Fallback
            return Environment.Is64BitProcess ? "win-x64" : "win-x86";
        }
    }
}