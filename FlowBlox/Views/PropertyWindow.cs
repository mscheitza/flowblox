using System;
using System.Reflection;
using System.Windows.Forms;
using FlowBlox.Views.PropertyView;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Util.Controls;

namespace FlowBlox.Views
{
    public partial class PropertyWindow : Form
    {
        private object _target;
        private PropertyViewTabControl _propertyViewTabControl;
        private bool anyValueChange;

        public PropertyWindow()
        {
            InitializeComponent();
            FlowBloxUILocalizationUtil.Localize(this);
        }

        internal void Initialize(object inst)
        {
            this.Text = FlowBloxComponentHelper.GetDisplayName(inst);

            _propertyViewTabControl = new PropertyViewTabControl()
            {
                Dock = DockStyle.Fill
            };
            _propertyViewTabControl.Initialize(inst, false);
            _propertyViewTabControl.TargetChanged += _propertyViewTabControl_TargetChanged;
            this.Controls.Add(_propertyViewTabControl);
            _propertyViewTabControl.Dock = DockStyle.Fill;
            UpdateUI();
        }

        private void _propertyViewTabControl_TargetChanged(object target, PropertyInfo property)
        {
            this.anyValueChange = true;
            this.UpdateUI();
        }

        private void UpdateUI()
        {
            btApply.Enabled = anyValueChange;
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_propertyViewTabControl.Apply())
                    return;

                if (this._target is IManagedObject)
                    ((IManagedObject)_target).OnAfterSave();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);

                FlowBloxMessageBox.Show(this,
                    FlowBloxResourceUtil.GetLocalizedString(nameof(PropertyWindow), "ApplyFailed", "Message"),
                    FlowBloxResourceUtil.GetLocalizedString(nameof(PropertyWindow), "ApplyFailed", "Title"),
                    FlowBloxMessageBox.Buttons.OK, FlowBloxMessageBox.Icons.Info);
            }
        }

        private void PropertyWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_propertyViewTabControl.State == PropertyViewTabControlState.Opened)
                _propertyViewTabControl.Cancel();
        }
    }
}
