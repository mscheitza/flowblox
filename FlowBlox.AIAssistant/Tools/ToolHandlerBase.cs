using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal abstract class ToolHandlerBase : IToolHandler
    {
        public abstract string Name { get; }
        public abstract ToolDefinition Definition { get; }
        public abstract Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct);
    }
}
