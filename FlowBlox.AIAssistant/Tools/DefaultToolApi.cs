using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    public class DefaultToolApi : IFlowBloxAIToolApi
    {
        private const string ExecuteInputFileCommandToolName = "ExecuteInputFileCommand";
        private readonly Dictionary<string, IToolHandler> _handlers;
        private readonly List<ToolDefinition> _definitions;
        public Func<ToolRequest, bool>? ToolExecutionConfirmationCallback { get; set; }

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

            if (RequiresUserConfirmation(request) && !IsToolExecutionApproved(request))
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"Execution for tool '{request.ToolName}' was cancelled by user confirmation.",
                    new JObject
                    {
                        ["cancelledByUser"] = true,
                        ["toolName"] = request.ToolName
                    }));
            }

            return handler.HandleAsync(request.Arguments ?? new JObject(), ct);
        }

        private static bool RequiresUserConfirmation(ToolRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ToolName))
                return false;

            return string.Equals(
                request.ToolName,
                ExecuteInputFileCommandToolName,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsToolExecutionApproved(ToolRequest request)
        {
            try
            {
                return ToolExecutionConfirmationCallback?.Invoke(request) == true;
            }
            catch
            {
                return false;
            }
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
                new SearchFlowBlockHandler(),
                new GetComponentSnapshotHandler(),
                new CreateFlowBlockHandler(),
                new UpdateFlowBlockHandler(),
                new DeleteFlowBlockHandler(),
                new ConnectFlowBlocksHandler(),
                new DisconnectFlowBlocksHandler(),
                new CreateManagedObjectHandler(),
                new CreateUserFieldHandler(),
                new UpdateManagedObjectHandler(),
                new DeleteManagedObjectHandler(),
                new GetManagedObjectsTypesHandler(),
                new GetSupportedTypesHandler(),
                new GetTypeKindsInfoHandler(),
                new GetManagedObjectKindsInfoHandler(),
                new ResolveAssociatedFlowBlockHandler(),
                new GetPlaceholdersHandler(),
                new GetToolboxEntriesHandler(),
                new GetInputFilesHandler(),
                new GetInputFileContentHandler(),
                new CreateFieldHandler(),
                new CreateOrUpdateInputFileHandler(),
                new ExecuteInputFileCommandHandler(),
                new DeleteInputFileHandler(),
                new RunProjectDebugTestHandler(),
                new RunTestDefinitionHandler(),
                new RunGenerationStrategiesHandler(),
                new GetLastDebugArtefactHandler(),
                new BatchExecuteToolRequestsHandler(ExecuteAsync)
            ];
        }
    }
}

