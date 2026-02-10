using System.Runtime.Loader;

namespace FlowBlox.Core.Factories
{
    public static class AppDomainInstanceFactory
    {
        public static IEnumerable<T> CreateInstances<T>(Func<Type, bool> typeFilter = null)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .Where(type => typeFilter == null || typeFilter(type))
                .Select(Activator.CreateInstance)
                .Cast<T>();
        }

        public static IEnumerable<T> CreateInstances<T>(IEnumerable<AssemblyLoadContext> contexts, Func<Type, bool> typeFilter = null)
        {
            var instances = new List<T>();

            foreach (var context in contexts)
            {
                var objects = context.Assemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(x => CombineFilters(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract, typeFilter).Invoke(x))
                    .Select(Activator.CreateInstance)
                    .Cast<T>();

                instances.AddRange(objects);
            }

            var additionalInstances = CreateInstances<T>(CombineFilters(type => !instances.Any(x => x.GetType() == type), typeFilter));
            instances.AddRange(additionalInstances);
            return instances;
        }

        private static Func<Type, bool> CombineFilters(Func<Type, bool> filter1, Func<Type, bool> filter2)
        {
            return type =>
            {
                if (!filter1(type))
                    return false;

                return filter2 == null || filter2(type);
            };
        }
    }
}