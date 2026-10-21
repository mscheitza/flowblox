using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler;

internal sealed class GetExplanationContentHandler : ToolHandlerBase
{
    public override string Name => "GetExplanationContent";
    public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns explanation content by key. Repeated retrieval automatically binds the content as a cache-preferred session system message; subsequent calls return only a notice.",
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
                ["message"] = $"Already loaded: '{entry.Title}' is part of the initial system guidance."
            }));

        return Task.FromResult(ToolHandlerUtilities.CurrentContentCache.GetExplanation(
            entry.Key,
            entry.Title,
            entry.Content));
    }
}