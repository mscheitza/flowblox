using FlowBlox.Core;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.UI;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Utilities;
using FlowBlox.Views;
using FlowBlox.Views.PropertyView;
using System;
using System.Windows.Forms;

namespace FlowBlox.Grid.Views.Main
{
    public partial class FlowBlockMainView : Form
    {
        private BaseFlowBlock _flowBlock;
        private PropertyViewTabControl _propertyViewTabControl;
        private bool anyValueChange;

        public bool ReadOnly { get; set; }

        public string InitiallyFocussedProperty { get; set; }

        public FlowBlockMainView()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
        }

        private void InitializeToolStrip()
        {
            var transientTarget = (BaseFlowBlock)_propertyViewTabControl.TransientTarget;
            var toolStripButtons = new UIActionsToolstripButtonProvider().GetToolStripItemsForComponent(transientTarget);
            foreach (var button in toolStripButtons)
            {
                toolStrip.Items.Add(button);
            }
            FlowBloxStyle.ApplyStyle(toolStrip);
        }

        private void InitializePropertyTabControl()
        {
            this._propertyViewTabControl = new PropertyViewTabControl();
            _propertyViewTabControl.TargetChanged += PropertyViewTabControl_TargetChanged;
            _propertyViewTabControl.Dock = DockStyle.Fill;
            _propertyViewTabControl.Initialize(_flowBlock, this.ReadOnly);
            var tabControl = _propertyViewTabControl;
            this.mainPanel.Controls.Add(tabControl);
            UpdateUI();
        }

        private void PropertyViewTabControl_TargetChanged(object target, System.Reflection.PropertyInfo property)
        {
            this.anyValueChange = true;
            UpdateUI();
        }

        public void Initialize(BaseFlowBlock flowBlock)
        {
            this.pictureBoxLogo.Image = SkiaToSystemDrawingHelper.ToSystemDrawingImage(flowBlock.Icon32);
            this.Text = FlowBloxComponentHelper.GetDisplayName(flowBlock);
            this.labelDescription.Text = FlowBlockHelper.GetDescription(flowBlock);
            this._flowBlock = flowBlock;
            InitializePropertyTabControl();
            InitializeToolStrip();
            UpdateUI();
        }

        private void UpdateUI()
        {
            btApply.Enabled = !ReadOnly && anyValueChange;
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_propertyViewTabControl.Apply())
                    return;

                _flowBlock.OnAfterSave();
                this.Close();
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);

                FlowBloxMessageBox.Show(this,
                    FlowBloxResourceUtil.GetLocalizedString(nameof(FlowBlockMainView), "ApplyFailed", "Message"),
                    FlowBloxResourceUtil.GetLocalizedString(nameof(FlowBlockMainView), "ApplyFailed", "Title"),
                    FlowBloxMessageBox.Buttons.OK, FlowBloxMessageBox.Icons.Info);
            }
        }

        private void FlowBlockMainView_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_propertyViewTabControl.State == PropertyViewTabControlState.Opened)
                _propertyViewTabControl.Cancel();
        }

        private void btManualExecution_Click(object sender, EventArgs e)
        {

        }

        private void FlowBlockMainView_Shown(object sender, EventArgs e)
        {
            if (this.InitiallyFocussedProperty != null)
                ControlHelper.FocusControlByName(this, this.InitiallyFocussedProperty, true);
        }
    }
}
