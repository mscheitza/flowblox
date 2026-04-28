using FlowBlox.AppWindow.Contents;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class AIAssistantViewPanelFactory : DockContentFactoryBase<AIAssistantView>
    {
        public AIAssistantViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override AIAssistantView Create()
        {
            var dockContent = new AIAssistantView();
            var key = typeof(AIAssistantView).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockRight,
                Visible = true
            };
        }
    }
}
