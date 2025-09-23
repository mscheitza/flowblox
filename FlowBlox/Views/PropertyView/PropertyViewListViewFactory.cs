using FlowBlox.Core;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.WPF;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UI;
using FlowBlox.Core.Attributes;
using FlowBlox.UICore.Views;
using FlowBlox.UICore.Utilities;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Options = FlowBlox.Core.Attributes.UIOptions;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.UICore.Interfaces;
using System.Collections.Generic;
using FlowBlox.Core.Logging;

namespace FlowBlox.Views.PropertyView
{
    internal class PropertyViewListViewFactory : PropertyViewListViewBaseFactory
    {
        public const string ToolStripMenuItemName_ComponentActions = "toolStripMenuItemComponentActions";

        public PropertyViewListViewFactory(PropertyInfo property, object target, bool readOnly) : base(property, target, readOnly)
        {

        }

        private void AddInstance(object inst)
        {
            if (!_list.Contains(inst))
            {
                _list.Add(inst);
                _property.SetValue(_target, _list);
            }
            ReplaceOrAddListViewItem(inst, _listType);
            RaiseControlChanged();
        }

        protected override void DeleteInstance(object instance)
        {
            _list.Remove(instance);
            _property.SetValue(_target, _list);
            _listView.Items.Remove(_listView.Items.Cast<ListViewItem>().Single(x => x.Tag == instance));
            RaiseControlChanged();
            base.DeleteInstance(instance);
        }

        private bool CanMoveUp() => !_readOnly && _listView.SelectedIndices.Count > 0 && _listView.SelectedIndices[0] > 0;

        private bool CanMoveDown() => !_readOnly && _listView.SelectedIndices.Count > 0 && _listView.SelectedIndices[0] < _listView.Items.Count - 1;

        private void DoMoveUp()
        {
            if (CanMoveUp())
            {
                int index = _listView.SelectedIndices[0];
                var item = _listView.Items[index];
                _listView.Items.RemoveAt(index);
                _list.RemoveAt(index);
                _listView.Items.Insert(index - 1, item);
                _list.Insert(index - 1, item.Tag);
                _listView.SelectedItems.Clear();
                item.Selected = true;
                item.Focused = true;
                RaiseControlChanged();
            }
        }

        private void DoMoveDown()
        {
            if (CanMoveDown())
            {
                int index = _listView.SelectedIndices[0];
                var item = _listView.Items[index];
                _listView.Items.RemoveAt(index);
                _list.RemoveAt(index);
                _listView.Items.Insert(index + 1, item);
                _list.Insert(index + 1, item.Tag);
                _listView.SelectedItems.Clear();
                item.Selected = true;
                item.Focused = true;
                RaiseControlChanged();
            }
        }

        public ListView Create()
        {
            base.CreateListView();

            _listView.ContextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem itemAdd = null;
            ToolStripMenuItem itemRemove = null;
            ToolStripMenuItem itemLink = null;
            ToolStripMenuItem itemUnlink = null;
            ToolStripMenuItem itemEdit = null;

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Create))
            {
                itemAdd = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Add"), FlowBloxMainUIImages.add_value_16, (sender, args) =>
                {
                    var newInstance = CreateNewInstance(_listView.FindForm(), _listType);
                    if (newInstance == null)
                        return;

                    if (WinFormsPropertyWindowProvider.CreatePropertyWindowAndShowDialog(_listView, _target, newInstance, _readOnly))
                        AddInstance(newInstance);
                    else
                        DeleteInstance(newInstance);
                });

                itemAdd.ShortcutKeys = Keys.Control | Keys.N;

                _listView.ContextMenuStrip.Items.Add(itemAdd);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Delete))
            {
                itemRemove = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Remove"), FlowBloxMainUIImages.remove_value_16, (sender, args) =>
                {
                    if (_listView.SelectedItems.Count > 0)
                    {
                        if (_listView.SelectedItems.Cast<ListViewItem>().Any(x => !IsDeletable(x.Tag, _listView.FindForm())))
                            return;

                        foreach (var selectedItem in _listView.SelectedItems.Cast<ListViewItem>())
                        {
                            DeleteInstance(selectedItem.Tag);
                        }
                    }
                });

                itemRemove.ShortcutKeys = Keys.Delete;

                _listView.ContextMenuStrip.Items.Add(itemRemove);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Edit))
            {
                itemEdit = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Edit"), FlowBloxMainUIImages.edit_value_16, (sender, args) =>
                {
                    if (_listView.SelectedItems.Count > 0)
                    {
                        var selectedItem = _listView.SelectedItems[0];
                        var item = selectedItem.Tag;
                        if (WinFormsPropertyWindowProvider.CreatePropertyWindowAndShowDialog(_listView, _target, item, _readOnly))
                        {
                            ReplaceOrAddListViewItem(item, _listType);
                            RaiseControlChanged();
                        }
                    }
                });

                itemEdit.ShortcutKeys = Keys.Control | Keys.E;

                _listView.ContextMenuStrip.Items.Add(itemEdit);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Link))
            {
                itemLink = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Link"), FlowBloxMainUIImages.link_16, (sender, args) =>
                {
                    var filterMethod = GetSelectionFilterMethod(_target, _flowBlockUIAttribute.SelectionFilterMethod, _listType);
                    var items = filterMethod?.Invoke(_target, null) as IList;

                    if (_listType == typeof(FieldElement))
                    {
                        var fieldSelectionWindow = new FieldSelectionWindow(_target, items)
                        {
                            IsRequired = !_flowBlockUIAttribute.UiOptions.HasFlag(Options.FieldSelectionIsOptional)
                        };
                        fieldSelectionWindow.MultiSelect = false;
                        if (fieldSelectionWindow.ShowDialog(_listView.FindForm()) == DialogResult.OK)
                        {
                            FlowBlockHelper.ApplyFieldSelectionRequiredOption((BaseFlowBlock)_target, fieldSelectionWindow.SelectedFields, fieldSelectionWindow.IsRequired);
                            var inst = fieldSelectionWindow.SelectedFields.Single();
                            AddInstance(inst);
                        }
                    }
                    else
                    {
                        if (filterMethod == null)
                            throw new InvalidOperationException("There was no selection filter found.");

                        if (string.IsNullOrEmpty(_flowBlockUIAttribute.SelectionDisplayMember))
                            throw new InvalidOperationException("There was no selection display member found.");

                        var dialog = new MultiValueSelectionDialog(
                            FlowBloxResourceUtil.GetLocalizedString("PropertyViewListViewFactory_SelectionDialog_Title", typeof(FlowBloxMainUITexts)),
                            FlowBloxResourceUtil.GetLocalizedString("PropertyViewListViewFactory_SelectionDialog_Message", typeof(FlowBloxMainUITexts)),
                            new GenericSelectionHandler<object>(
                                items.Cast<object>(),
                                x => (string)ReflectionHelper
                                    .GetPropertyFromType(_listType, _flowBlockUIAttribute.SelectionDisplayMember)
                                    .GetValue(x)
                            ));

                        var result = WindowsFormWPFHelper.ShowDialog(dialog, _listView.FindForm());
                        if (result.HasValue && result.Value)
                        {
                            var inst = dialog.SelectedItem.Value;
                            AddInstance(inst);
                        }
                    }
                });

                itemLink.ShortcutKeys = Keys.Control | Keys.L;

                _listView.ContextMenuStrip.Items.Add(itemLink);
            }

            if (_flowBlockUIAttribute.Operations.HasFlag(UIOperations.Unlink))
            {
                itemUnlink = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("Unlink"), FlowBloxMainUIImages.unlink_16, (sender, args) =>
                {
                    if (_listView.SelectedItems.Count > 0)
                    {
                        foreach(var selectedItem in _listView.SelectedItems.Cast<ListViewItem>())
                        {
                            _list.Remove(selectedItem.Tag);
                            _property.SetValue(_target, _list);
                            _listView.Items.Remove(selectedItem);
                            RaiseControlChanged();
                        }
                    }
                });

                itemUnlink.ShortcutKeys = Keys.Control | Keys.U;

                _listView.ContextMenuStrip.Items.Add(itemUnlink);
            }

            ToolStripMenuItem itemMoveUp = null;
            ToolStripMenuItem itemMoveDown = null;
            if (_listViewAttribute.IsMovable)
            {
                itemMoveUp = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("MoveUp"), FlowBloxMainUIImages.moveup_16, (sender, args) => DoMoveUp());
                itemMoveUp.ShortcutKeys = Keys.Control | Keys.Up;

                itemMoveDown = new ToolStripMenuItem(FlowBloxResourceUtil.GetLocalizedString("MoveDown"), FlowBloxMainUIImages.movedown_16, (sender, args) => DoMoveDown());
                itemMoveDown.ShortcutKeys = Keys.Control | Keys.Down;

                _listView.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _listView.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { itemMoveUp, itemMoveDown });
            }

            ToolStripMenuItem componentMenuItem = null;

            var actionUpdate = () =>
            {
                if (itemRemove != null)
                    itemRemove.Enabled = !_readOnly && _listView.SelectedIndices.Count > 0;

                if (itemEdit != null)
                    itemEdit.Enabled = !_readOnly && _listView.SelectedIndices.Count == 1;

                if (itemUnlink != null)
                    itemUnlink.Enabled = !_readOnly && _listView.SelectedIndices.Count > 0;

                if (itemMoveUp != null)
                    itemMoveUp.Enabled = CanMoveUp();

                if (itemMoveDown != null)
                    itemMoveDown.Enabled = CanMoveDown();

                if (_listView.SelectedIndices.Count == 1)
                {
                    if (!_listView.ContextMenuStrip.Items.ContainsKey(ToolStripMenuItemName_ComponentActions))
                    {
                        var component = _listView.SelectedItems[0].Tag as IFlowBloxComponent;
                        if (component != null)
                        {
                            var toolstrupMenuItemsProvider = new UIActionsToolstripMenuItemsProvider();

                            List<ToolStripMenuItem> componentMenuItems;
                            try
                            {
                                componentMenuItems = toolstrupMenuItemsProvider.GetToolStripItemsForComponent(component);
                            }
                            catch(Exception e)
                            {
                                FlowBloxLogManager.Instance.GetLogger().Exception(e);

                                FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>().ShowMessageBox(
                                    FlowBloxResourceUtil.GetLocalizedString("PropertyViewListViewFactory_ComponentActionsLoadingFailure_Message", typeof(FlowBloxMainUITexts)),
                                    FlowBloxResourceUtil.GetLocalizedString("PropertyViewListViewFactory_ComponentActionsLoadingFailure_Title", typeof(FlowBloxMainUITexts)),
                                    UICore.Enums.FlowBloxMessageBoxTypes.Error);
                                
                                return;
                            }
                            
                            if (componentMenuItems.Any())
                            {
                                componentMenuItem = new ToolStripMenuItem(
                                    FlowBloxResourceUtil.GetLocalizedString("ComponentActions"),
                                    null,
                                    componentMenuItems.ToArray());
                                componentMenuItem.Name = ToolStripMenuItemName_ComponentActions;
                                _listView.ContextMenuStrip.Items.Add(componentMenuItem);
                                FlowBloxStyle.ApplyStyle(_listView);
                            }
                        }
                    }
                }
                else
                {
                    if (componentMenuItem != null)
                        _listView.ContextMenuStrip.Items.Remove(componentMenuItem);
                }
            };

            _listView.SelectedIndexChanged += (s, e) => actionUpdate();
            actionUpdate();

            return _listView;
        }

        private void ReplaceOrAddListViewItem(object item, Type listType)
        {
            int? insertIndex = null;

            var oldListViewItem = _listView.Items.Cast<ListViewItem>()
                .FirstOrDefault(lvItem => lvItem.Tag == item);

            if (oldListViewItem != null)
            {
                insertIndex = _listView.Items.IndexOf(oldListViewItem);
                _listView.Items.RemoveAt(insertIndex.Value);
            }

            InsertOrAddListViewItem(item, insertIndex);
        }
    }
}
