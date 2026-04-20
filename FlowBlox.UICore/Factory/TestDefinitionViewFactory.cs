using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Views;
using System.Windows;
using FlowBlox.UICore.Interfaces;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.UICore.Factory
{
    public class TestDefinitionViewFactory : IPropertyWindowViewFactory
    {
        public Window Create(object instance, object target, bool readOnly)
        {
            return new TestDefinitionView((FlowBloxTestDefinition)instance, (BaseFlowBlock)target);
        }

        public bool SupportsType(Type type)
        {
            return typeof(FlowBloxTestDefinition).IsAssignableFrom(type);
        }
    }
}
