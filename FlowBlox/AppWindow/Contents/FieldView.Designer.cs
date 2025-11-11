namespace FlowBlox.Views
{
    partial class FieldView
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldView));
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            itmOpenFieldValue = new System.Windows.Forms.ToolStripMenuItem();
            itmCopy = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            itmRefresh = new System.Windows.Forms.ToolStripMenuItem();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            lvFields = new System.Windows.Forms.ListView();
            chFlowBlockName = new System.Windows.Forms.ColumnHeader();
            chFieldName = new System.Windows.Forms.ColumnHeader();
            chFieldValue = new System.Windows.Forms.ColumnHeader();
            tbFilter = new System.Windows.Forms.TextBox();
            flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            checkBoxShowFlowBlock = new FlowBlox.Core.Components.FlowBloxCheckBox();
            contextMenuStrip.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            flowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { itmOpenFieldValue, itmCopy, toolStripSeparator1, itmRefresh });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new System.Drawing.Size(275, 76);
            // 
            // itmOpenFieldValue
            // 
            itmOpenFieldValue.Image = (System.Drawing.Image)resources.GetObject("itmOpenFieldValue.Image");
            itmOpenFieldValue.Name = "itmOpenFieldValue";
            itmOpenFieldValue.ShortcutKeyDisplayString = "Double Click";
            itmOpenFieldValue.Size = new System.Drawing.Size(274, 22);
            itmOpenFieldValue.Text = "itmOpenFieldValue_Text";
            itmOpenFieldValue.Click += itmOpenFieldValue_Click;
            // 
            // itmCopy
            // 
            itmCopy.Image = (System.Drawing.Image)resources.GetObject("itmCopy.Image");
            itmCopy.Name = "itmCopy";
            itmCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            itmCopy.Size = new System.Drawing.Size(274, 22);
            itmCopy.Text = "itmCopy_Text";
            itmCopy.Click += itmCopy_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(271, 6);
            // 
            // itmRefresh
            // 
            itmRefresh.Image = (System.Drawing.Image)resources.GetObject("itmRefresh.Image");
            itmRefresh.Name = "itmRefresh";
            itmRefresh.ShortcutKeys = System.Windows.Forms.Keys.F5;
            itmRefresh.Size = new System.Drawing.Size(274, 22);
            itmRefresh.Text = "itmRefresh_Text";
            itmRefresh.Click += itmRefresh_Click;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(lvFields, 0, 1);
            tableLayoutPanel.Controls.Add(tbFilter, 0, 0);
            tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 2);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 3;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel.Size = new System.Drawing.Size(315, 589);
            tableLayoutPanel.TabIndex = 1;
            // 
            // lvFields
            // 
            lvFields.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { chFlowBlockName, chFieldName, chFieldValue });
            lvFields.ContextMenuStrip = contextMenuStrip;
            lvFields.Dock = System.Windows.Forms.DockStyle.Fill;
            lvFields.FullRowSelect = true;
            lvFields.GridLines = true;
            lvFields.Location = new System.Drawing.Point(3, 31);
            lvFields.MultiSelect = false;
            lvFields.Name = "lvFields";
            lvFields.OwnerDraw = true;
            lvFields.Size = new System.Drawing.Size(309, 525);
            lvFields.TabIndex = 0;
            lvFields.UseCompatibleStateImageBehavior = false;
            lvFields.View = System.Windows.Forms.View.Details;
            lvFields.DrawItem += lvFields_DrawItem;
            lvFields.DrawSubItem += lvFields_DrawSubItem;
            lvFields.SelectedIndexChanged += lvFields_SelectedIndexChanged;
            lvFields.DoubleClick += lvFields_DoubleClick;
            // 
            // chFlowBlockName
            // 
            chFlowBlockName.Text = "Flow-Block";
            chFlowBlockName.Width = 90;
            // 
            // chFieldName
            // 
            chFieldName.Text = "Field";
            chFieldName.Width = 115;
            // 
            // chFieldValue
            // 
            chFieldValue.Text = "Value";
            chFieldValue.Width = 115;
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            tbFilter.Location = new System.Drawing.Point(3, 3);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(309, 22);
            tbFilter.TabIndex = 1;
            tbFilter.TextChanged += tbFilter_TextChanged;
            tbFilter.KeyDown += tbFilter_KeyDown;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Controls.Add(checkBoxShowFlowBlock);
            flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel.Location = new System.Drawing.Point(3, 562);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new System.Drawing.Size(309, 24);
            flowLayoutPanel.TabIndex = 2;
            // 
            // checkBoxShowFlowBlock
            // 
            checkBoxShowFlowBlock.Checked = false;
            checkBoxShowFlowBlock.Location = new System.Drawing.Point(111, 3);
            checkBoxShowFlowBlock.Name = "checkBoxShowFlowBlock";
            checkBoxShowFlowBlock.Size = new System.Drawing.Size(195, 24);
            checkBoxShowFlowBlock.TabIndex = 0;
            checkBoxShowFlowBlock.Text = "checkBoxShowFlowBlock_Text";
            checkBoxShowFlowBlock.Click += checkBoxShowFlowBlock_Click;
            // 
            // FieldView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(tableLayoutPanel);
            Font = new System.Drawing.Font("Calibri", 9F);
            Name = "FieldView";
            Size = new System.Drawing.Size(315, 589);
            contextMenuStrip.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            flowLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem itmCopy;
        private System.Windows.Forms.ToolStripMenuItem itmOpenFieldValue;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem itmRefresh;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.ListView lvFields;
        private System.Windows.Forms.ColumnHeader chFieldName;
        private System.Windows.Forms.ColumnHeader chFieldValue;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.ColumnHeader chFlowBlockName;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private Core.Components.FlowBloxCheckBox checkBoxShowFlowBlock;
    }
}
