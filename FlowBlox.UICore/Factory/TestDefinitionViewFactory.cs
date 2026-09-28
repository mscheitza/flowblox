using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Views;
using System.Windows;
using TestDefinitionViewFactoryResources = FlowBlox.UICore.Resources.ClassResources.TestDefinitionViewFactory;

namespace FlowBlox.UICore.Factory
{
    public class TestDefinitionViewFactory : IPropertyWindowViewFactory
    {
        private readonly Lazy<FlowBlox.Core.Provider.Registry.FlowBloxRegistry> _registry = new(FlowBloxRegistryProvider.GetRegistry);

        public Window Create(object instance, object target, bool readOnly)
        {
            return new TestDefinitionView((FlowBloxTestDefinition)instance, (BaseFlowBlock)target);
        }

        public bool CanCreate(object instance, object target, bool readOnly, out string message)
        {
            if (_registry.Value?.GetStartFlowBlock() != null)
            {
                message = null;
                return true;
            }

            message = FlowBloxResourceUtil.GetLocalizedString(
                nameof(TestDefinitionViewFactoryResources.StartFlowBlockRequired),
                typeof(TestDefinitionViewFactoryResources));
            return false;
        }

        public bool SupportsType(Type type)
        {
            return typeof(FlowBloxTestDefinition).IsAssignableFrom(type);
        }
    }
}
