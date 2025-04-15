using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlowBlox.UICore.Interfaces;

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
