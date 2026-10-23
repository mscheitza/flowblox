using System.Collections.Concurrent;
using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.State;

// Runtime-only content. Repeated type retrieval promotes complete declaring types.
internal static class AssistantSessionContentStore
{
    private static readonly ConcurrentDictionary<string, AssistantSessionContentCache> Sessions = new(StringComparer.Ordinal);

    public static AssistantSessionContentCache Get(string sessionId) => Sessions.GetOrAdd(sessionId, _ => new());

    public static void Clear(string sessionId) => Sessions.TryRemove(sessionId, out _);

    public static DateTimeOffset ApplicationStartedAt { get; } = ReadApplicationStartedAt();

    private static DateTimeOffset ReadApplicationStartedAt()
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return process.StartTime.ToUniversalTime();
    }
}

internal sealed class AssistantSessionContentCache
{
    private readonly object _sync = new();
    private readonly HashSet<string> _listedDeclaringTypes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _promotedDeclaringTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AssistantSystemMessage> _bindings = new(StringComparer.Ordinal);

    public IReadOnlyList<AssistantSystemMessage> GetSystemMessages()
    {
        lock (_sync)
            return _bindings.Values.ToArray();
    }

    public bool IsTypePromoted(string fullName)
    {
        lock (_sync)
            return _promotedDeclaringTypes.Contains(fullName);
    }

    // Count each declaring type only once per handler call, even when several properties refer to it.
    // Rendering is separate and never changes these markers.
    public void RegisterDeclaringTypes(
        IEnumerable<string> declaringTypes,
        Func<string, AssistantSystemMessage> describePromotion)
    {
        lock (_sync)
        {
            var types = declaringTypes.Distinct(StringComparer.Ordinal)
                .Where(type => !_promotedDeclaringTypes.Contains(type))
                .ToArray();
            var firstRetrievals = types.Where(type => !_listedDeclaringTypes.Contains(type))
                .ToArray();
            var promote = types.Except(firstRetrievals, StringComparer.Ordinal)
                .ToArray();
            foreach (var type in firstRetrievals)
                _listedDeclaringTypes.Add(type);
            // All winning declarations are visible while rendering their inheritance references.
            foreach (var type in promote)
                _promotedDeclaringTypes.Add(type);

            try
            {
                var messages = promote.Select(describePromotion)
                    .ToArray();
                foreach (var message in messages)
                    _bindings.Add(message.Key, message);
            }
            catch
            {
                foreach (var type in promote)
                    _promotedDeclaringTypes.Remove(type);

                foreach (var type in firstRetrievals)
                    _listedDeclaringTypes.Remove(type);

                throw;
            }
        }
    }

    public ToolResponse LoadExplanation(
        string key,
        string title,
        string content)
    {
        lock (_sync)
        {
            var bindingKey = "explanation:" + key;
            if (_bindings.ContainsKey(bindingKey))
                return new()
                {
                    Ok = true,
                    Result = new JObject
                    {
                        ["key"] = key,
                        ["loaded"] = false,
                        ["message"] = $"Already loaded: '{title}' is available as a system message in this session."
                    }
                };

            _bindings.Add(bindingKey, new(
                bindingKey,
                title,
                AssistantSystemMessageKind.Explanation,
                content,
                true));
            return new()
            {
                Ok = true,
                Result = new JObject
                {
                    ["key"] = key,
                    ["loaded"] = true,
                    ["message"] = $"'{title}' was loaded and will be available as a system message from the next request."
                }
            };
        }
    }

    public static ToolResponse PromotionNotice(string typeOrKey) => new()
        {
            Ok = true,
            Result = new JObject
            {
                ["type"] = typeOrKey,
                ["message"] = "Provided as a runtime session system message because of repeated retrieval."
            }
        };
}
