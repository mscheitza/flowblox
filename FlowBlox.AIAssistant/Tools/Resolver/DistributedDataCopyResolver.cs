using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Resolver
{
    internal static class DistributedDataCopyResolver
    {
        public static DistributedDataCopy Resolve(JObject request, string? requiredDataType = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            var dataType = (request.Value<string>("dataType") ?? string.Empty).Trim();
            var fileName = (request.Value<string>("fileName") ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(requiredDataType) &&
                !string.Equals(dataType, requiredDataType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"copyFromDistributedData.dataType must be '{requiredDataType}'.");
            }

            return new DistributedDataCopy(
                dataType,
                fileName,
                DistributedDataCatalog.ReadContent(dataType, fileName));
        }
    }

    internal sealed record DistributedDataCopy(string DataType, string FileName, string Content)
    {
        public string Source => $"distributedData:{DataType}/{FileName}";
    }
}
