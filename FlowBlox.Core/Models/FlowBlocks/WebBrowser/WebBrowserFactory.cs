namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public class WebBrowserFactory
    {
        public static WebBrowserBase Create(string serviceName)
        {
            return Create(serviceName, null);
        }

        public static WebBrowserBase Create(string serviceName, string serializedSettings)
        {
            var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    string.Equals(t.Name, serviceName, StringComparison.OrdinalIgnoreCase) &&
                    typeof(WebBrowserBase).IsAssignableFrom(t));

            if (serviceType == null)
                throw new ArgumentException($"Service '{serviceName}' not found.");

            // Try to find a constructor that accepts serializedSettings: string
            if (!string.IsNullOrEmpty(serializedSettings))
            {
                var ctorWithSettings = serviceType.GetConstructor([typeof(string)]);
                if (ctorWithSettings != null)
                {
                    var instanceWithSettings = ctorWithSettings.Invoke([serializedSettings]);
                    return (WebBrowserBase)instanceWithSettings;
                }
            }

            // Fallback to parameterless constructor
            var instance = Activator.CreateInstance(serviceType);
            return (WebBrowserBase)instance;
        }
    }
}
