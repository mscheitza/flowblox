using System.Runtime.InteropServices;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    public sealed class FlowBloxOnnxRuntimeLoader : FlowBloxOnnxNativeLoaderBase
    {
        public static FlowBloxOnnxRuntimeLoader Instance { get; } = new();

        private FlowBloxOnnxRuntimeLoader() { }

        protected override string DataSubFolder => "onnxruntimes";
        protected override string LoaderLabel => "ONNX Runtime";

        protected override string GetOnnxRuntimeLibraryFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "onnxruntime.dll";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "libonnxruntime.so";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "libonnxruntime.dylib";

            return "onnxruntime.dll";
        }
    }
}
