using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Loader;

namespace FlowBlox.Core.DependencyInjection
{
    public class FlowBloxServiceLocator
    {
        private static readonly Lazy<FlowBloxServiceLocator> _instance = new Lazy<FlowBloxServiceLocator>(() => new FlowBloxServiceLocator());
        private readonly ServiceCollection _serviceCollection;
        private ServiceProvider _serviceProvider;

        private FlowBloxServiceLocator()
        {
            _serviceCollection = new ServiceCollection();

            RegisterServices();
        }

        private void RegisterServices()
        {
            var typesToRegister = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IFlowBloxServiceRegistration).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToList();

            foreach (var type in typesToRegister)
            {
                IFlowBloxServiceRegistration registrationInstance = (IFlowBloxServiceRegistration)Activator.CreateInstance(type);
                registrationInstance?.RegisterServices(_serviceCollection);
            }
        }

        public void RegisterServices(AssemblyLoadContext loadContext)
        {
            foreach (var assembly in loadContext.Assemblies)
            {
                var registrationTypes = assembly.GetTypes()
                    .Where(t => !t.IsInterface && !t.IsAbstract)
                    .Where(t => typeof(IFlowBloxServiceRegistration).IsAssignableFrom(t));

                foreach (var type in registrationTypes)
                {
                    var registrationInstance = (IFlowBloxServiceRegistration)Activator.CreateInstance(type);
                    registrationInstance?.RegisterServices(_serviceCollection);
                }
            }

            _serviceProvider?.Dispose();
            _serviceProvider = null;
        }

        public void UnregisterServices(AssemblyLoadContext loadContext)
        {
            var assembliesInContext = loadContext.Assemblies.ToHashSet();
            var descriptorsToRemove = _serviceCollection
                .Where(sd =>
                    sd.ImplementationType != null &&
                    assembliesInContext.Contains(sd.ImplementationType.Assembly))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                _serviceCollection.Remove(descriptor);
            }

            _serviceProvider?.Dispose();
            _serviceProvider = null;
        }

        public static FlowBloxServiceLocator Instance => _instance.Value;

        public T GetService<T>()
        {
            if (_serviceProvider == null)
                _serviceProvider = _serviceCollection.BuildServiceProvider();
            return _serviceProvider.GetService<T>();
        }

        public IEnumerable<T> GetServices<T>()
        {
            if (_serviceProvider == null)
                _serviceProvider = _serviceCollection.BuildServiceProvider();
            return _serviceProvider.GetServices<T>();
        }
    }
}
