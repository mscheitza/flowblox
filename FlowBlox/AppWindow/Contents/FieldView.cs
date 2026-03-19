using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.Views;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class FieldView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly FieldViewControl _fieldViewControl;
        private readonly FieldViewModel? _viewModel;

        public FieldView()
        {
            Text = FlowBloxResourceUtil.GetLocalizedString("FieldView_Text", typeof(FlowBloxMainUITexts));
            Name = Text;
            DockAreas = DockAreas.DockRight | DockAreas.DockLeft | DockAreas.DockBottom;
            Padding = new Padding(0, 25, 0, 25);

            _fieldViewControl = new FieldViewControl();
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Child = _fieldViewControl
            };

            Controls.Add(_elementHost);

            _viewModel = _fieldViewControl.DataContext as FieldViewModel;
        }

        internal void OnAfterUIRegistryInitialized()
        {
            _fieldViewControl.OnAfterUIRegistryInitialized();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewModel?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
