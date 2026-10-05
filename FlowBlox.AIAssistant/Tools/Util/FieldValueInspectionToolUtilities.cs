using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class FieldValueInspectionToolUtilities
    {
        public static bool TryCreateOptions(
            JObject args,
            out FieldValueInspectionOptions options,
            out string error)
        {
            var searchMode = (args.Value<string>("searchMode") ?? "StartAt").Trim();
            if (!string.Equals(searchMode, "StartAt", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(searchMode, "LookAround", StringComparison.OrdinalIgnoreCase))
            {
                options = new FieldValueInspectionOptions();
                error = "searchMode must be one of: StartAt, LookAround.";
                return false;
            }

            options = new FieldValueInspectionOptions
            {
                SearchIndex = args.Value<int?>("searchIndex"),
                SearchValues = args.Value<string>("searchValues"),
                SearchMode = searchMode
            };
            error = string.Empty;
            return true;
        }
    }
}
