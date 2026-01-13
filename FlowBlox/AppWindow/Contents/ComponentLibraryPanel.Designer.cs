using FlowBlox.Core;

namespace FlowBlox.AppWindow.Contents
{
    partial class ComponentLibraryPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            treeView_Library = new System.Windows.Forms.TreeView();
            imageList_Library_Icon = new System.Windows.Forms.ImageList(components);
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tbFilter = new System.Windows.Forms.TextBox();
            toolStrip = new System.Windows.Forms.ToolStrip();
            btManageExtensions = new System.Windows.Forms.ToolStripButton();
            tableLayoutPanel1.SuspendLayout();
            toolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // treeView_Library
            // 
            treeView_Library.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView_Library.FullRowSelect = true;
            treeView_Library.ImageIndex = 0;
            treeView_Library.ImageList = imageList_Library_Icon;
            treeView_Library.Location = new System.Drawing.Point(3, 58);
            treeView_Library.Name = "treeView_Library";
            treeView_Library.SelectedImageIndex = 0;
            treeView_Library.Size = new System.Drawing.Size(794, 364);
            treeView_Library.TabIndex = 14;
            treeView_Library.ItemDrag += treeViewElements_ItemDrag;
            treeView_Library.DragEnter += treeViewElements_DragEnter;
            // 
            // imageList_Library_Icon
            // 
            imageList_Library_Icon.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList_Library_Icon.ImageSize = new System.Drawing.Size(24, 24);
            imageList_Library_Icon.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tbFilter, 0, 1);
            tableLayoutPanel1.Controls.Add(treeView_Library, 0, 2);
            tableLayoutPanel1.Controls.Add(toolStrip, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(800, 425);
            tableLayoutPanel1.TabIndex = 15;
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            tbFilter.Location = new System.Drawing.Point(3, 28);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(794, 23);
            tbFilter.TabIndex = 16;
            tbFilter.TextChanged += tbFilter_TextChanged;
            tbFilter.KeyDown += tbFilter_KeyDown;
            // 
            // toolStrip
            // 
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btManageExtensions });
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new System.Drawing.Size(800, 25);
            toolStrip.TabIndex = 15;
            toolStrip.Text = "toolStrip1";
            // 
            // btManageExtensions
            // 
            btManageExtensions.Image = FlowBloxMainUIImages.extension_16;
            btManageExtensions.ImageTransparentColor = System.Drawing.Color.Magenta;
            btManageExtensions.Name = "btManageExtensions";
            btManageExtensions.Size = new System.Drawing.Size(162, 22);
            btManageExtensions.Text = "btManageExtensions_Text";
            btManageExtensions.Click += btManageExtensions_Click;
            // 
            // ComponentLibraryPanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "ComponentLibraryPanel";
            Padding = new System.Windows.Forms.Padding(0, 25, 0, 0);
            Text = "Komponenten-Bibliothek";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TreeView treeView_Library;
        private System.Windows.Forms.ImageList imageList_Library_Icon;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btManageExtensions;
        private System.Windows.Forms.TextBox tbFilter;
    }
}