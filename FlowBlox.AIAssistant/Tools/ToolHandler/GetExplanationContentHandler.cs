using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetExplanationContentHandler : ToolHandlerBase
    {
        public override string Name => "GetExplanationContent";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns explanation/prompt content by key. If knownHash matches, returns unchanged=true without content.",
            new JObject
            {
                ["key"] = "string",
                ["knownHash"] = "string?"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var key = args.Value<string>("key");
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

            if (!AssistantPromptCatalog.TryGetEntry(key, out var entry) || entry == null)
            {
                var availableKeys = new JArray(AssistantPromptCatalog.GetAllEntries().Select(x => x.Key));
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"Explanation entry '{key}' was not found.",
                    new JObject { ["availableKeys"] = availableKeys }));
            }

            var knownHash = args.Value<string>("knownHash");
            if (!string.IsNullOrWhiteSpace(knownHash)
                && string.Equals(knownHash, entry.ContentHash, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["key"] = entry.Key,
                    ["title"] = entry.Title,
                    ["hint"] = entry.Hint,
                    ["isIncludedInInitialPrompt"] = entry.IsIncludedInInitialPrompt,
                    ["contentHash"] = entry.ContentHash,
                    ["unchanged"] = true
                }));
            }

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["key"] = entry.Key,
                ["title"] = entry.Title,
                ["hint"] = entry.Hint,
                ["isIncludedInInitialPrompt"] = entry.IsIncludedInInitialPrompt,
                ["contentHash"] = entry.ContentHash,
                ["unchanged"] = false,
                ["content"] = entry.Content
            }));
        }
    }
}
