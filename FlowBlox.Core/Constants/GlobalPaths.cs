using System.Reflection;

namespace FlowBlox.Core.Constants
{
    public class GlobalPaths
    {
        public static string CurrentPath => Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
        public static string CurrentDirectory => Path.GetDirectoryName(CurrentPath);
        public static string GlobalConfigurationXml => Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "FlowBlox", "global_configuration.xml");
        public static string RecentProjectsPath => Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "FlowBlox", "recentProjects.json");
    }
}
