using System.Reflection;

namespace FlowBlox.Core.Constants
{
    public class GlobalPaths
    {
        public static string CurrentPath => Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
        public static string CurrentDirectory => Path.GetDirectoryName(CurrentPath);
        public static string GlobalToolboxDirectory => Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "FlowBlox", "toolbox");
        public static string GlobalConfigurationXml => Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "FlowBlox", "global_configuration.xml");
    }
}
