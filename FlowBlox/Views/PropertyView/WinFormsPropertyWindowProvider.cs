using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Views.Main;
using System.Windows.Forms;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Util.WPF;
using FlowBlox.UICore.Views;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Grid.Provider;
using System.Windows;
using FlowBlox.UICore.Factory;
using System;
using System.Linq;
using FlowBlox.Core.Util;
using System.DirectoryServices.ActiveDirectory;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Manager;

namespace FlowBlox.Views.PropertyView
{
    public class WinFormsPropertyWindowProvider
    {
        public static bool CreatePropertyWindowAndShowDialog(Control control, object target, object instance, bool readOnly)
        {
            var propertyWindowViewFactory = GetPropertyWindowViewFactoryForType(instance.GetType());
            if (propertyWindowViewFactory != null)
            {
                return InvokeWPFViewUsingTransaction(control, instance, target, readOnly, propertyWindowViewFactory);
            }
            else
            {
                var framework = FlowBloxOptions.GetOptionInstance().GetOption("PropertyView.UIFramework");
                if (framework.Value == GlobalConstants.PropertyViewUIFrameworkWPF)
                {
                    var propertyViewWpf = new UICore.Views.PropertyWindow(new PropertyWindowArgs(instance, readOnly: readOnly));
                    var owner = ControlHelper.FindParentOfType<Form>(control, true);
                    return WindowsFormWPFHelper.ShowDialog(propertyViewWpf, owner) == true;
                }
                else
                {
                    var propertyWindow = new Views.PropertyWindow();
                    propertyWindow.StartPosition = FormStartPosition.CenterParent;
                    propertyWindow.Initialize(instance);
                    return propertyWindow.ShowDialog(control.FindForm()) == DialogResult.OK;
                }
            }
        }

        private static IPropertyWindowViewFactory GetPropertyWindowViewFactoryForType(Type instanceType)
        {
            var propertyWindowViewFactories = FlowBloxServiceLocator.Instance.GetServices<IPropertyWindowViewFactory>();
            return propertyWindowViewFactories.FirstOrDefault(x => x.SupportsType(instanceType));
        }

        private static bool InvokeWPFViewUsingTransaction(Control control, object instance, object target, bool readOnly, IPropertyWindowViewFactory factory)
        {
            var manager = new PropertyViewTransactionManager();

            // Initialize and open the transaction
            var openResult = manager.Open(instance);
            var transientInstance = openResult.TransientTarget;

            // Create and show the WPF dialog using the factory
            var dialog = factory.Create(transientInstance, target, readOnly);
            var result = WindowsFormWPFHelper.ShowDialog(dialog, control.FindForm());

            // Commit or cancel the transaction based on the dialog result
            if (result.HasValue && result.Value)
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
    }
}
