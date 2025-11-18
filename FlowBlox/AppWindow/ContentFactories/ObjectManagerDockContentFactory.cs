using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Views.PropertyView;
using System;
using System.Windows.Controls;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ObjectManagerDockContentFactory : DockContentFactoryBase<DockContent>
    {
        private readonly Type _type;
        private readonly string _displayName;

        public ObjectManagerDockContentFactory(WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel, Type type, string displayName) : base(dockPanel)
        {
            _type = type;
            _displayName = displayName;
        }

        public override DockContent Create()
        {
            var dockContent = new DockContent
            {
                Text = _displayName,
                Name = _displayName,
                DockAreas = DockAreas.DockBottom
            };

            // Factory-Methode zur Erstellung des PropertyViewTabControl
            Func<PropertyViewTabControl> factoryMethod = () => CreatePropertyViewTabControl(_type);
            var propertyViewTabControl = factoryMethod.Invoke();
            dockContent.Controls.Add(propertyViewTabControl);

            // Key basiert auf dem Type, um eindeutige Einstellungen zu gewährleisten
            var key = _type.FullName;
            return Create(key, dockContent);
        }

        private PropertyViewTabControl CreatePropertyViewTabControl(Type type)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            if (registry == null)
                throw new InvalidOperationException("FlowBloxRegistry cannot be null.");

            var instance = Activator.CreateInstance(type, [registry]) as IDockableObjectManager;

            var propertyViewTabControl = new PropertyViewTabControl(false)
            {
                Dock = DockStyle.Fill
            };

            propertyViewTabControl.Initialize(instance, AppWindow.Instance.IsRuntimeActive);
            return propertyViewTabControl;
        }
    }
}