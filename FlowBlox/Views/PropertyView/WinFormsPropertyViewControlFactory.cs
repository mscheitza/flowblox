using FlowBlox.Core.Util.WPF;
using FlowBlox.UICore.Views;
using FlowBlox.UICore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Interfaces;
using System.Windows.Forms;
using FlowBlox.UICore.Factory.Base;

namespace FlowBlox.Views.PropertyView
{
    public class WinFormsPropertyViewControlFactory : PropertyViewControlFactoryBase<Form>
    {
        public delegate void ControlChangedEventHandler(object target, PropertyInfo property);

        public event ControlChangedEventHandler ControlChanged;

        protected void RaiseControlChanged() => ControlChanged?.Invoke(_target, _property);

        public WinFormsPropertyViewControlFactory(PropertyInfo property, object target, bool readOnly)
            : base(property, target, readOnly)
        {
        }

        protected override Type ShowTypeSelectionDialog(Form owner, IList<Type> types)
        {
            var invalidTypes = types
                .Where(t => t.GetCustomAttribute<DisplayAttribute>() == null)
                .Select(t => t.FullName)
                .ToList();

            if (invalidTypes.Any())
            {
                FlowBloxMessageBox.Show(
                    owner,
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("Global_MissingDisplayAttributes_Message"),
                        string.Join(Environment.NewLine, invalidTypes.Select(name => $" - {name}"))
                    ),
                    FlowBloxResourceUtil.GetLocalizedString("Global_MissingDisplayAttributes_Title"),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Warning);

                return null;
            }

            var dialog = new MultiValueSelectionDialog("Auswählen", "Bitte wählen Sie ein Objekt aus.",
                new GenericSelectionHandler<Type>(types, t => FlowBloxResourceUtil.GetDisplayName(t.GetCustomAttribute<DisplayAttribute>())));

            var result = WindowsFormWPFHelper.ShowDialog(dialog, owner);
            return result.HasValue && result.Value ? dialog.SelectedItem.Value as Type : null;
        }

        protected override async Task ShowDependencyViolationDialogAsync(Form owner, List<string> allReferences, List<IFlowBloxComponent> dependencies)
        {
            await Task.Run(() =>
            {
                if (owner.InvokeRequired)
                {
                    owner.BeginInvoke(new System.Windows.Forms.MethodInvoker(() =>
                    {
                        ShowDependencyViolationDialog(owner, allReferences, dependencies);
                    }));
                }
                else
                {
                    ShowDependencyViolationDialog(owner, allReferences, dependencies);
                }
            });
        }

        private void ShowDependencyViolationDialog(Form owner, List<string> allReferences, List<IFlowBloxComponent> dependencies)
        {
            FlowBloxMessageBox.Show(
                owner,
                string.Format(FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Message"), string.Join(Environment.NewLine, allReferences.Select(description => string.Concat(" - ", description)))),
                FlowBloxResourceUtil.GetLocalizedString("Global_DependencyViolation_Title"),
                FlowBloxMessageBox.Buttons.OK,
                FlowBloxMessageBox.Icons.Info);

        }
    }
}
