using System.Reflection;

namespace FlowBlox.Core.Constants
{
    public static class GlobalUrls
    {
        public const string FlowBloxWebsite = "https://www.flowblox.net/";
        public const string FlowBloxGitHubRepository = "https://github.com/mscheitza/flowblox";
        public const string FlowBloxSampleExtensionRepository = "https://github.com/mscheitza/flowblox/tree/main/FlowBloxSampleExtension";
        public const string FlowBloxSampleExtensionUIRepository = "https://github.com/mscheitza/flowblox/tree/main/FlowBloxSampleExtension.UI";
        public const string FlowBloxReportProblem = "https://www.flowblox.net/reportproblem";
        public const string FlowBloxPublicApiBaseUrl = "https://www.flowblox.net/api/";
        public const string FlowBloxInstallerManifestUrl = "https://flowblox.net/app/FlowBloxInstallerUpdates.xml";

        public static IReadOnlyDictionary<string, string> GetAll()
        {
            return typeof(GlobalUrls)
                .GetFields(BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(x => x.IsLiteral && !x.IsInitOnly && x.FieldType == typeof(string))
                .ToDictionary(
                    x => x.Name,
                    x => (string)x.GetRawConstantValue()!,
                    StringComparer.Ordinal);
        }
    }
}