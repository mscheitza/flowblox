using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.Loader;

namespace FlowBlox.Core.DependencyInjection
{
    public class FlowBloxServiceLocator
    {
        private static readonly Lazy<FlowBloxServiceLocator> _instance = new Lazy<FlowBloxServiceLocator>(() => new FlowBloxServiceLocator());
        private readonly ServiceCollection _serviceCollection;
        private readonly HashSet<Type> _registeredServiceRegistrationTypes;
        private ServiceProvider _serviceProvider;

        private FlowBloxServiceLocator()
        {
            _serviceCollection = new ServiceCollection();
            _registeredServiceRegistrationTypes = new HashSet<Type>();

            RegisterServicesFromCurrentAppDomain();
        }

        public void RegisterServicesFromCurrentAppDomain()
        {
            var typesToRegister = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(IsServiceRegistrationType)
                .ToList();

            RegisterServiceRegistrationTypes(typesToRegister);
        }

        public void RegisterServices(AssemblyLoadContext loadContext)
        {
            var registrationTypes = loadContext.Assemblies
                .SelectMany(GetLoadableTypes)
                .Where(IsServiceRegistrationType)
                .ToList();

            RegisterServiceRegistrationTypes(registrationTypes);
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

            var registrationTypesToRemove = assembliesInContext
                .SelectMany(GetLoadableTypes)
                .Where(IsServiceRegistrationType)
                .ToList();

            foreach (var registrationType in registrationTypesToRemove)
            {
                _registeredServiceRegistrationTypes.Remove(registrationType);
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

        private void RegisterServiceRegistrationTypes(IEnumerable<Type> registrationTypes)
        {
            var registeredAny = false;

            foreach (var type in registrationTypes)
            {
                if (!_registeredServiceRegistrationTypes.Add(type))
                    continue;

                var registrationInstance = (IFlowBloxServiceRegistration)Activator.CreateInstance(type);
                registrationInstance?.RegisterServices(_serviceCollection);
                registeredAny = true;
            }

            if (registeredAny)
            {
                _serviceProvider?.Dispose();
                _serviceProvider = null;
            }
        }

        private static bool IsServiceRegistrationType(Type type)
        {
            return typeof(IFlowBloxServiceRegistration).IsAssignableFrom(type)
                && !type.IsInterface
                && !type.IsAbstract;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }
    }
}
