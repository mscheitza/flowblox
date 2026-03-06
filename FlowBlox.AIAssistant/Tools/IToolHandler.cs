using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    public interface IToolHandler
    {
        string Name { get; }
        ToolDefinition Definition { get; }
        Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct);
    }
}
