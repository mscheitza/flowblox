using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.Util;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetCategoryChildrenHandler : ToolHandlerBase
    {
        public override string Name => "GetCategoryChildren";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns child categories and flow block kinds for categoryPath, including displayName and description metadata for flow block kinds.",
            new JObject
            {
                ["categoryPath"] = "string[]"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var categoryPath = args["categoryPath"]?.ToObject<string[]>() ?? Array.Empty<string>();

            var category = FlowBlockCategory.GetAll()
                .FirstOrDefault(x => ToolHandlerUtilities.PathOf(x).SequenceEqual(categoryPath));

            if (category == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail("Category path could not be resolved."));
            }

            var childCategories = FlowBlockCategory.GetAll()
                .Where(x => x.ParentCategory == category)
                .OrderBy(x => x.DisplayName)
                .Select(x => new JObject
                {
                    ["displayName"] = x.DisplayName,
                    ["description"] = string.Empty,
                    ["path"] = new JArray(ToolHandlerUtilities.PathOf(x))
                });

            var kinds = ToolHandlerUtilities.GetProject()
                .CreateInstances<BaseFlowBlock>()
                .GroupBy(x => x.GetType().FullName ?? x.GetType().Name, StringComparer.Ordinal)
                .Select(x => x.First())
                .Where(x => x.GetCategory().Equals(category))
                .OrderBy(FlowBloxComponentHelper.GetDisplayName)
                .Select(ToolHandlerUtilities.ToTypeInfo);

            var payload = new JObject
            {
                ["category"] = category.DisplayName,
                ["childCategories"] = new JArray(childCategories),
                ["flowBlockKinds"] = new JArray(kinds)
            };

            return Task.FromResult(ToolHandlerUtilities.Ok(payload));
        }
    }
}
