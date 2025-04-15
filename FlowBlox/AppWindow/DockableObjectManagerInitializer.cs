using FlowBlox.AppWindow.ContentFactories;
using FlowBlox.AppWindow.Util;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Views.PropertyView;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow
{
    internal class DockableObjectManagerInitializer
    {
        private DockPanel _dockPanel;
        private FlowBloxRegistry _registry;
        private readonly List<DockContentEntry> _dockEntries;

        public DockableObjectManagerInitializer(DockPanel dockPanel)
        {
            this._dockPanel = dockPanel;
            this._registry = FlowBloxRegistryProvider.GetRegistry();
            this._dockEntries = new List<DockContentEntry>();
        }

        public void InitializeAllObjectManager()
        {
            var currentDomain = AppDomain.CurrentDomain;
            var assemblies = currentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.GetInterfaces().Contains(typeof(IDockableObjectManager)) && !type.IsAbstract)
                    {
                        var displayNameAttribute = (DisplayAttribute)type.GetCustomAttribute(typeof(DisplayAttribute), false);
                        var displayName = FlowBloxResourceUtil.GetDisplayName(displayNameAttribute);

                        var objectManager = (IDockableObjectManager)Activator.CreateInstance(type);
                        if (objectManager.IsActive)
                            CreateAndAddDockContent(type, displayName);
                    }
                }
            }

            RegisterRegistryEvents();
        }

        private void RegisterRegistryEvents()
        {
            _registry.OnManagedObjectAdded -= FlowBloxRegistry_OnManagedObjectAdded;
            _registry.OnManagedObjectAdded += FlowBloxRegistry_OnManagedObjectAdded;

            _registry.OnManagedObjectRemoved -= FlowBloxRegistry_OnManagedObjectRemoved;
            _registry.OnManagedObjectRemoved += FlowBloxRegistry_OnManagedObjectRemoved;
        }

        private void FlowBloxRegistry_OnManagedObjectAdded(Core.Events.ManagedObjectAddedEventArgs eventArgs)
        {
            Reload();
        }

        private void FlowBloxRegistry_OnManagedObjectRemoved(Core.Events.ManagedObjectRemovedEventArgs eventArgs)
        {
            Reload();
        }

        private void CreateAndAddDockContent(Type type, string displayName)
        {
            var dynamicFactory = new ObjectManagerDockContentFactory(_dockPanel, type, displayName);
            _dockEntries.Add(new DockContentEntry()
            {
                Content = dynamicFactory.Create(),
                Factory = dynamicFactory
            });
        }

        public void Recreate()
        {
            _dockPanel.SuspendLayout(true);
            var activeContents = PaneActiveContentHelper.CapturePaneActiveContents(_dockPanel);
            _dockEntries.ForEach(entry => entry.Recreate());
            PaneActiveContentHelper.RestorePaneActiveContents(_dockPanel, activeContents);
            _dockPanel.ResumeLayout(false);
        }

        public void Reload()
        {
            foreach(var propertyViewTabControl in _dockEntries.Select(x => x.Content)
                .SelectMany(x => x.Controls.Cast<Control>())
                .OfType<PropertyViewTabControl>())
            {
                if (propertyViewTabControl.Target is IDockableObjectManager objectManager)
                {
                    objectManager.Reload();
                }
                foreach (var propertyView in propertyViewTabControl.GetAssociatedPropertyViews())
                {
                    foreach (var factory in propertyView.GetAssociatedFactories())
                    {
                        factory.Reload();
                    }
                }
            }
        }
    }
}
