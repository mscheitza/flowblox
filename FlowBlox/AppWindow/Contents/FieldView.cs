using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Events;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Grid.Provider;
using FlowBlox.UICore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FlowBlox.Views
{
    /// <summary>
    /// View of the field monitor. Displayed to the right of FlowBlox Grid by default.
    // </summary>
    public partial class FieldView : UserControl
    {
        private readonly Dictionary<FieldElement, ListViewItem> _fieldMap = new Dictionary<FieldElement, ListViewItem>();
        private readonly ListViewColumnAdjustmentHandler _adjustmentHandler;
        private FlowBloxProjectComponentProvider _componentProvider;
        private readonly Font _smallFont;
        private FlowBloxRegistry _registry;
        private FlowBloxUIRegistry _uiRegistry;

        public FieldView()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            ListViewHelper.EnableDoubleBuffer(lvFields);
            _smallFont = new Font(lvFields.Font.FontFamily, Math.Max(6f, lvFields.Font.Size - 2f), lvFields.Font.Style);
            _adjustmentHandler = ListViewColumnAdjustmentHandler.Register(lvFields);
            _adjustmentHandler.ApplyColumnFont(chFlowBlockName.Index, _smallFont);
            _adjustmentHandler.OnColumnWidthCalculated += (s, e) =>
            {
                if (e.Column == chFlowBlockName && e.CalculatedWidth > 0)
                    _savedFlowBlockColWidth = e.CalculatedWidth;
            };
            checkBoxShowFlowBlock.CheckedChanged += CheckBoxShowFlowBlock_CheckedChanged;
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
            _registry = FlowBloxRegistryProvider.GetRegistry();
            RebindUiRegistry();
            UpdateUI();
        }

        private void _uiRegistry_FlowBlockUIElementRegistered(object sender, Grid.Events.FlowBlockUIElementRegisteredEventArgs e)
        {
            RegisterElementSelectedChangedByUser(e.UIElement);
        }

        private void CheckBoxShowFlowBlock_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private int _savedFlowBlockColWidth;
        private void SetFlowBlockColumnVisible(bool visible)
        {
            var col = chFlowBlockName;
            if (visible)
            {
                if (col.Width == 0)
                    col.Width = _savedFlowBlockColWidth;
            }
            else if (col.Width > 0)
            {
                _savedFlowBlockColWidth = col.Width;
                col.Width = 0;
            }
            lvFields.Invalidate();
        }

        private void UpdateUI()
        {
            itmCopy.Enabled = (lvFields.SelectedItems.Count > 0);
            itmOpenFieldValue.Enabled = (lvFields.SelectedItems.Count == 1);
            SetFlowBlockColumnVisible(checkBoxShowFlowBlock.Checked);
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

            _registry.OnManagedObjectRemoved -= Registry_OnManagedObjectRemoved;
            _registry.OnManagedObjectRemoved += Registry_OnManagedObjectRemoved;

            _adjustmentHandler.AdjustListViewColumns();
        }

        private void Registry_OnManagedObjectRemoved(ManagedObjectRemovedEventArgs eventArgs)
        {
            foreach (var lvItemFieldElement in lvFields.Items
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
            lvItemFieldElement.Text = fieldElement.FlowBlockName;
            lvItemFieldElement.Tag = fieldElement;
            lvItemFieldElement.SubItems[0].Tag = fieldElement.FlowBlockName;
            lvItemFieldElement.SubItems.Add(fieldElement.Name);
            lvItemFieldElement.SubItems.Add(fieldElement.Pending ?
                FlowBloxResourceUtil.GetLocalizedString("FieldElement_PendingValue") : 
                TextHelper.ShortenString(fieldElement.StringValue, 500, true));

            _fieldMap[fieldElement] = lvItemFieldElement;
            lvFields.Items.Add(lvItemFieldElement);

            fieldElement.OnNameChanged -= FieldElement_OnNameChange;
            fieldElement.OnNameChanged += FieldElement_OnNameChange;

            fieldElement.OnValueChanged -= FieldElement_OnValueChange;
            fieldElement.OnValueChanged += FieldElement_OnValueChange;

            if (fieldElement.IsRegularField() && _uiRegistry != null)
            {
                var uiElement = _uiRegistry.GetUIElementToGridElement(fieldElement.Source);
                RegisterElementSelectedChangedByUser(uiElement);
            }
        }

        private void RegisterElementSelectedChangedByUser(FlowBlockUIElement uiElement)
        {
            if (uiElement == null)
                return;

            uiElement.ElementSelectedChangedByUser -= UiElement_ElementSelectedChangedByUser;
            uiElement.ElementSelectedChangedByUser += UiElement_ElementSelectedChangedByUser;
        }

        private void UiElement_ElementSelectedChangedByUser(FlowBlockUIElement sender, bool selected)
        {
            if (_uiRegistry == null)
                return;

            var resultFlowBlock = sender.InternalFlowBlock as BaseResultFlowBlock;
            if (resultFlowBlock == null)
                return;

            var selectedFields = _uiRegistry.UIElements
                .Where(x => x.ElementSelected)
                .SelectMany(x => (x.InternalFlowBlock as BaseResultFlowBlock)?.Fields ?? Enumerable.Empty<FieldElement>())
                .ToList();

            foreach (ListViewItem lvItemFieldElement in lvFields.Items)
            {
                var fieldElement = (FieldElement)lvItemFieldElement.Tag;
                if (fieldElement == null) 
                    continue;

                bool fieldSelected = false;
                if (selectedFields.Contains(fieldElement))
                    fieldSelected = true;

                lvItemFieldElement.Selected = fieldSelected;
            }
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
                _listViewItem.SubItems[2].Text = TextHelper.ShortenString(newValue, 500, true);
            }
        }

        private void FieldElement_OnNameChange(FieldElement field, string oldName, string newName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FieldElement.FieldElementNameChangedEventHandler(FieldElement_OnNameChange), [field, oldName, newName]);
                return;
            }

            if (_fieldMap.TryGetValue(field, out var _listViewItem))
            {
                _listViewItem.Name = field.Name;
                _listViewItem.Text = field.FlowBlockName;
                _listViewItem.SubItems[0].Tag = field.FlowBlockName;
                _listViewItem.SubItems[1].Text = field.Name;
            }
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

        internal void OnAfterUIRegistryInitialized()
        {
            _registry = FlowBloxRegistryProvider.GetRegistry();
            RebindUiRegistry();
            InitializeFields();
        }

        private void RebindUiRegistry()
        {
            if (_uiRegistry != null)
                _uiRegistry.FlowBlockUIElementRegistered -= _uiRegistry_FlowBlockUIElementRegistered;

            _uiRegistry = _componentProvider.GetCurrentUIRegistry();
            if (_uiRegistry != null)
                _uiRegistry.FlowBlockUIElementRegistered += _uiRegistry_FlowBlockUIElementRegistered;
        }

        private IEnumerable<FieldElement> GetFieldsWithFilter(string filter)
        {
            List<FieldElement> fields =
            [
                .. FlowBloxRegistryProvider.GetRegistry().GetUserFields(),
                .. FlowBloxRegistryProvider.GetRegistry().GetRuntimeFields(true),
            ];
            return fields.Where(x => 
                string.IsNullOrEmpty(filter) || 
                x.FullyQualifiedName.ToLower().Contains(filter.ToLower()));
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
            // Check if Ctrl + A is pressed
            if (e.Control && e.KeyCode == Keys.A)
            {
                // Select all text in the text box
                tbFilter.SelectAll();

                // Suppress event forwarding
                e.SuppressKeyPress = true;
            }
        }

        private void lvFields_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = e.Item.Selected || ((ListView)sender).SelectedIndices.Contains(e.ItemIndex);
            var backColor = selected ? SystemColors.Highlight : e.SubItem.BackColor;
            var foreColor = selected ? SystemColors.HighlightText : e.SubItem.ForeColor;
            using (var back = new SolidBrush(backColor))
                e.Graphics.FillRectangle(back, e.Bounds);

            // Only column 0 receives the "BlockName + FieldName" rendering
            if (e.ColumnIndex == 0)
            {
                // Retrieve data
                string blockName = e.SubItem.Tag as string ?? string.Empty;

                // Fonts/Colors
                var gray = selected ?
                    ControlPaint.LightLight(foreColor) :
                    Color.FromArgb(96, 116, 168);

                // Layout
                var bounds = e.Bounds;
                var paddingLeft = 6;
                var yCenter = bounds.Top + (bounds.Height / 2);

                // TextFlags
                var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

                // Measure & draw block name
                var maxWidth = bounds.Width - paddingLeft;
                var blockRect = new Rectangle(bounds.Left + paddingLeft, bounds.Top, maxWidth, bounds.Height);
                var blockSize = TextRenderer.MeasureText(blockName, _smallFont, new Size(int.MaxValue, int.MaxValue), flags);

                // Effective drawn width (with ellipse, possibly smaller than Measure)
                TextRenderer.DrawText(e.Graphics, blockName, _smallFont, blockRect, gray, flags | TextFormatFlags.VerticalCenter);

                return;
            }

            // Draw all other columns by default (including selection)
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text ?? string.Empty, e.SubItem.Font, e.Bounds, foreColor, 
                TextFormatFlags.EndEllipsis | 
                TextFormatFlags.NoPrefix | 
                TextFormatFlags.VerticalCenter | 
                TextFormatFlags.SingleLine);
        }

        private void lvFields_DrawItem(object sender, DrawListViewItemEventArgs e)
        {

        }

        private void checkBoxShowFlowBlock_Click(object sender, EventArgs e)
        {
            UpdateUI();
        }
    }
}
