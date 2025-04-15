using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Controls;
using System.Linq;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Events;
using FlowBlox.UICore.Utilities;

namespace FlowBlox.Views
{
    /// <summary>
    /// Ansicht des Feld-Monitors. Wird standardmäßig rechts von WebFlowIDE.Grid angezeigt.
    /// </summary>
    public partial class FieldView : UserControl
    {
        private readonly Dictionary<FieldElement, ListViewItem> _fieldMap = new Dictionary<FieldElement, ListViewItem>();
        private readonly ListViewColumnAdjustmentHandler _adjustmentHandler;

        public FieldView()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            ListViewHelper.EnableDoubleBuffer(lvFields);
            _adjustmentHandler = ListViewColumnAdjustmentHandler.Register(lvFields);
            UpdateUI();
        }

        private void UpdateUI()
        {
            itmCopy.Enabled = (lvFields.SelectedItems.Count > 0);
            itmOpenFieldValue.Enabled = (lvFields.SelectedItems.Count == 1);
        }

        private void InitializeFields()
        {
            string filter = tbFilter.Text;
            var filteredFields = GetFieldsWithFilter(filter);
            lvFields.Items.Clear();
            foreach (var fieldElement in filteredFields)
            {
                AppendField(fieldElement);
            }

            var registry = FlowBloxRegistryProvider.GetRegistry();
            registry.OnManagedObjectRemoved -= Registry_OnManagedObjectRemoved;
            registry.OnManagedObjectRemoved += Registry_OnManagedObjectRemoved;
            
            _adjustmentHandler.AdjustListViewColumns();
        }

        private void Registry_OnManagedObjectRemoved(ManagedObjectRemovedEventArgs eventArgs)
        {
            foreach(var lvItemFieldElement in lvFields.Items
                .Cast<ListViewItem>()
                .Where(x => x.Tag == eventArgs.RemovedObject)
                .ToList())
            {
                lvFields.Items.Remove(lvItemFieldElement);
            }
        }

        private void AppendField(FieldElement fieldElement)
        {
            string name = fieldElement.Name;
            ListViewItem lvItemFieldElement = new ListViewItem();
            lvItemFieldElement.Name = name;
            lvItemFieldElement.Text = name;
            lvItemFieldElement.Tag = fieldElement;
            lvItemFieldElement.SubItems.Add(fieldElement.Pending ?
                FlowBloxResourceUtil.GetLocalizedString("FieldElement_PendingValue") : fieldElement.StringValue);
            _fieldMap[fieldElement] = lvItemFieldElement;
            lvFields.Items.Add(lvItemFieldElement);

            fieldElement.OnNameChanged -= FieldElement_OnNameChange;
            fieldElement.OnNameChanged += FieldElement_OnNameChange;

            fieldElement.OnValueChanged -= FieldElement_OnValueChange;
            fieldElement.OnValueChanged += FieldElement_OnValueChange;
        }

        private void FieldElement_OnValueChange(FieldElement field, string oldValue, string newValue)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FieldElement.FieldElementValueChangedEventHandler(FieldElement_OnValueChange), new object[] { field, oldValue, newValue });
                return;
            }

            if (_fieldMap.TryGetValue(field, out var _listViewItem))
            {
                _listViewItem.SubItems[1].Text = newValue;
            }
        }

        private void FieldElement_OnNameChange(FieldElement field, string oldName, string newName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FieldElement.FieldElementNameChangedEventHandler(FieldElement_OnNameChange), new object[] { field, oldName, newName });
                return;
            }

            if (_fieldMap.TryGetValue(field, out var _listViewItem))
            {
                _listViewItem.Name = field.Name;
                _listViewItem.Text = field.Name;
            }
        }

        public int GetWidth()
        {
            int Width = 19;

            foreach (ColumnHeader ColumnHeader in lvFields.Columns)
            {
                Width += ColumnHeader.Width;
            }

            return Width;
        }

        private void itmCopy_Click(object sender, EventArgs e)
        {
            StringBuilder copyExpression = new StringBuilder();
            foreach (ListViewItem lvItemField in lvFields.SelectedItems)
            {
                copyExpression.AppendLine(((FieldElement)lvItemField.Tag).StringValue);
            }
            try
            {
                Clipboard.SetText(copyExpression.ToString());
            }
            catch (Exception Exception)
            {
                FlowBloxMessageBox.Show
                    (
                        this,
                        "Konnte den Ausdruck nicht in die Zwischenablage kopieren. Ursache: " + Exception.Message,
                        "Kopieren von Feldwert(en) fehlgeschlagen",
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Error
                    );
            }
        }

        private void itmOpenFieldValue_Click(object sender, EventArgs e)
        {
            if (lvFields.SelectedItems.Count == 1)
            {
                var field = (FieldElement)lvFields.SelectedItems[0].Tag;
                string value = field.StringValue;
                FlowBloxEditingHelper.OpenUsingEditor(value, field.FullyQualifiedName);
            }
        }

        private void lvFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void lvFields_DoubleClick(object sender, EventArgs e)
        {
            itmOpenFieldValue_Click(sender, e);
        }

        private void itmRefresh_Click(object sender, EventArgs e) => InitializeFields();

        private void tbFilter_TextChanged(object sender, EventArgs e) => InitializeFields();

        internal void OnProjectLoaded() => InitializeFields();

        private IEnumerable<FieldElement> GetFieldsWithFilter(string filter)
        {
            List<FieldElement> fields =
            [
                .. FlowBloxRegistryProvider.GetRegistry().GetUserFields(),
                .. FlowBloxRegistryProvider.GetRegistry().GetRuntimeFields(),
            ];
            return fields.Where(x => string.IsNullOrEmpty(filter) || x.FullyQualifiedName.ToLower().Contains(filter.ToLower()));
        }

        internal new bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A) && tbFilter.Focused)
            {
                tbFilter.SelectAll();
                return true;
            }
            return false;
        }

        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            // Überprüfen, ob Strg + A gedrückt wird
            if (e.Control && e.KeyCode == Keys.A)
            {
                // Markieren Sie den gesamten Text in der TextBox
                tbFilter.SelectAll();

                // Unterdrücken Sie die Weiterleitung des Ereignisses
                e.SuppressKeyPress = true;
            }
        }
    }
}
