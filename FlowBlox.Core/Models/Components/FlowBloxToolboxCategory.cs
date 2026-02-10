using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Models.Components
{
    public static class FlowBloxToolboxCategory
    {
        static FlowBloxToolboxCategory()
        {
            InvokeRegistration();
        }

        public const string Regex = nameof(Regex);
        public const string XPath = nameof(XPath);
        public const string CounterFormat = nameof(CounterFormat);
        public const string Format = nameof(Format);
        public const string SQL = nameof(SQL);
        public const string Filter = nameof(Filter);
        public const string DBConnection = nameof(DBConnection);
        public const string ChatTemplates = nameof(ChatTemplates);

        internal static void InvokeRegistration()
        {
            if (Directory.Exists(GlobalPaths.GlobalToolboxDirectory))
                Directory.Delete(GlobalPaths.GlobalToolboxDirectory, true);

            foreach (var service in FlowBloxServiceLocator.Instance.GetServices<IFlowBlockToolboxRegistrationService>())
            {
                service.Register();
            }
        }

        private static readonly HashSet<string> _registry = new HashSet<string>();

        public static void Register(string toolboxCategory)
        {
            _registry.AddIfNotExists(toolboxCategory);
        }

        public static IReadOnlyCollection<string> GetAll() => _registry;
    }
}