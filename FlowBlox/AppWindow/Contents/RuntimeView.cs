using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.Views;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class RuntimeView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly RuntimeViewControl _runtimeViewControl;
        private readonly RuntimeViewModel _viewModel;

        public RuntimeView()
        {
            Text = FlowBloxResourceUtil.GetLocalizedString("RuntimeView_Text", typeof(FlowBloxMainUITexts));
            Name = Text;
            DockAreas = DockAreas.DockBottom;
            Padding = new Padding(0, 0, 0, 25);

            _runtimeViewControl = new RuntimeViewControl();
            _viewModel = _runtimeViewControl.DataContext as RuntimeViewModel;
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Child = _runtimeViewControl
            };

            Controls.Add(_elementHost);
        }

        internal void InitializeRuntime(BaseRuntime runtime)
        {
            _runtimeViewControl.InitializeRuntime(runtime);
        }

        internal void Append(string message, FlowBloxLogLevel logLevel)
        {
            _runtimeViewControl.Append(message, logLevel);
        }

        internal void ContinueExecutionByUser() => _runtimeViewControl.ContinueExecutionByUser();

        internal void PauseExecutionByUser() => _runtimeViewControl.PauseExecutionByUser();

        internal void StopExecutionByUser() => _runtimeViewControl.StopExecutionByUser();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _viewModel?.Dispose();

            base.Dispose(disposing);
        }
    }
}