using FlowBlox.Core.DependencyInjection;
using FlowBlox.UICore.Views;
using System.Windows;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Manager;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.PropertyView;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public static class WpfPropertyWindowProvider
    {
        public static bool CreatePropertyWindowAndShowDialog(Window owner, object target, object instance, bool readOnly, bool isNew = false)
        {
            var propertyWindowViewFactory = GetPropertyWindowViewFactoryForType(instance.GetType());
            if (propertyWindowViewFactory != null)
            {
                return InvokeWPFViewUsingTransaction(owner, instance, target, readOnly, propertyWindowViewFactory, isNew);
            }
            else
            {
                var propertyView = new PropertyWindow(new PropertyWindowArgs(instance, parent: target, readOnly: readOnly, isNew: isNew))
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

        private static bool InvokeWPFViewUsingTransaction(Window owner, object instance, object target, bool readOnly, IPropertyWindowViewFactory factory, bool isNew)
        {
            var manager = new PropertyViewTransactionManager();

            var openResult = manager.Open(instance);
            var transientInstance = openResult.TransientTarget;

            var dialog = factory.Create(transientInstance, target, readOnly);
            if (dialog is Window window)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = owner;
                MarkDialogAsDirtyIfNew(dialog, isNew);
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

        private static void MarkDialogAsDirtyIfNew(object dialog, bool isNew)
        {
            if (!isNew || dialog == null)
                return;

            if (dialog is PropertyWindow propertyWindow &&
                propertyWindow.DataContext is PropertyWindowViewModel propertyWindowViewModel)
            {
                if (propertyWindowViewModel.PropertyViewModel != null)
                    propertyWindowViewModel.PropertyViewModel.IsDirty = true;
                return;
            }

            if (dialog is TestDefinitionView testDefinitionView &&
                testDefinitionView.DataContext is TestDefinitionViewModel testDefinitionViewModel)
            {
                testDefinitionViewModel.IsDirty = true;
            }
        }
    }
}
