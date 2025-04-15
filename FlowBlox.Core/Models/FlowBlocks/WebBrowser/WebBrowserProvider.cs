namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public class WebBrowserProvider
    {
        public static WebBrowserBase Create(string serviceName)
        {
            var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => string.Equals(t.Name, serviceName, StringComparison.OrdinalIgnoreCase) && typeof(WebBrowserBase).IsAssignableFrom(t));

            if (serviceType == null)
                throw new ArgumentException($"Service '{serviceName}' not found.");

            var instance = Activator.CreateInstance(serviceType);
            return (WebBrowserBase)instance;
        }
    }
}
