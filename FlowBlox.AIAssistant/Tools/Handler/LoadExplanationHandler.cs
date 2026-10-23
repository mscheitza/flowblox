using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler;

internal sealed class LoadExplanationHandler : ToolHandlerBase
{
    public override string Name => "LoadExplanation";
    public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
        Name,
        "Loads on-demand explanation content into subsequent system messages. Preloaded or previously loaded content is not duplicated.",
        new JObject
        {
            ["key"] = "string"
        });

    public override Task<ToolResponse> HandleAsync(
        JObject args,
        CancellationToken ct)
    {
        var key = args.Value<string>("key");
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

        if (!AssistantPromptCatalog.TryGetEntry(key, out var entry) || entry == null)
            return Task.FromResult(ToolHandlerUtilities.Fail(
                $"Explanation entry '{key}' was not found.",
                new JObject
                {
                    ["availableKeys"] = new JArray(AssistantPromptCatalog.GetAllEntries()
                        .Select(x => x.Key))
                }));

        if (entry.IsIncludedInInitialPrompt)
            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["key"] = entry.Key,
                ["loaded"] = false,
                ["message"] = $"Already loaded: '{entry.Title}' is part of the initial system guidance."
            }));

        return Task.FromResult(ToolHandlerUtilities.CurrentContentCache.LoadExplanation(
            entry.Key,
            entry.Title,
            entry.Content));
    }
}
