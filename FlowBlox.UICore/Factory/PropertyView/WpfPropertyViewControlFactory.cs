using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using FlowBlox.UICore.Factory.Base;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class WpfPropertyViewControlFactory : PropertyViewControlFactoryBase<Window>, INotifyPropertyChanged
    {
        protected readonly Window _window;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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