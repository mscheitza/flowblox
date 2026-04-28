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
        public event EventHandler<FlowBlocksConnectionsChangedEventArgs>? FlowBlocksConnectionsChanged;

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

            return ExecuteHandlerAndNotifyChanges(handler, request, ct);
        }

        private async Task<ToolResponse> ExecuteHandlerAndNotifyChanges(IToolHandler handler, ToolRequest request, CancellationToken ct)
        {
            var response = await handler
                .HandleAsync(request.Arguments ?? new JObject(), ct)
                .ConfigureAwait(false);

            TryNotifyFlowBlockConnectionsChanged(response);
            return response;
        }

        private void TryNotifyFlowBlockConnectionsChanged(ToolResponse response)
        {
            if (response?.Ok != true || response.Result == null)
                return;

            var modeToken = response.Result.Value<string>("mode");
            var from = response.Result.Value<string>("from");
            var to = response.Result.Value<string>("to");

            if (string.IsNullOrWhiteSpace(modeToken) ||
                string.IsNullOrWhiteSpace(from) ||
                string.IsNullOrWhiteSpace(to))
            {
                return;
            }

            FlowBlockConnectionChangeMode? changeMode = null;
            var changed = false;

            if (response.Result.Value<bool?>("connected") == true)
            {
                changed = response.Result.Value<bool?>("changed") ?? true;
                changeMode = FlowBlockConnectionChangeMode.Connect;
            }
            else if (response.Result.Value<bool?>("disconnected") == true)
            {
                changed = response.Result.Value<bool?>("wasConnected") == true;
                changeMode = FlowBlockConnectionChangeMode.Disconnect;
            }

            if (!changed || changeMode == null)
                return;

            FlowBlocksConnectionsChanged?.Invoke(
                this,
                new FlowBlocksConnectionsChangedEventArgs
                {
                    Changes =
                    [
                        new FlowBlockConnectionChange
                        {
                            From = from,
                            To = to,
                            LinkMode = modeToken,
                            Mode = changeMode.Value
                        }
                    ]
                });
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
                new AutoAdjustFlowLayoutHandler(),
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

