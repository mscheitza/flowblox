using FlowBlox.Core.Actions;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Provider;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Grid.Provider;
using System.Windows.Forms;

namespace FlowBlox.Actions
{
    public class FlowBloxDeleteAction : FlowBloxBaseAction
    {
        private readonly FlowBloxProjectComponentProvider _componentProvider;

        public FlowBloxDeleteAction()
        {
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
        }

        public Panel MainPanel { get; set; }

        public FlowBlockUIElement UIElement { get; set; }

        public override void Undo()
        {
            MainPanel.Controls.Add(UIElement);
            _componentProvider.GetCurrentUIRegistry().RegisterGridUIElement(UIElement);
            FlowBloxRegistryProvider.GetRegistry().RegisterFlowBlock(UIElement.InternalFlowBlock);
            base.Undo();
        }

        public override void Invoke()
        {
            MainPanel.Controls.Remove(UIElement);
            _componentProvider.GetCurrentUIRegistry().RemoveUIElement(UIElement);
            FlowBloxRegistryProvider.GetRegistry().RemoveFlowBlock(UIElement.InternalFlowBlock);
            base.Invoke();
        }
    }
}
