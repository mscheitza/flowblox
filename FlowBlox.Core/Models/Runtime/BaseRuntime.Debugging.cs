using FlowBlox.Core.Models.Runtime.Debugging;
using System.Text.Json;

namespace FlowBlox.Core.Models.Runtime
{
    partial class BaseRuntime
    {
        private readonly object _debuggingLock = new object();
        private RuntimeDebuggingResult _debuggingResult;

        public void SetDebuggingResult(RuntimeDebuggingResult debuggingResult)
        {
            lock (_debuggingLock)
            {
                _debuggingResult = debuggingResult;
            }
        }

        public RuntimeDebuggingResult GetDebuggingResult()
        {
            lock (_debuggingLock)
            {
                return _debuggingResult;
            }
        }

        public string SerializeDebuggingResultToJson(bool pretty = false)
        {
            RuntimeDebuggingResult result;
            lock (_debuggingLock)
            {
                result = _debuggingResult;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = pretty
            };

            return JsonSerializer.Serialize(result, jsonOptions);
        }
    }
}
