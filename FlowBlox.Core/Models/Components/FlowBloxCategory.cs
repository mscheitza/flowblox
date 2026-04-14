using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using System.Runtime.Loader;

namespace FlowBlox.Core.Models.Components
{
    public sealed class FlowBlockCategory
    {
        public string DisplayName { get; }

        public FlowBlockCategory ParentCategory { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = DisplayName.GetHashCode();
                if (ParentCategory != null)
                    hash = (hash * 397) ^ ParentCategory.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is not FlowBlockCategory other)
                return false;

            return EqualsRecursive(this, other);
        }

        private static bool EqualsRecursive(FlowBlockCategory a, FlowBlockCategory b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (!string.Equals(a.DisplayName, b.DisplayName, StringComparison.Ordinal))
                return false;

            return EqualsRecursive(a.ParentCategory, b.ParentCategory);
        }

        public FlowBlockCategory(string displayName, FlowBlockCategory parentCategory = null)
        {
            DisplayName = displayName;
            ParentCategory = parentCategory;
        }

        private static readonly HashSet<FlowBlockCategory> _registry = new HashSet<FlowBlockCategory>();

        static FlowBlockCategory()
        {
            InvokeRegistration();
        }

        internal static void InvokeRegistration()
        {
            foreach (var service in FlowBloxServiceLocator.Instance.GetServices<IFlowBloxCategoryRegistrationService>())
            {
                service.Register();
            }
        }

        internal static void InvokeDeregistration(AssemblyLoadContext loadContext)
        {
            var assembliesInContext = loadContext.Assemblies.ToHashSet();
            foreach (var service in FlowBloxServiceLocator.Instance.GetServices<IFlowBloxCategoryRegistrationService>()
                .Where(service => assembliesInContext.Contains(service.GetType().Assembly)))
            {
                service.Unregister();
            }
        }

        private static string Get(string resourceKey) => FlowBloxResourceUtil.GetLocalizedString(resourceKey, typeof(FlowBloxTexts));

        public static readonly FlowBlockCategory Logic = new(Get("FloxBloxCategory_Logic"));
        public static readonly FlowBlockCategory Persistence = new(Get("FloxBloxCategory_Persistence"));
        public static readonly FlowBlockCategory TextOperations = new(Get("FloxBloxCategory_TextOperations"));
        public static readonly FlowBlockCategory Additional = new(Get("FloxBloxCategory_Additional"));
        public static readonly FlowBlockCategory Selection = new(Get("FloxBloxCategory_Selection"));
        public static readonly FlowBlockCategory Web = new(Get("FloxBloxCategory_Web"));
        public static readonly FlowBlockCategory IO = new(Get("FloxBloxCategory_IO"));
        public static readonly FlowBlockCategory Generation = new(Get("FloxBloxCategory_Generation"));
        public static readonly FlowBlockCategory ControlFlow = new(Get("FloxBloxCategory_ControlFlow"));
        public static readonly FlowBlockCategory Communication = new(Get("FloxBloxCategory_Communication"));
        public static readonly FlowBlockCategory Authorization = new(Get("FloxBloxCategory_Authorization"));
        public static readonly FlowBlockCategory Calculation = new(Get("FloxBloxCategory_Calculation"));
        public static readonly FlowBlockCategory DateOperations = new(Get("FloxBloxCategory_DateOperations"));
        public static readonly FlowBlockCategory AI = new(Get("FloxBloxCategory_AI"));
        public static readonly FlowBlockCategory ShellExecution = new(Get("FloxBloxCategory_ShellExecution"));
        public static readonly FlowBlockCategory Extensions = new(Get("FloxBloxCategory_Extensions"));
        public static readonly FlowBlockCategory Compression = new(Get("FloxBloxCategory_Compression"));

        // Serialization
        public static readonly FlowBlockCategory Serialization = new(Get("FloxBloxCategory_Serialization"));
        public static readonly FlowBlockCategory Xml = new(Get("FloxBloxCategory_Serialization_XML"), Serialization);
        public static readonly FlowBlockCategory Json = new(Get("FloxBloxCategory_Serialization_Json"), Serialization);

        public static void Register(FlowBlockCategory category)
        {
            _registry.AddIfNotExists(category);
        }

        public static void Unregister(FlowBlockCategory category)
        {
            _registry.Remove(category);
        }

        public static IReadOnlyCollection<FlowBlockCategory> GetAll() => _registry;
    }
}
