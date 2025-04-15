using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Factory;
using FlowBlox.UICore.Views;
using System;
using System.Linq;
using System.Windows;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Manager;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public static class WpfPropertyWindowProvider
    {
        public static bool CreatePropertyWindowAndShowDialog(Window owner, object target, object instance, bool readOnly)
        {
            var propertyWindowViewFactory = GetPropertyWindowViewFactoryForType(instance.GetType());
            if (propertyWindowViewFactory != null)
            {
                return InvokeWPFViewUsingTransaction(owner, instance, target, readOnly, propertyWindowViewFactory);
            }
            else
            {
                var propertyView = new PropertyWindow(new PropertyWindowArgs(instance, readOnly))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = owner
                };
                return propertyView.ShowDialog() == true;
            }
        }

        private static IPropertyWindowViewFactory GetPropertyWindowViewFactoryForType(Type instanceType)
        {
            var propertyWindowViewFactories = FlowBloxServiceLocator.Instance.GetServices<IPropertyWindowViewFactory>();
            return propertyWindowViewFactories.FirstOrDefault(x => x.SupportsType(instanceType));
        }

        private static bool InvokeWPFViewUsingTransaction(Window owner, object instance, object target, bool readOnly, IPropertyWindowViewFactory factory)
        {
            var manager = new PropertyViewTransactionManager();

            var openResult = manager.Open(instance);
            var transientInstance = openResult.TransientTarget;

            var dialog = factory.Create(transientInstance, target, readOnly);
            if (dialog is Window window)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = owner;
                var result = window.ShowDialog();

                if (result == true)
                {
                    manager.Commit(instance, transientInstance);
                    return true;
                }
                else
                {
                    manager.Cancel();
                    return false;
                }
            }

            manager.Cancel();
            return false;
        }
    }
}