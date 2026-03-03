using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    public class DefaultToolApi : IFlowBloxAIToolApi
    {
        private readonly Dictionary<string, IToolHandler> _handlers;

        public DefaultToolApi()
        {
            _handlers = new Dictionary<string, IToolHandler>(StringComparer.OrdinalIgnoreCase);
            Register(new StubToolHandler("GetProjectJson"));
            Register(new StubToolHandler("GetRootCategories"));
            Register(new StubToolHandler("GetCategoryChildren"));
            Register(new StubToolHandler("GetFlowBlockTypesFromCategory"));
            Register(new StubToolHandler("CreateDefaultFlowBlockJson"));
            Register(new StubToolHandler("GetManagedObjectDefaultFlowBlockJson"));
            Register(new StubToolHandler("GetManagedObjectKindsInfo"));
            Register(new StubToolHandler("GetFlowBlockBaseKindsInfo"));
        }

        public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.ToolName))
            {
                return Task.FromResult(new ToolResponse
                {
                    Ok = false,
                    Error = "ToolName is required."
                });
            }

            if (_handlers.TryGetValue(request.ToolName, out var handler))
                return handler.HandleAsync(request.Arguments ?? new JObject(), ct);

            return Task.FromResult(new ToolResponse
            {
                Ok = false,
                Error = $"Tool '{request.ToolName}' is not registered.",
                Log = new JArray($"CorrelationId={request.CorrelationId}")
            });
        }

        private void Register(IToolHandler handler)
        {
            _handlers[handler.Name] = handler;
        }

        private sealed class StubToolHandler : IToolHandler
        {
            public string Name { get; }

            public StubToolHandler(string name)
            {
                Name = name;
            }

            public Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
            {
                return Task.FromResult(new ToolResponse
                {
                    Ok = false,
                    Error = $"Tool '{Name}' is not implemented in MVP.",
                    Result = new JObject
                    {
                        ["tool"] = Name,
                        ["implemented"] = false
                    },
                    Log = new JArray("stub")
                });
            }
        }
    }
}
