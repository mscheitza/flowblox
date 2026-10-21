using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class LoadExplanationContentHandler : ToolHandlerBase
    {
        public override string Name => "LoadExplanationContent";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Loads on-demand explanation content into subsequent system messages. Preloaded or previously loaded content returns Already loaded; no content is returned.",
            new JObject
            {
                ["key"] = "string"
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

            if (entry.IsIncludedInInitialPrompt)
            {
                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["key"] = entry.Key,
                    ["loaded"] = false,
                    ["message"] = $"Already loaded: '{entry.Title}' is part of the initial system guidance."
                }));
            }

            var loaded = ExplanationSessionStore.Load(ToolHandlerUtilities.CurrentSessionGuid, entry.Key);
            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["key"] = entry.Key,
                ["loaded"] = loaded,
                ["message"] = loaded
                    ? $"'{entry.Title}' was loaded and will be available as a system message from the next request."
                    : $"Already loaded: '{entry.Title}' is available as a system message in this session."
            }));
        }
    }
}
