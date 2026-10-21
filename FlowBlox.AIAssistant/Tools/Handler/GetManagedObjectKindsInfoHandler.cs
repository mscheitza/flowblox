using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class GetManagedObjectKindsInfoHandler : ToolHandlerBase
    {
        public override string Name => "GetManagedObjectKindsInfo";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns compact managed object metadata. " +
            "Property flags: N=nullable, W=writable, S=simple, E=enum, C=collection. " +
            "UI flags: C=create, L=link, U=unlink, E=edit, D=delete, [FLD]=field, [FL]=file, [DI]=directory selection. " +
            "Attributes: comma-separated Name(arg;member=value).",
            new JObject
            {
                ["typeFullName"] = "string (type full name, assembly-qualified names are supported)",
                ["includeSummarized"] = "optional: bool (default: false). Re-send full details from the summarized part of this session. Properties already sent in the current live context are never duplicated."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var includeSummarized = args.Value<bool?>("includeSummarized") ?? false;
            var baseResponse = ToolHandlerUtilities.CreateUnifiedTypeInfoResponse(
                args.Value<string>("typeFullName"),
                includeSummarized);

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
