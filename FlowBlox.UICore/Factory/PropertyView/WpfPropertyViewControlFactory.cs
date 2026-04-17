using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Events;
using FlowBlox.UICore.Factory.Base;
using FlowBlox.UICore.Models.PropertyView;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using Mysqlx.Crud;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class WpfPropertyViewControlFactory : PropertyViewControlFactoryBase<Window>, INotifyPropertyChanged
    {
        protected readonly Window _window;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<AssociationBeforeLinkEventArgs> AssociationBeforeLink;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual AssociationBeforeLinkResult ProcessAssociationBeforeLink(string propertyName, object linkedObject)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name must not be null, empty, or whitespace.", nameof(propertyName));

            if (linkedObject == null)
                throw new ArgumentNullException(nameof(linkedObject));

            var eventArgs = new AssociationBeforeLinkEventArgs(propertyName, linkedObject);
            AssociationBeforeLink?.Invoke(this, eventArgs);

            return new AssociationBeforeLinkResult
            {
                Cancel = eventArgs.Cancel,
                LinkedObject = eventArgs.LinkedObject
            };
        }

        public WpfPropertyViewControlFactory(Window window, PropertyInfo property, object target, bool readOnly)
            : base(property, target, readOnly)
        {
            _window = window;
        }

        protected override Type ShowTypeSelectionDialog(Window owner, IList<Type> types)
        {
            var dialog = new MultiValueSelectionDialog("Auswählen", "Bitte wählen Sie ein Objekt aus.",
                new GenericSelectionHandler<Type>(types, t => FlowBloxResourceUtil.GetDisplayName(t.GetCustomAttribute<DisplayAttribute>())));

            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var result = dialog.ShowDialog();
            return result == true ? dialog.SelectedItem.Value as Type : null;
        }

        protected async override Task ShowDependencyViolationDialogAsync(Window owner, List<string> allReferences, List<IFlowBloxComponent> dependencies)
        {
            await MessageBoxHelper.ShowMessageBoxAsync(
                    (MetroWindow)owner,
                    FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Title"),
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message"),
                        string.Join(Environment.NewLine, allReferences.Select(description => " - " + description)))
                );
        }
    }
}