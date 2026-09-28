using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.ViewModels.PropertyView;
using FlowBlox.UICore.Views;
using System.Windows;
using WpfPropertyWindowProviderResources = FlowBlox.UICore.Resources.ClassResources.WpfPropertyWindowProvider;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public static class WpfPropertyWindowProvider
    {
        public static bool CreatePropertyWindowAndShowDialog(Window owner, object target, object instance, bool readOnly, bool isNew = false)
        {
            try
            {
                var propertyWindowViewFactory = GetPropertyWindowViewFactoryForType(instance.GetType());
                if (propertyWindowViewFactory != null)
                {
                    if (!propertyWindowViewFactory.CanCreate(instance, target, readOnly, out var message))
                    {
                        ShowViewCannotBeCreatedMessage(message);
                        return false;
                    }

                    return InvokeWPFView(owner, instance, target, readOnly, propertyWindowViewFactory, isNew);
                }

                var propertyView = new PropertyWindow(new PropertyWindowArgs(instance, parent: target, readOnly: readOnly, isNew: isNew))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = owner
                };
                return propertyView.ShowDialog() == true;
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                ShowViewCreationErrorMessage();
                return false;
            }
        }

        private static IPropertyWindowViewFactory GetPropertyWindowViewFactoryForType(Type instanceType)
        {
            var propertyWindowViewFactories = FlowBloxServiceLocator.Instance.GetServices<IPropertyWindowViewFactory>();
            return propertyWindowViewFactories.FirstOrDefault(x => x.SupportsType(instanceType));
        }

        private static bool InvokeWPFView(
            Window owner,
            object instance,
            object target,
            bool readOnly,
            IPropertyWindowViewFactory factory,
            bool isNew)
        {
            var dialog = factory.Create(instance, target, readOnly);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = owner;
            MarkDialogAsDirtyIfNew(dialog, isNew);
            return dialog.ShowDialog() == true;
        }

        private static void ShowViewCreationErrorMessage()
        {
            FlowBloxServiceLocator.Instance
                .GetService<IFlowBloxMessageBoxService>()
                ?.ShowMessageBox(
                    FlowBloxResourceUtil.GetLocalizedString(
                        nameof(WpfPropertyWindowProviderResources.ViewCreationErrorMessage),
                        typeof(WpfPropertyWindowProviderResources)),
                    FlowBloxResourceUtil.GetLocalizedString(
                        nameof(WpfPropertyWindowProviderResources.ViewCreationErrorTitle),
                        typeof(WpfPropertyWindowProviderResources)),
                    FlowBloxMessageBoxTypes.Error);
        }

        private static void ShowViewCannotBeCreatedMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            FlowBloxServiceLocator.Instance
                .GetService<IFlowBloxMessageBoxService>()
                ?.ShowMessageBox(
                    message,
                    FlowBloxResourceUtil.GetLocalizedString(
                        nameof(WpfPropertyWindowProviderResources.ViewCannotBeCreatedTitle),
                        typeof(WpfPropertyWindowProviderResources)),
                    FlowBloxMessageBoxTypes.Information);
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
                return;
            }

            if (dialog is not FrameworkElement frameworkElement ||
                frameworkElement.DataContext == null)
                return;

            var dataContext = frameworkElement.DataContext;
            var isDirtyProperty = dataContext.GetType().GetProperty(nameof(PropertyViewModel.IsDirty));
            if (isDirtyProperty?.CanWrite == true &&
                isDirtyProperty?.PropertyType == typeof(bool))
            {
                isDirtyProperty.SetValue(dataContext, true);
            }
        }
    }
}
