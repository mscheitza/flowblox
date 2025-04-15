using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FlowBlox.Views.PropertyView
{
    public class PropertyViewDataGridFactory : WinFormsPropertyViewControlFactory, IValidatableFactory<DataGridView>
    {
        private bool _initialized;
        private Type _listType;
        private DataGridView _dataGridView;
        private Dictionary<object, DataRow> _originAssignments;
        private int? _movedToIndex;

        public PropertyViewDataGridFactory(PropertyInfo property, object target, bool readOnly) : base(property, target, readOnly)
        {
            
        }

        private readonly Dictionary<DataGridViewColumn, bool> _initializedCreatedDataGridColumns = new Dictionary<DataGridViewColumn, bool>();

        private void MoveDataRow(DataTable table, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || 
                oldIndex >= table.Rows.Count || 
                newIndex < 0 || 
                newIndex >= table.Rows.Count || 
                oldIndex == newIndex)
            {
                return;
            }

            _movedToIndex = newIndex;

            DataRow oldRow = table.Rows[oldIndex];
            DataRow newRow = table.NewRow();
            newRow.ItemArray = oldRow.ItemArray;
            table.Rows.Remove(oldRow);
            table.Rows.InsertAt(newRow, newIndex);
        }

        public DataGridView Create()
        {
            var propertyValue = _property.GetValue(_target);

            if (propertyValue is IList list)
            {
                var dataTable = GenericDataTableConverter.ConvertToDataTable(list, out Dictionary<object, DataRow> assignments);
                _dataGridView = new DataGridView 
                { 
                    DataSource = dataTable, 
                    Dock = DockStyle.Fill,
                    AllowUserToAddRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                _originAssignments = assignments;

                _dataGridView.ContextMenuStrip = new ContextMenuStrip();

                var itemAdd = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Add"), FlowBloxMainUIImages.add_value_16, new EventHandler((x, y) =>
                {
                    var dt = (DataTable)_dataGridView.DataSource;
                    var newRow = dt.NewRow();
                    dt.Rows.Add(newRow);
                    UpdateRowButtonTexts(_dataGridView);
                    RaiseControlChanged();
                }));
                itemAdd.ShortcutKeys = Keys.Control | Keys.N;
                
                var removeItem = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Remove"), FlowBloxMainUIImages.remove_value_16, new EventHandler((x, y) =>
                {
                    var dt = (DataTable)_dataGridView.DataSource;
                    foreach (var row in _dataGridView.SelectedRows.Cast<DataGridViewRow>())
                    {
                        var dataRow = dt.Rows[row.Index];
                        var originAssigments_Reversed = _originAssignments.ReverseDictionary();
                        if (!originAssigments_Reversed.ContainsKey(dataRow))
                            continue;

                        var instance = originAssigments_Reversed[dataRow];
                        if (!IsDeletable(instance, _dataGridView.FindForm()))
                            return;
                    }

                    foreach (var row in _dataGridView.SelectedRows.Cast<DataGridViewRow>())
                    {
                        dt.Rows.RemoveAt(row.Index);
                        UpdateRowButtonTexts(_dataGridView);
                        RaiseControlChanged();
                    }
                }));
                removeItem.ShortcutKeys = Keys.Delete;

                var upItem = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("MoveUp"), FlowBloxMainUIImages.moveup_16, new EventHandler((x, y) =>
                {
                    if (_dataGridView.SelectedRows.Count > 0)
                    {
                        var selectedIndex = _dataGridView.SelectedRows[0].Index;
                        if (selectedIndex > 0)
                        {
                            MoveDataRow(_dataGridView.DataSource as DataTable, selectedIndex, selectedIndex - 1);
                            _dataGridView.ClearSelection();
                            _dataGridView.CurrentCell = _dataGridView.Rows[selectedIndex - 1].Cells[0];
                            _dataGridView.Rows[selectedIndex - 1].Selected = true;
                        }
                    }
                }));
                upItem.ShortcutKeys = Keys.Control | Keys.Up;

                var downItem = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("MoveDown"), FlowBloxMainUIImages.movedown_16, new EventHandler((x, y) =>
                {
                    if (_dataGridView.SelectedRows.Count > 0)
                    {
                        var selectedIndex = _dataGridView.SelectedRows[0].Index;
                        if (selectedIndex < _dataGridView.Rows.Count - 1)
                        {
                            MoveDataRow(_dataGridView.DataSource as DataTable, selectedIndex, selectedIndex + 1);
                            _dataGridView.ClearSelection();
                            _dataGridView.Rows[selectedIndex + 1].Selected = true;
                        }
                    }
                }));
                downItem.ShortcutKeys = Keys.Control | Keys.Down;

                if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Create) == true)
                    _dataGridView.ContextMenuStrip.Items.Add(itemAdd);
                if (_flowBlockUIAttribute?.Operations.HasFlag(UIOperations.Delete) == true)
                    _dataGridView.ContextMenuStrip.Items.Add(removeItem);
                _dataGridView.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _dataGridView.ContextMenuStrip.Items.Add(upItem);
                _dataGridView.ContextMenuStrip.Items.Add(downItem);

                var actionUpdate = () =>
                {
                    var anySelected = _dataGridView.SelectedRows.Count > 0;
                    var singleSelected = _dataGridView.SelectedRows.Count == 1;

                    // Ändern des Status des "Entfernen"-Kontextmenüelements basierend auf der aktuellen Auswahl
                    removeItem.Enabled = anySelected;

                    // Ändern des Status des "Nach oben"-Kontextmenüelements basierend auf der aktuellen Auswahl
                    upItem.Enabled = singleSelected && _dataGridView.SelectedRows[0].Index > 0;

                    // Ändern des Status des "Nach unten"-Kontextmenüelements basierend auf der aktuellen Auswahl
                    downItem.Enabled = singleSelected && _dataGridView.SelectedRows[0].Index < _dataGridView.Rows.Count - 1;
                };

                _dataGridView.SelectionChanged += (s, e) => actionUpdate();
                actionUpdate();

                this._listType = list.GetType().GetGenericArguments()[0];

                _dataGridView.DataBindingComplete += (s, e) =>
                {
                    if (_initialized)
                        return;

                    _initialized = true;

                    var initialColumns = _dataGridView.Columns
                        .Cast<DataGridViewColumn>()
                        .ToList();

                    // Hinzufügen von Auswahlspalten und Setzen der Überschrift basierend auf dem DisplayAttribute
                    foreach (var column in initialColumns)
                    {
                        var propertyInfo = _listType.GetProperty(column.Name.Split('.')[0]); // Splitten des Namens auf das "." um den Original-Namen des Eigenschaft zu bekommen
                        if (propertyInfo == null)
                            continue;

                        var displayAttr = propertyInfo.GetCustomAttribute<DisplayAttribute>();
                        if (displayAttr == null)
                            continue;

                        column.HeaderText = FlowBloxResourceUtil.GetDisplayName(displayAttr);
                        column.ReadOnly = propertyInfo.CanWrite == false || _readOnly;

                        var flowBlockUIAttribute = propertyInfo.GetCustomAttribute<FlowBlockUIAttribute>();
                        column.Visible = flowBlockUIAttribute?.Visible ?? true;

                        if (flowBlockUIAttribute != null)
                        {
                            column.ReadOnly = _readOnly;

                            if (flowBlockUIAttribute.Factory == UIFactory.Association)
                            {
                                var buttonColumn = new DataGridViewButtonColumn();
                                buttonColumn.Name = column.Name + '.' + GlobalConstants.DataGridViewColumnAssociationSuffix;
                                buttonColumn.HeaderText = column.HeaderText + " - " + FlowBloxResourceUtil.GetLocalizedString("Global_Selection");
                                buttonColumn.DisplayIndex = column.DisplayIndex + 1;

                                column.Visible = false; // Setze die Originalspalte auf unsichtbar

                                _dataGridView.RowsAdded += (s, e) =>
                                {
                                    UpdateRowButtonTexts(_dataGridView);
                                };

                                _dataGridView.Columns.Add(buttonColumn);

                                _dataGridView.CellClick += (s, e) =>
                                {
                                    if (e.ColumnIndex < 0)
                                        return;

                                    if (e.RowIndex < 0)
                                        return;

                                    if (_dataGridView.Columns[e.ColumnIndex] == buttonColumn)
                                    {
                                        DataGridViewRow row = _dataGridView.Rows[e.RowIndex];
                                        var item = row.Cells[column.Name].Value;
                                        if (item is DBNull)
                                        {
                                            item = CreateNewInstance(_dataGridView.FindForm(), propertyInfo.PropertyType);
                                        }
                                        var propertyWindow = new PropertyWindow()
                                        {
                                            StartPosition = FormStartPosition.CenterParent
                                        };
                                        propertyWindow.Initialize(item);
                                        if (propertyWindow.ShowDialog(_dataGridView.FindForm()) == DialogResult.OK)
                                        {
                                            // Update the button text after the dialog is closed
                                            row.Cells[e.ColumnIndex].Value = item.ToString();
                                            row.Cells[column.Name].Value = item;
                                        }
                                    }
                                };
                            }
                            else if (!string.IsNullOrEmpty(flowBlockUIAttribute.SelectionFilterMethod))
                            {
                                var filterMethod = GetSelectionFilterMethod(_target, flowBlockUIAttribute.SelectionFilterMethod, _listType);
                                if (filterMethod != null)
                                {
                                    var items = filterMethod.Invoke(_target, null) as IList;

                                    // Erstelle Mapping zwischen DisplayName und Item
                                    var valueOverrides = new Dictionary<object, object>();
                                    
                                    foreach (var item in items)
                                    {
                                        var pi = item.GetType().GetProperty(flowBlockUIAttribute.SelectionDisplayMember);
                                        var displayName = pi.GetValue(item).ToString();
                                        valueOverrides[displayName] = item;
                                    }

                                    // Füge eine Auswahlspalte hinzu
                                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn
                                    {
                                        Name = column.Name + '.' + GlobalConstants.DataGridViewColumnSelectionSuffix,
                                        HeaderText = column.HeaderText,
                                        DataSource = items,
                                        DisplayMember = flowBlockUIAttribute.SelectionDisplayMember,
                                        ValueMember = flowBlockUIAttribute.SelectionDisplayMember,
                                        DefaultCellStyle = new DataGridViewCellStyle { NullValue = "Bitte auswählen" }
                                    };
                                    comboBoxColumn.DisplayIndex = column.DisplayIndex;
                                    _dataGridView.Columns.Add(comboBoxColumn);

                                    column.Visible = false; // Setze die Originalspalte auf unsichtbar

                                    SetUpComboBoxColumnEventing(comboBoxColumn, column.Name, valueOverrides, flowBlockUIAttribute.SelectionDisplayMember);
                                }
                            }
                        }

                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn
                            {
                                Name = column.Name + '.' + GlobalConstants.DataGridViewColumnSelectionSuffix,
                                HeaderText = column.HeaderText,
                                DataSource = Enum.GetValues(propertyInfo.PropertyType).Cast<Enum>().Select(e => e.GetDisplayName()).ToList(),
                                DefaultCellStyle = new DataGridViewCellStyle { NullValue = "Bitte auswählen" }
                            };
                            _dataGridView.Columns.Add(comboBoxColumn);
                            comboBoxColumn.DisplayIndex = column.DisplayIndex;
                            column.Visible = false; // Setze die Originalspalte auf unsichtbar

                            var valueOverrides = Enum.GetValues(propertyInfo.PropertyType)
                               .Cast<Enum>()
                               .ToDictionary(e => (object)e.GetDisplayName(), e => (object)Convert.ToInt32(e));

                            SetUpComboBoxColumnEventing(comboBoxColumn, column.Name, valueOverrides);
                        }
                    }

                    if (_flowBlockUIAttribute.UiOptions.HasFlag(UIOptions.EnableFieldSelection))
                    {
                        _dataGridView.EditingControlShowing += (sender, e) =>
                        {
                            if (_dataGridView.CurrentCell.OwningColumn is DataGridViewTextBoxColumn)
                            {
                                TextBox textBox = e.Control as TextBox;
                                if (textBox == null)
                                    return;

                                textBox.KeyDown -= EditingControl_KeyDown;
                                textBox.KeyDown += EditingControl_KeyDown;
                            }
                        };
                    }

                    _dataGridView.DataError += _dataGridView_DataError;

                    _dataGridView.CellClick += (s, e) =>
                    {
                        if (e.ColumnIndex < 0)
                            return;

                        if (e.RowIndex < 0)
                            return;

                        if (_dataGridView.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
                        {
                            _dataGridView.BeginEdit(true);
                            ((ComboBox)_dataGridView.EditingControl).DroppedDown = true;
                        }
                    };

                    _dataGridView.RowsRemoved += (s, e) =>
                    {
                        FlushChanges();
                    };

                    _dataGridView.CellValueChanged += (s, e) =>
                    {
                        FlushChanges();
                    };
                };

                return _dataGridView;
            }
            else
            {
                throw new InvalidOperationException("Cannot use GridView factory for non-list properties.");
            }
        }

        private void _dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            var logger = FlowBloxLogManager.Instance.GetLogger();
            logger.Error($"An error occurred in the DataGridView at row {e.RowIndex}, column {e.ColumnIndex}.", e.Exception);
        }

        private void EditingControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                OnFieldSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void OnFieldSelection()
        {
            var flowBlock = _target as BaseFlowBlock;
            FieldSelectionWindow fieldSelectionWindow = new FieldSelectionWindow(flowBlock)
            {
                IsRequired = !_flowBlockUIAttribute.UiOptions.HasFlag(UIOptions.FieldSelectionIsOptional)
            };
            if (fieldSelectionWindow.ShowDialog(_dataGridView.FindForm()) == DialogResult.OK)
            {
                string selectedFields = string.Concat(fieldSelectionWindow.SelectedFields.Select(x => x.FullyQualifiedName));
                var textBox = _dataGridView.EditingControl as TextBox;
                if (textBox != null && textBox.SelectionStart >= 0)
                {
                    int selectionStart = textBox.SelectionStart;
                    textBox.Text = textBox.Text.Insert(selectionStart, selectedFields);
                    textBox.SelectionStart = selectionStart + selectedFields.Length;
                }
            }
        }

        private void FlushChanges()
        {
            if (!_initializedCreatedDataGridColumns.All(x => x.Value))
                return;

            var list = GenericDataTableConverter.ConvertToList((DataTable)_dataGridView.DataSource, _listType, out var updatedAssignments, _originAssignments);
            foreach(var updatedAssignment in updatedAssignments)
            {
                _originAssignments.TryAdd(updatedAssignment.Value, updatedAssignment.Key);
            }

            if (_property.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
            {
                Type itemType = _property.PropertyType.GenericTypeArguments.First();
                Type collectionType = typeof(ObservableCollection<>).MakeGenericType(itemType);
                var observableCollection = (IList)Activator.CreateInstance(collectionType);
                foreach(var listItem in list)
                {
                    observableCollection.Add(listItem);
                }
                _property.SetValue(_target, observableCollection); 
            }
            else
            {
                _property.SetValue(_target, list);
            }
            RaiseControlChanged();
        }

        private void SetUpComboBoxColumnEventing(DataGridViewComboBoxColumn comboBoxColumn, string originalColumnName, IDictionary<object, object> valueOverrides = null, string displayMember = null)
        {
            _initializedCreatedDataGridColumns[comboBoxColumn] = false;

            _dataGridView.DataBindingComplete += (s, e) =>
            {
                if (_movedToIndex != null && e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded)
                {
                    _dataGridView.Rows[_movedToIndex.Value].CopyCellValueForRow(originalColumnName, comboBoxColumn.Name, valueOverrides?.ReverseDictionary(), displayMember: displayMember);
                    _movedToIndex = null;
                }

                if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset)
                    _initializedCreatedDataGridColumns[comboBoxColumn] = false;

                if (_initializedCreatedDataGridColumns.All(x => x.Value))
                    return;

                _dataGridView.CopyCellValue(originalColumnName, comboBoxColumn.Name, valueOverrides?.ReverseDictionary(), displayMember: displayMember);
                _initializedCreatedDataGridColumns[comboBoxColumn] = true;
            };

            _dataGridView.CellValueChanged += (s, e) =>
            {
                if (!_initializedCreatedDataGridColumns.All(x => x.Value))
                    return;

                if (e.ColumnIndex == comboBoxColumn.Index)
                {
                    _dataGridView.CopyCellValue(comboBoxColumn.Name, originalColumnName, valueOverrides);
                }
            };
        }

        public override void Reload()
        {
            var propertyValue = _property.GetValue(_target);
            if (propertyValue is IList list)
            {
                var dataTable = GenericDataTableConverter.ConvertToDataTable(list, out Dictionary<object, DataRow> assignments);
                _dataGridView.DataSource = dataTable;
                _originAssignments = assignments;
            }
        }

        private void UpdateRowButtonTexts(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                foreach(DataGridViewColumn column in _dataGridView.Columns)
                {
                    string buttonColumnName = column.Name + '.' + GlobalConstants.DataGridViewColumnAssociationSuffix;
                    var buttonColumn = _dataGridView.Columns.Cast<DataGridViewColumn>().FirstOrDefault(x => x.Name == buttonColumnName);
                    if (buttonColumn == null)
                        continue;

                    if (row.Cells[column.Name].Value is DBNull)
                        row.Cells[buttonColumn.Name].Value = FlowBloxResourceUtil.GetLocalizedString("Global_Create");
                    else
                        row.Cells[buttonColumn.Name].Value = row.Cells[column.Name].Value.ToString();
                }
            }
        }

        public bool Validate(DataGridView dataGridView)
        {
            bool isValid = true;

            var assignments = new Dictionary<DataRow, object>();
            var list = GenericDataTableConverter.ConvertToList((DataTable)_dataGridView.DataSource, _listType, out assignments);

            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                if (!row.IsNewRow)  // Exclude the 'new row' at the end
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        // If the cell is not associated with a data-bound item, skip validation
                        if (string.IsNullOrEmpty(cell.OwningColumn.DataPropertyName) || row.DataBoundItem == null)
                            continue;

                        var view = (DataRowView)row.DataBoundItem;
                        var dataRow = view.Row;
                        var item = assignments[dataRow];

                        var targetType = item.GetType();
                        var property = targetType.GetProperty(cell.OwningColumn.DataPropertyName);
                        var value = property.GetValue(item, null);

                        var context = new ValidationContext(item) { MemberName = cell.OwningColumn.DataPropertyName };
                        var results = new List<ValidationResult>();

                        // Check whether the cell's value is valid
                        if (!Validator.TryValidateProperty(value, context, results))
                        {
                            // If the validation failed, mark the cell
                            cell.Style.BackColor = Color.LightSalmon;
                            cell.ErrorText = string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage));
                            isValid = false;
                        }
                        else
                        {
                            // If the validation succeeded, unmark the cell
                            cell.Style.BackColor = Color.White;
                            cell.ErrorText = string.Empty;
                        }
                    }
                }
            }
            return isValid;
        }
    }
}
