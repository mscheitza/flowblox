using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace FlowBlox.Grid.Elements.UserControls.Renderer
{
    public class FlowBlockUIElementRenderer
    {
        private readonly Font _font;
        private readonly ToolTip _toolTip = new ToolTip();

        private TableLayoutPanel _tablePanel;
        private FlowBlockUIElement _flowBlockUIElement;
        private BaseFlowBlock _flowBlock;

        public int RenderedEntries { get; private set; }

        public FlowBlockUIElementRenderer(FlowBlockUIElement flowBlockUIElement)
        {
            this._flowBlockUIElement = flowBlockUIElement;
            this._flowBlock = flowBlockUIElement.InternalFlowBlock;
            this._font = new Font("Calibri", (float)8.25);
        }

        private void InitializeToolTip()
        {
            _toolTip.AutoPopDelay = int.MaxValue;
            _toolTip.InitialDelay = 500;
            _toolTip.ReshowDelay = 500;
            _toolTip.ShowAlways = true;
        }

        public TableLayoutPanel Render()
        {
            InitializeToolTip();

            _tablePanel = new TableLayoutPanel 
            { 
                ColumnCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            _tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Icon
            _tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
            _tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // labelValue

            this.RenderedEntries = 0;

            RenderProperties(_flowBlock);
            RenderFields(_flowBlock);
            RenderRequiredFields(_flowBlock);
            RenderActivationConditions(_flowBlock);

            AdjustTableWidthToMaxLabelValue();

            _tablePanel.RowCount = this.RenderedEntries;
            return _tablePanel;
        }

        public void AdjustTableWidthToMaxLabelValue()
        {
            int lastColumnIndex = _tablePanel.ColumnCount - 1;
            int maxLabelWidth = 0;

            foreach (Control control in _tablePanel.Controls)
            {
                if (control is Label label && _tablePanel.GetColumn(label) == lastColumnIndex)
                {
                    // Ermittle die tatsächliche Breite des Textes im Label
                    var textSize = TextRenderer.MeasureText(label.Text, label.Font);

                    // Prüfe, ob dies das breiteste Label ist
                    if (label.Width > maxLabelWidth)
                    {
                        maxLabelWidth = label.Width;
                    }
                }
            }

            // Berechne die Gesamtbreite des TableLayoutPanels basierend auf der größten Label-Breite
            int totalWidth = 0;

            // Addiere die Breiten der ersten beiden Spalten (AutoSize)
            for (int i = 0; i < lastColumnIndex; i++)
            {
                totalWidth += _tablePanel.GetColumnWidths()[i];
            }

            // Addiere die maximale Breite der letzten Spalte (labelValue)
            totalWidth += maxLabelWidth;

            // Setze die Breite des TableLayoutPanels
            _tablePanel.Width = totalWidth;
        }

        private Color MouseOverColor => Color.LightSlateGray;
        private Color MouseLeaveColor => Color.Transparent;

        private void RenderProperties(BaseFlowBlock block)
        {
            foreach (var propertyName in block.GetDisplayableProperties())
            {
                var property = block.GetType().GetProperty(propertyName);
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();

                var propertyValue = property == null
                    ? null
                    : FlowBloxFieldHelper.GetPropertyValueOrSelectedField(block, property);
                if (propertyValue == null)
                    continue;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsGenericType)
                {
                    RenderListProperties(propertyName, propertyValue);
                }
                else
                {
                    var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, true);
                    AddPropertyRow(displayName, propertyName, propertyValue.ToString());
                }
            }
        }

        private void RenderListProperties(string propertyName, object propertyValue)
        {
            var list = (System.Collections.IEnumerable)propertyValue;
            var listType = propertyValue.GetType().GetGenericArguments()[0];
            var displayAttribute = listType.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttribute != null ?
                FlowBloxResourceUtil.GetDisplayName(displayAttribute, true) :
                propertyName;

            int index = 0;
            foreach (var item in list)
            {
                var headerText = $"{displayName} #{index + 1}";
                AddHeaderRow(headerText);

                foreach (var itemProperty in listType.GetProperties())
                {
                    var itemDisplayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();
                    if (itemDisplayAttribute == null)
                        continue;

                    var itemPropertyValue = itemProperty.GetValue(item);
                    if (itemPropertyValue == null)
                        continue;

                    var itemDisplayName = FlowBloxResourceUtil.GetDisplayName(itemDisplayAttribute, true);
                    AddPropertyRow(itemDisplayName, $"{propertyName}[{index}].{itemProperty.Name}", itemPropertyValue.ToString(), item);
                }

                index++;
            }
        }

        private void AddHeaderRow(string headerText)
        {
            var headerFont = new Font(_font, FontStyle.Underline);

            var headerLabel = new Label
            {
                Text = headerText,
                Font = headerFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _tablePanel.Controls.Add(new Label { AutoSize = true }, 0, this.RenderedEntries);
            _tablePanel.Controls.Add(headerLabel, 1, this.RenderedEntries);
            _tablePanel.Controls.Add(new Label { AutoSize = true }, 2, this.RenderedEntries);

            _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            this.RenderedEntries++;
        }

        private void AddPropertyRow(string displayName, string propertyName, string propertyValue, object propertyInstance = null)
        {
            var icon = new PictureBox { Image = FlowBloxMainUIImages.property_small_12, Size = new Size(12, 12), SizeMode = PictureBoxSizeMode.CenterImage, Dock = DockStyle.Left };
            var label = new Label { Text = displayName, Font = _font, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Left, AutoSize = true };
            var labelValue = new Label
            {
                AutoSize = true,
                Text = GetLabelValue(propertyValue),
                Font = _font,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            RegisterDblClickAction(labelValue, () =>
            {
                if (propertyInstance != null)
                    _flowBlockUIElement.RaiseListPropertyDoubleClick(propertyName, propertyInstance);
                else
                    _flowBlockUIElement.RaisePropertyDoubleClick(propertyName);
            });

            _toolTip.SetToolTip(labelValue, propertyValue);

            labelValue.MouseEnter += (s, e) => labelValue.BackColor = MouseOverColor;
            labelValue.MouseLeave += (s, e) => labelValue.BackColor = MouseLeaveColor;

            _tablePanel.Controls.Add(icon, 0, this.RenderedEntries);
            _tablePanel.Controls.Add(label, 1, this.RenderedEntries);
            _tablePanel.Controls.Add(labelValue, 2, this.RenderedEntries);

            _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            this.RenderedEntries++;
        }

        private string GetLabelValue(string value)
        {
            if (value == null)
                return null;

            if (value.Contains(Environment.NewLine))
                value = value.Replace(Environment.NewLine, " ");

            return value.Trim();
        }

        private void RenderFields(BaseFlowBlock block)
        {
            if (!(block is BaseResultFlowBlock))
                return;

            foreach (var field in ((BaseResultFlowBlock)block).Fields)
            {
                string shortText;

                var labelIcon = new PictureBox 
                { 
                    Image = FlowBloxMainUIImages.field_small_12, 
                    Size = new Size(12, 12), 
                    SizeMode = PictureBoxSizeMode.CenterImage, 
                    Dock = DockStyle.Left 
                };

                var labelValue = new Label 
                {
                    AutoSize = true,
                    Text = field.Name, 
                    Font = _font, 
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                RegisterDblClickAction(labelValue, () =>
                {
                    _flowBlockUIElement.RaiseResultFieldDoubleClick(field);
                });

                _toolTip.SetToolTip(labelValue, string.Format(
                    FlowBloxResourceUtil.GetLocalizedString("FlowBlockUIElementRenderer_Field_Tooltip", typeof(FlowBloxMainUITexts)),
                    field.Name, field.FullyQualifiedName));

                labelValue.MouseEnter += (s, e) => labelValue.BackColor = MouseOverColor;
                labelValue.MouseLeave += (s, e) => labelValue.BackColor = MouseLeaveColor;

                _tablePanel.Controls.Add(labelIcon, 0, this.RenderedEntries);
                _tablePanel.Controls.Add(labelValue, 1, this.RenderedEntries);
                _tablePanel.SetColumnSpan(labelValue, 2);

                _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                this.RenderedEntries++;

                RenderModifiers(field);
                RenderConditions(field);
            }
        }

        private void RegisterDblClickAction(Label labelValue, Action action)
        {
            string clipboardString = null;
            labelValue.Click += (sender, eventArgs) =>
            {
                if (Clipboard.ContainsData(DataFormats.Text))
                    clipboardString = Clipboard.GetText();
                else
                    clipboardString = null;
            };
            labelValue.DoubleClick += (sender, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(clipboardString))
                    Clipboard.SetText(clipboardString);

                action.Invoke();
            };
        }

        private void RenderModifiers(FieldElement fieldElement)
        {
            foreach (var modifier in fieldElement.Modifiers)
            {
                string shortText;

                var labelIcon = new Label 
                { 
                    Text = "mod", 
                    Font = _font, 
                    ForeColor = Color.LightGreen, 
                    TextAlign = ContentAlignment.MiddleRight, 
                    Dock = DockStyle.Right, 
                    AutoSize = true };

                var labelValue = new Label
                {
                    AutoSize = true,
                    Text = GetLabelValue(modifier.ToString()),
                    Font = _font,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                RegisterDblClickAction(labelValue, () =>
                {
                    _flowBlockUIElement.RaiseModifierDoubleClick(modifier);
                });

                _toolTip.SetToolTip(labelValue, modifier.ToString());

                labelValue.MouseEnter += (s, e) => labelValue.BackColor = MouseOverColor;
                labelValue.MouseLeave += (s, e) => labelValue.BackColor = MouseLeaveColor;

                _tablePanel.Controls.Add(labelIcon, 0, this.RenderedEntries);
                _tablePanel.Controls.Add(labelValue, 1, this.RenderedEntries);
                _tablePanel.SetColumnSpan(labelValue, 2);

                _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                this.RenderedEntries++;
            }
        }

        private void RenderConditions(FieldElement fieldElement)
        {
            foreach (var condition in fieldElement.Conditions)
            {
                string shortText;

                var labelIcon = new Label 
                { 
                    Text = "cond", 
                    Font = _font, 
                    ForeColor = Color.LightCoral, 
                    TextAlign = ContentAlignment.MiddleRight, 
                    Dock = DockStyle.Right, 
                    AutoSize = true 
                };

                var labelValue = new Label
                {
                    AutoSize = true,
                    Text = GetLabelValue(condition.ShortDisplayName),
                    Font = _font,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                RegisterDblClickAction(labelValue, () =>
                {
                    _flowBlockUIElement.RaiseConditionDoubleClick(condition);
                });

                _toolTip.SetToolTip(labelValue, condition.DisplayName);

                labelValue.MouseEnter += (s, e) => labelValue.BackColor = MouseOverColor;
                labelValue.MouseLeave += (s, e) => labelValue.BackColor = MouseLeaveColor;

                _tablePanel.Controls.Add(labelIcon, 0, this.RenderedEntries);
                _tablePanel.Controls.Add(labelValue, 1, this.RenderedEntries);
                _tablePanel.SetColumnSpan(labelValue, 2);

                _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                this.RenderedEntries++;
            }
        }

        private void RenderRequiredFields(BaseFlowBlock block)
        {
            foreach (var requiredFieldContext in block.GetRequiredFieldContexts())
            {
                var labelIcon = new Label
                {
                    Text = "req",
                    Font = _font,
                    ForeColor = Color.PeachPuff,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Left,
                    AutoSize = true
                };

                var labelValue = new Label
                {
                    Text = requiredFieldContext.FieldElement.Name,
                    Font = _font,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    AutoSize = true
                };

                RegisterDblClickAction(labelValue, () =>
                {
                    _flowBlockUIElement.RaiseRequiredFieldDoubleClick(requiredFieldContext.FlowBloxComponent, requiredFieldContext.FieldElement);
                });

                _toolTip.SetToolTip(labelValue, string.Format(
                    FlowBloxResourceUtil.GetLocalizedString("FlowBlockUIElementRenderer_RequiredFields_Tooltip", typeof(FlowBloxMainUITexts)),
                    requiredFieldContext.FieldElement.FullyQualifiedName));

                labelValue.MouseEnter += (s, e) => labelValue.BackColor = MouseOverColor;
                labelValue.MouseLeave += (s, e) => labelValue.BackColor = MouseLeaveColor;

                _tablePanel.Controls.Add(labelIcon, 0, this.RenderedEntries);
                _tablePanel.Controls.Add(labelValue, 1, this.RenderedEntries);
                _tablePanel.SetColumnSpan(labelValue, 2);

                _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                this.RenderedEntries++;
            }
        }

        private void RenderActivationConditions(BaseFlowBlock block)
        {
            foreach (var condition in block.ActivationConditions)
            {
                string shortText;

                var labelIcon = new Label { Text = "if", Font = _font, ForeColor = Color.Goldenrod, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Left, AutoSize = true };
                var labelValue = new Label 
                {
                    AutoSize = true,
                    Text = GetLabelValue(condition.ShortDisplayName), 
                    Font = _font, 
                    TextAlign = ContentAlignment.MiddleLeft, 
                    Dock = DockStyle.Fill
                };

                RegisterDblClickAction(labelValue, () =>
                {
                    _flowBlockUIElement.RaiseConditionDoubleClick(condition);
                });

                _toolTip.SetToolTip(labelValue, condition.DisplayName);

                labelValue.MouseEnter += (s, e) => labelValue.BackColor = MouseOverColor;
                labelValue.MouseLeave += (s, e) => labelValue.BackColor = MouseLeaveColor;

                _tablePanel.Controls.Add(labelIcon, 0, this.RenderedEntries);
                _tablePanel.Controls.Add(labelValue, 1, this.RenderedEntries);
                _tablePanel.SetColumnSpan(labelValue, 2);

                _tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                this.RenderedEntries++;
            }
        }
    }
}
