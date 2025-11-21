using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Views
{
    internal partial class OptionWindow : Form
    {
        private FlowBloxOptions options = null;

        public OptionWindow(OptionElement selectedOption)
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            this.options = FlowBloxOptions.GetOptionInstance();
            this.InitializeOptions(selectedOption);
        }

        private void InitializeOptions(OptionElement preSelectedOptionElement)
        {
            List<OptionElement> optionElements = options.GetOptions();

            treeViewOptions.BeginUpdate();
            treeViewOptions.Nodes.Clear();

            foreach (var option in optionElements)
            {
                AddOptionToTree(option);
            }

            treeViewOptions.EndUpdate();

            if (preSelectedOptionElement != null)
            {
                var node = FindNodeByTag(treeViewOptions.Nodes, preSelectedOptionElement);
                if (node != null)
                {
                    treeViewOptions.SelectedNode = node;
                    node.EnsureVisible();
                }
            }
        }

        /// <summary>
        /// Adds the given <see cref="OptionElement"/> to the TreeView,
        /// creating category and subcategory nodes as needed.
        /// Returns the final leaf node representing the option.
        /// </summary>
        /// <param name="option">The option to add to the tree.</param>
        /// <returns>The leaf <see cref="TreeNode"/> for the given option.</returns>
        private TreeNode AddOptionToTree(OptionElement option)
        {
            string[] parts = option.Name.Split('.');
            TreeNodeCollection currentNodes = treeViewOptions.Nodes;
            TreeNode currentNode = null;

            for (int i = 0; i < parts.Length; i++)
            {
                var existing = FindNodeByText(currentNodes, parts[i]);
                if (existing == null)
                {
                    existing = new TreeNode(parts[i]);
                    currentNodes.Add(existing);
                }

                currentNode = existing;
                currentNodes = existing.Nodes;
            }

            if (currentNode != null)
            {
                currentNode.Tag = option;
                currentNode.ImageKey = "option";
            }

            return currentNode;
        }

        private TreeNode FindNodeByText(TreeNodeCollection nodes, string text)
        {
            foreach (TreeNode n in nodes)
            {
                if (string.Equals(n.Text, text, StringComparison.OrdinalIgnoreCase))
                    return n;
            }
            return null;
        }

        private TreeNode FindNodeByTag(TreeNodeCollection nodes, OptionElement element)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Tag == element)
                    return n;

                var sub = FindNodeByTag(n.Nodes, element);
                if (sub != null)
                    return sub;
            }
            return null;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            string search = textBoxSearch.Text.Trim();
            ApplyTreeFilter(search);
        }

        private void ApplyTreeFilter(string search)
        {
            bool doExpand = !string.IsNullOrWhiteSpace(search);
            search = search.ToLowerInvariant();

            treeViewOptions.BeginUpdate();
            treeViewOptions.Nodes.Clear();

            foreach (var option in options.GetOptions())
            {
                if (string.IsNullOrEmpty(search) || option.Name.ToLowerInvariant().Contains(search))
                {
                    TreeNode addedNode = AddOptionToTree(option);
                    if (doExpand && addedNode != null)
                        ExpandParents(addedNode);
                }
            }

            treeViewOptions.EndUpdate();
        }

        /// <summary>
        /// Expands the specified node and all of its parent nodes up to the root.
        /// </summary>
        /// <param name="node">The node to expand.</param>
        private void ExpandParents(TreeNode node)
        {
            TreeNode current = node;
            while (current != null)
            {
                current.Expand();
                current = current.Parent;
            }
        }

        private void treeViewOptions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is OptionElement optionElement)
            {
                optionView1.Visible = true;
                optionView1.Initialize(optionElement);
                btDelete.Enabled = !optionElement.SystemOption;
            }
            else
            {
                optionView1.Visible = false;
                btDelete.Enabled = false;
            }
        }

        private void btAddOption_Click(object sender, EventArgs e)
        {
            List<OptionElement> OptionElements = options.GetOptions();
            OptionElement NewOptionElement = new OptionElement();

            EditValueWindow EditValue = new EditValueWindow(false, false);
            EditValue.ShowDialog(this);
            if (!string.IsNullOrEmpty(EditValue.GetValue()))
            {
                string OptionName = EditValue.GetValue();
                NewOptionElement.Name = OptionName;
                options.OptionCollection[OptionName] = NewOptionElement;
                OptionElements.Add(NewOptionElement);
                InitializeOptions(null);
            }
        }

        private void btDeleteOption_Click(object sender, EventArgs e)
        {
            var lvOption = treeViewOptions.SelectedNode;
            if (lvOption == null)
                return;

            List<OptionElement> optionElements = this.options.GetOptions();
            OptionElement OptionElement = (OptionElement)lvOption.Tag;
            this.options.OptionCollection.Remove(OptionElement.Name);
            InitializeOptions(null);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btRevertOptions_Click(object sender, EventArgs e)
        {
            string message = FlowBloxResourceUtil.GetLocalizedString(
                "OptionWindow_RevertOptions_Message",
                typeof(FlowBloxMainUITexts));

            string title = FlowBloxResourceUtil.GetLocalizedString(
                "OptionWindow_RevertOptions_Title",
                typeof(FlowBloxMainUITexts));

            DialogResult dialogResult = FlowBloxMessageBox.Show
            (
                this,
                message,
                title,
                FlowBloxMessageBox.Buttons.YesNoCancel,
                FlowBloxMessageBox.Icons.Question
            );

            if (dialogResult == DialogResult.Yes)
            {
                options.InitDefaults(true);
                options.Save();
                InitializeOptions(null);
            }
        }
    }
}
