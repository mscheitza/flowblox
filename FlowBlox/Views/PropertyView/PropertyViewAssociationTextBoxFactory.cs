using FlowBlox.Core.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Views;
using FlowBlox.UICore.Utilities;
using System.Collections;
using FlowBlox.Core.Util.WPF;
using System.Drawing;
using FlowBlox.Core;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Components;

namespace FlowBlox.Views.PropertyView
{
    public class AssociationTextBoxFactory : WinFormsPropertyViewControlFactory
    {
        private readonly FlowBlockUIAttribute _flowBlockUIAttribute;

        private Button _deleteButton;
        private Button _editButton;
        private FlowBloxTextBox _textBox;

        public AssociationTextBoxFactory(PropertyInfo property, object target, bool readOnly) 
            : base(property, target, readOnly)
        {
            _flowBlockUIAttribute = property.GetCustomAttribute<FlowBlockUIAttribute>();
        }

        public Tuple<FlowBloxTextBox, FlowLayoutPanel> Create()
        {
            _textBox = new FlowBloxTextBox()
            {
                Text = _property.GetValue(_target)?.ToString() ?? GetEmptyString(),
                Multiline = false,
                ReadOnly = true,
                ShowSizingGrip = false
            };

            this.ControlChanged += (t, p) =>
            {
                UpdateOperations();
            };

            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            panel.Controls.Add(new PictureBox()
            {
                Image = FlowBloxMainUIImages.component_16,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(16, 20)
            });

            panel.Controls.Add(_textBox);

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Create))
            {
                var addButton = new Button
                {
                    Image = FlowBloxMainUIImages.add_value_16,
                    Width = 24,
                    Enabled = !_readOnly
                };

                addButton.Click += (sender, args) =>
                {
                    var newInstance = CreateNewInstance(panel.FindForm());
                    if (newInstance == null)
                        return;

                    var propertyWindow = new PropertyWindow()
                    {
                        StartPosition = FormStartPosition.CenterParent
                    };
                    propertyWindow.Initialize(newInstance);
                    if (propertyWindow.ShowDialog(panel.FindForm()) == DialogResult.OK)
                    {
                        _property.SetValue(_target, newInstance);
                        _textBox.Text = newInstance.ToString();
                        RaiseControlChanged();
                    }
                };

                panel.Controls.Add(addButton);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Link))
            {
                var linkButton = new Button
                {
                    Image = FlowBloxMainUIImages.link_16,
                    Width = 24,
                    Enabled = !_readOnly
                };

                linkButton.Click += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(_flowBlockUIAttribute.SelectionFilterMethod))
                    {
                        FlowBloxMessageBox.Show(
                            panel.FindForm(),
                            string.Format(
                                FlowBloxResourceUtil.GetLocalizedString("Global_MissingFilterMethod_Message"),
                                _property.Name),
                            FlowBloxResourceUtil.GetLocalizedString("Global_MissingFilterMethod_Title"),
                            FlowBloxMessageBox.Buttons.OK,
                            FlowBloxMessageBox.Icons.Warning);
                        return;
                    }

                    var filterMethod = GetSelectionFilterMethod(_target, _flowBlockUIAttribute.SelectionFilterMethod);
                    if (filterMethod == null)
                        throw new InvalidOperationException("There was no selection filter found.");

                    if (string.IsNullOrEmpty(_flowBlockUIAttribute.SelectionDisplayMember))
                        throw new InvalidOperationException("There was no selection display member found.");

                    var items = filterMethod.Invoke(_target, null) as IList;

                    var dialog = new MultiValueSelectionDialog("Auswählen", "Bitte wählen Sie ein Objekt aus.",
                        new GenericSelectionHandler<object>(items.Cast<object>(), x => (string)ReflectionHelper.GetPropertyFromType(_property.PropertyType, _flowBlockUIAttribute.SelectionDisplayMember).GetValue(x)));

                    var result = WindowsFormWPFHelper.ShowDialog(dialog, panel.FindForm());
                    if (result.HasValue && result.Value)
                    {
                        var inst = dialog.SelectedItem.Value;
                        _property.SetValue(_target, inst);
                        _textBox.Text = inst.ToString();
                        RaiseControlChanged();
                    }
                };

                panel.Controls.Add(linkButton);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Unlink))
            {
                var unlinkButton = new Button
                {
                    Image = FlowBloxMainUIImages.unlink_16,
                    Width = 24,
                    Enabled = !_readOnly
                };

                unlinkButton.Click += (sender, args) =>
                {
                    var item = _property.GetValue(_target, null);
                    if (item == null)
                        return;

                    _textBox.Text = string.Empty;
                    _property.SetValue(_target, null);
                    RaiseControlChanged();
                };

                panel.Controls.Add(unlinkButton);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Edit))
            {
                _editButton = new Button
                {
                    Image = FlowBloxMainUIImages.edit_value_16,
                    Width = 24,
                    Enabled = !_readOnly && _property.GetValue(_target) != null,
                };

                _editButton.Click += (sender, args) =>
                {
                    var item = _property.GetValue(_target, null);
                    if (item == null)
                        return;

                    var propertyWindow = new PropertyWindow()
                    {
                        StartPosition = FormStartPosition.CenterParent
                    };
                    propertyWindow.Initialize(item);
                    if (propertyWindow.ShowDialog(panel.FindForm()) == DialogResult.OK)
                    {
                        _textBox.Text = item.ToString();
                        RaiseControlChanged();
                    }
                };

                panel.Controls.Add(_editButton);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Delete))
            {
                _deleteButton = new Button
                {
                    Image = FlowBloxMainUIImages.remove_value_16,
                    Width = 24,
                    Enabled = !_readOnly && _property.GetValue(_target) != null
                };

                _deleteButton.Click += (sender, args) =>
                {
                    var item = _property.GetValue(_target, null);
                    if (item == null)
                        return;

                    if (!IsDeletable(item, panel.FindForm()))
                        return;

                    _textBox.Text = GetEmptyString();
                    _property.SetValue(_target, null);
                    DeleteInstance(item);
                    RaiseControlChanged();
                };

                panel.Controls.Add(_deleteButton);
            }

            return new Tuple<FlowBloxTextBox, FlowLayoutPanel>(_textBox, panel);
        }

        private string GetEmptyString()
        {
            return FlowBloxResourceUtil.GetLocalizedString("AssociationTextBox_Empty", typeof(FlowBloxTexts));
        }

        public override void Reload()
        {
            _textBox.Text = _property.GetValue(_target)?.ToString();
            base.Reload();
        }

        private void UpdateOperations()
        {
            if (_editButton != null)
                _editButton.Enabled = _property.GetValue(_target) != null;

            if (_deleteButton != null)
                _deleteButton.Enabled = _property.GetValue(_target) != null;
        }
    }
}
