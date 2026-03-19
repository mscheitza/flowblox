using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    public class DefaultToolApi : IFlowBloxAIToolApi
    {
        private readonly Dictionary<string, IToolHandler> _handlers;
        private readonly List<ToolDefinition> _definitions;

        public DefaultToolApi()
        {
            var handlers = CreateHandlers();

            _handlers = handlers.ToDictionary(
                handler => handler.Name,
                handler => handler,
                StringComparer.OrdinalIgnoreCase);

            _definitions = handlers
                .Select(handler => handler.Definition)
                .ToList();
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions()
        {
            return _definitions;
        }

        public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.ToolName))
                return Task.FromResult(ToolHandlerUtilities.Fail("ToolName is required."));

            if (!_handlers.TryGetValue(request.ToolName, out var handler))
                return Task.FromResult(ToolHandlerUtilities.Fail($"Tool '{request.ToolName}' is not registered."));

            return handler.HandleAsync(request.Arguments ?? new JObject(), ct);
        }

        private List<IToolHandler> CreateHandlers()
        {
            return
            [
                new GetProjectJsonHandler(),
                new GetExplanationManifestHandler(),
                new GetExplanationContentHandler(),
                new GetRootCategoriesHandler(),
                new GetCategoryChildrenHandler(),
                new CreateFlowBlockHandler(),
                new UpdateFlowBlockHandler(),
                new DeleteFlowBlockHandler(),
                new ConnectFlowBlocksHandler(),
                new DisconnectFlowBlocksHandler(),
                new CreateManagedObjectHandler(),
                new UpdateManagedObjectHandler(),
                new DeleteManagedObjectHandler(),
                new GetManagedObjectsTypesHandler(),
                new GetSupportedTypesHandler(),
                new GetTypeKindsInfoHandler(),
                new GetManagedObjectKindsInfoHandler(),
                new GetPlaceholdersHandler(),
                new CreateFieldHandler(),
                new BatchExecuteToolRequestsHandler(ExecuteAsync)
            ];
        }
    }
}
