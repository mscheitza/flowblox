using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Handler
{
    internal sealed class GetTypeKindsInfoHandler : ToolHandlerBase
    {
        public override string Name => "GetTypeKindsInfo";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns compact type metadata for FlowBloxReactiveObject types and Enums. " +
            "Property flags: N=nullable, W=writable, S=simple, E=enum, C=collection. " +
            "UI flags: C=create, L=link, U=unlink, E=edit, D=delete, [FLD]=field, [FL]=file, [DI]=directory selection. " +
            "Attributes: comma-separated Name(arg;member=value).",
            new JObject
            {
                ["typeFullName"] = "string",
                ["includeSummarized"] = "optional: bool (default: false). Include summary-cached properties. Properties already loaded in the current message history stay omitted.",
                ["includeBaseMembers"] = "optional: bool (default: false). Include framework Base*FlowBlock members. Domain-base members are always included."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var includeSummarized = args.Value<bool?>("includeSummarized") ?? false;
            var includeBaseMembers = args.Value<bool?>("includeBaseMembers") ?? false;
            var response = ToolHandlerUtilities.CreateUnifiedTypeInfoResponse(
                args.Value<string>("typeFullName"),
                includeSummarized,
                includeBaseMembers);

            return Task.FromResult(response);
        }
    }
}
