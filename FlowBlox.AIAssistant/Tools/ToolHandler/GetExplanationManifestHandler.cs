using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetExplanationManifestHandler : ToolHandlerBase
    {
        public override string Name => "GetExplanationManifest";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns available explanation/prompt content entries with stable keys and content hashes.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var items = new JArray(
                AssistantPromptCatalog.GetAllEntries()
                    .Select(x => new JObject
                    {
                        ["key"] = x.Key,
                        ["title"] = x.Title,
                        ["hint"] = x.Hint,
                        ["isIncludedInInitialPrompt"] = x.IsIncludedInInitialPrompt,
                        ["contentHash"] = x.ContentHash,
                        ["contentLength"] = x.Content.Length
                    }));

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["items"] = items
            }));
        }
    }
}
