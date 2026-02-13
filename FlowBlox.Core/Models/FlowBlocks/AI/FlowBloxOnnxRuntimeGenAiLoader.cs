using System.Runtime.InteropServices;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    public sealed class FlowBloxOnnxRuntimeGenAiLoader : FlowBloxOnnxNativeLoaderBase
    {
        public static FlowBloxOnnxRuntimeGenAiLoader Instance { get; } = new();

        private FlowBloxOnnxRuntimeGenAiLoader() { }

        protected override string DataSubFolder => "onnxruntimesgenai";
        protected override string LoaderLabel => "ONNX Runtime GenAI";

        protected override string GetOnnxRuntimeLibraryFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "onnxruntime-genai.dll";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "libonnxruntime-genai.so";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "libonnxruntime-genai.dylib";

            // Fallback (Windows-like)
            return "onnxruntime-genai.dll";
        }
    }
}