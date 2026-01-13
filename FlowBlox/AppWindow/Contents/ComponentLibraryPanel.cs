using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Drawing;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Util.WPF;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public partial class ComponentLibraryPanel : DockContent
    {
        private FlowBloxProject _project;

        private static bool TypeMatchesFilter(Type type, string filter)
        {
            if (type == null) 
                throw new ArgumentNullException(nameof(type));

            if (string.IsNullOrWhiteSpace(filter))
                return true;

            if (type.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute == null)
                return false;

            var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false);
            if (!string.IsNullOrEmpty(displayName) &&
                displayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        public ComponentLibraryPanel()
        {
            InitializeComponent();
            ControlHelper.EnableDoubleBuffer(treeView_Library);
            InitializeProject();
            InitializeLibrary();
            FlowBloxUILocalizationUtil.Localize(this);
            FlowBloxProjectManager.Instance.ProjectChanged += OnProjectChanged;
        }

        private const string CategoryImageKey = "Category";

        private void InitializeLibrary()
        {
            treeView_Library.Nodes.Clear();

            if (_project == null)
                return;

            // Determine filter expression
            string filterExpression = tbFilter.Text;

            // Group all FlowBlocks by category
            var flowBlocksByCategory = _project.CreateInstances<BaseFlowBlock>(type => TypeMatchesFilter(type, filterExpression))
                .GroupBy(fb => fb.GetCategory())
                .ToDictionary(g => g.Key, g => g.ToList());

            // All root categories (ParentCategory == null), sorted
            var rootCategories = FlowBlockCategory.GetAll()
                .Where(c => c.ParentCategory == null)
                .OrderBy(c => c.DisplayName);

            foreach (var rootCategory in rootCategories)
            {
                var categoryNode = BuildCategoryNodeRecursive(rootCategory, flowBlocksByCategory);
                if (categoryNode != null)
                    treeView_Library.Nodes.Add(categoryNode);
            }
            treeView_Library.ExpandAll();
        }

        private TreeNode BuildCategoryNodeRecursive(FlowBlockCategory category, Dictionary<FlowBlockCategory, List<BaseFlowBlock>> flowBlocksByCategory)
        {
            // Create an empty image for the category and add it to the image list
            if (!imageList_Library_Icon.Images.ContainsKey(CategoryImageKey))
                imageList_Library_Icon.Images.Add(CategoryImageKey,
                    ImageHelper.CopyImage(FlowBloxMainUIImages.library_category_16, imageList_Library_Icon.ImageSize.Width, imageList_Library_Icon.ImageSize.Height));

            var categoryNode = new TreeNode(category.DisplayName)
            {
                ImageKey = CategoryImageKey,
                SelectedImageKey = CategoryImageKey
            };

            var childCategories = FlowBlockCategory.GetAll()
                .Where(c => c.ParentCategory == category)
                .OrderBy(c => c.DisplayName);

            // If no child categories or flow blocks are present: Hide
            List<BaseFlowBlock> blocks;
            var hasFlowBlocks = flowBlocksByCategory.TryGetValue(category, out blocks);
            if (!childCategories.Any() && !hasFlowBlocks)
                return null;

            foreach (var childCategory in childCategories)
            {
                var childNode = BuildCategoryNodeRecursive(childCategory, flowBlocksByCategory);
                if (childNode != null)
                    categoryNode.Nodes.Add(childNode);
            }

            if (hasFlowBlocks)
            {
                foreach (var block in blocks.OrderBy(b => FlowBloxComponentHelper.GetDisplayName(b)))
                {
                    var blockNode = CreateFlowBlockNode(block);
                    categoryNode.Nodes.Add(blockNode);
                }
            }

            // If no nodes are present: Hide
            if (categoryNode.Nodes.Count == 0)
                return null;

            return categoryNode;
        }

        private TreeNode CreateFlowBlockNode(BaseFlowBlock flowBlock)
        {
            string typeKey = flowBlock.GetType().FullName;

            if (!imageList_Library_Icon.Images.ContainsKey(typeKey))
            {
                if (flowBlock.Icon16 != null)
                {
                    var image = SkiaToSystemDrawingHelper.ToSystemDrawingImage(flowBlock.Icon16);

                    var resizedImage = ImageHelper.CopyImage(image,
                        imageList_Library_Icon.ImageSize.Width,
                        imageList_Library_Icon.ImageSize.Height);

                    imageList_Library_Icon.Images.Add(typeKey, resizedImage);
                }
            }

            return new TreeNode(FlowBloxComponentHelper.GetDisplayName(flowBlock))
            {
                Tag = flowBlock,
                Name = typeKey,
                ImageKey = typeKey,
                SelectedImageKey = typeKey
            };
        }


        private void InitializeProject()
        {
            _project = FlowBloxProjectManager.Instance.ActiveProject;

            if (_project != null)
            {
                _project.ExtensionsReloaded += OnExtensionsReloaded;
                _project.BeforeUnloadExtension += OnBeforeUnloadExtension;
            }
        }

        private void OnProjectChanged(object sender, ProjectChangedEventArgs eventArgs)
        {
            if (_project != null)
            {
                _project.ExtensionsReloaded -= OnExtensionsReloaded;
                _project.BeforeUnloadExtension -= OnBeforeUnloadExtension;
            }

            _project = eventArgs.NewProject;

            if (_project != null)
            {
                _project.ExtensionsReloaded += OnExtensionsReloaded;
                _project.BeforeUnloadExtension += OnBeforeUnloadExtension;
            }

            InitializeLibrary();
        }

        private HashSet<Type> GetAllFlowBlockTypes(AssemblyLoadContext loadContext)
        {
            var typesToUnload = loadContext.Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BaseFlowBlock).IsAssignableFrom(t))
                .ToHashSet();

            return typesToUnload;
        }

        private void OnBeforeUnloadExtension(object sender, AssemblyLoadContext loadContext)
        {
            var flowBlockTypes = GetAllFlowBlockTypes(loadContext);

            var nodesToRemove = new List<TreeNode>();
            foreach (TreeNode node in treeView_Library.Nodes)
            {
                CollectNodesToRemove(node, flowBlockTypes, nodesToRemove);
            }

            foreach (TreeNode node in nodesToRemove)
            {
                node.Tag = null;
                node.Remove();
            }
        }

        private void CollectNodesToRemove(TreeNode parentNode, HashSet<Type> flowBlockTypes, List<TreeNode> nodesToRemove)
        {
            foreach (TreeNode childNode in parentNode.Nodes)
            {
                if (childNode.Tag is BaseFlowBlock flowBlock)
                {
                    var flowBlockType = flowBlock.GetType();
                    if (flowBlockTypes.Contains(flowBlockType))
                    {
                        nodesToRemove.Add(childNode);
                    }
                }
                CollectNodesToRemove(childNode, flowBlockTypes, nodesToRemove);
            }
        }

        private void OnExtensionsReloaded(object sender, EventArgs e)
        {
            InitializeLibrary();
        }

        private void treeViewElements_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void treeViewElements_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        public void UpdateUI()
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            treeView_Library.Enabled = (project != null) && !AppWindow.Instance.IsRuntimeActive;
        }

        private void btManageExtensions_Click(object sender, EventArgs e)
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            var dialog = new ExtensionsWindow(project);
            var owner = ControlHelper.FindParentOfType<Form>(this, true);
            WindowsFormWPFHelper.ShowDialog(dialog, owner);
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
            if (e.Control && e.KeyCode == Keys.A)
            {
                tbFilter.SelectAll();
                e.SuppressKeyPress = true;
            }
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            InitializeLibrary();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                FlowBloxProjectManager.Instance.ProjectChanged -= OnProjectChanged;

                if (_project != null)
                    _project.ExtensionsReloaded -= OnExtensionsReloaded;
            }
            base.Dispose(disposing);
        }
    }
}
