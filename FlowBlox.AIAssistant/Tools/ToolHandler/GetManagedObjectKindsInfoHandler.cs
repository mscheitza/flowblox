using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetManagedObjectKindsInfoHandler : ToolHandlerBase
    {
        public override string Name => "GetManagedObjectKindsInfo";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns managed object kind metadata.",
            new JObject
            {
                ["typeFullName"] = "string (type full name, assembly-qualified names are supported)",
                ["includeAlreadySent"] = "optional: bool (default: false). If true, properties that were already described in this session are returned with full details again."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var includeAlreadySent = args.Value<bool?>("includeAlreadySent") ?? false;
            var baseResponse = ToolHandlerUtilities.CreateUnifiedTypeInfoResponse(
                args.Value<string>("typeFullName"),
                includeAlreadySent);

            if (!baseResponse.Ok)
            {
                return Task.FromResult(baseResponse);
            }

            var kindTypeFullName = baseResponse.Result?["kind"]?["fullName"]?.Value<string>();
            var resolvedType = ToolHandlerUtilities.ResolveType(kindTypeFullName);
            if (resolvedType == null || !typeof(IManagedObject).IsAssignableFrom(resolvedType))
            {
                return Task.FromResult(ToolHandlerUtilities.Fail($"Type '{args.Value<string>("typeFullName")}' is not a managed object type."));
            }

            return Task.FromResult(baseResponse);
        }
    }
}
