namespace FlowBlox.Views
{
    partial class FieldSelectionWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldSelectionWindow));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btApply = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            btCancel = new System.Windows.Forms.Button();
            listViewFields = new System.Windows.Forms.ListView();
            chFlowBlock = new System.Windows.Forms.ColumnHeader();
            chField = new System.Windows.Forms.ColumnHeader();
            fieldView_imageList = new System.Windows.Forms.ImageList(components);
            lbFieldDefinitions = new System.Windows.Forms.Label();
            lbDescription = new System.Windows.Forms.Label();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            cbRequired = new FlowBlox.Core.Components.FlowBloxCheckBox();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 299F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 4);
            tableLayoutPanel1.Controls.Add(listViewFields, 0, 2);
            tableLayoutPanel1.Controls.Add(lbFieldDefinitions, 0, 1);
            tableLayoutPanel1.Controls.Add(lbDescription, 0, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel2, 0, 3);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new System.Drawing.Size(590, 471);
            tableLayoutPanel1.TabIndex = 0;
            tableLayoutPanel1.Paint += tableLayoutPanel1_Paint;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btApply);
            flowLayoutPanel1.Controls.Add(btCancel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(4, 434);
            flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(582, 34);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // btApply
            // 
            btApply.Enabled = false;
            btApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.ImageKey = "accept";
            btApply.ImageList = imageList;
            btApply.Location = new System.Drawing.Point(468, 3);
            btApply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btApply.Name = "btApply";
            btApply.Size = new System.Drawing.Size(110, 30);
            btApply.TabIndex = 0;
            btApply.Text = "btApply_Text";
            btApply.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btApply.UseVisualStyleBackColor = true;
            btApply.Click += btApply_Click;
            // 
            // imageList
            // 
            imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = System.Drawing.Color.Transparent;
            imageList.Images.SetKeyName(0, "cancel");
            imageList.Images.SetKeyName(1, "accept");
            // 
            // btCancel
            // 
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btCancel.ImageKey = "cancel";
            btCancel.ImageList = imageList;
            btCancel.Location = new System.Drawing.Point(350, 3);
            btCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // listViewFields
            // 
            listViewFields.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { chFlowBlock, chField });
            listViewFields.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewFields.FullRowSelect = true;
            listViewFields.GridLines = true;
            listViewFields.Location = new System.Drawing.Point(4, 83);
            listViewFields.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listViewFields.Name = "listViewFields";
            listViewFields.Size = new System.Drawing.Size(582, 315);
            listViewFields.SmallImageList = fieldView_imageList;
            listViewFields.TabIndex = 6;
            listViewFields.UseCompatibleStateImageBehavior = false;
            listViewFields.View = System.Windows.Forms.View.Details;
            listViewFields.SelectedIndexChanged += listViewFields_SelectedIndexChanged;
            listViewFields.DoubleClick += listViewFields_DoubleClick;
            // 
            // chFlowBlock
            // 
            chFlowBlock.Text = "Flow-Block";
            chFlowBlock.Width = 278;
            // 
            // chField
            // 
            chField.Text = "Field";
            chField.Width = 300;
            // 
            // fieldView_imageList
            // 
            fieldView_imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            fieldView_imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("fieldView_imageList.ImageStream");
            fieldView_imageList.TransparentColor = System.Drawing.Color.Transparent;
            fieldView_imageList.Images.SetKeyName(0, "connected");
            fieldView_imageList.Images.SetKeyName(1, "disconnected");
            fieldView_imageList.Images.SetKeyName(2, "user");
            // 
            // lbFieldDefinitions
            // 
            lbFieldDefinitions.AutoSize = true;
            lbFieldDefinitions.BackColor = System.Drawing.SystemColors.Control;
            lbFieldDefinitions.Dock = System.Windows.Forms.DockStyle.Fill;
            lbFieldDefinitions.ForeColor = System.Drawing.SystemColors.ControlText;
            lbFieldDefinitions.Location = new System.Drawing.Point(4, 55);
            lbFieldDefinitions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbFieldDefinitions.Name = "lbFieldDefinitions";
            lbFieldDefinitions.Size = new System.Drawing.Size(582, 25);
            lbFieldDefinitions.TabIndex = 9;
            lbFieldDefinitions.Tag = "style_header";
            lbFieldDefinitions.Text = "lbFieldDefinitions_Text";
            lbFieldDefinitions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbDescription
            // 
            lbDescription.AutoSize = true;
            lbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            lbDescription.Font = new System.Drawing.Font("Calibri", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lbDescription.Location = new System.Drawing.Point(4, 0);
            lbDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new System.Drawing.Size(582, 55);
            lbDescription.TabIndex = 10;
            lbDescription.Text = "lbDescription_Text";
            lbDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(cbRequired);
            flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new System.Drawing.Point(3, 404);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(584, 24);
            flowLayoutPanel2.TabIndex = 11;
            // 
            // cbRequired
            // 
            cbRequired.Checked = true;
            cbRequired.Location = new System.Drawing.Point(460, 3);
            cbRequired.Name = "cbRequired";
            cbRequired.Size = new System.Drawing.Size(121, 24);
            cbRequired.TabIndex = 1;
            cbRequired.Text = "cbRequired_Text";
            // 
            // FieldSelectionWindow
            // 
            AcceptButton = btApply;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(590, 471);
            Controls.Add(tableLayoutPanel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FieldSelectionWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "FieldSelectionWindow_Text";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView listViewElements;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader ColumnName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.ListView listViewFields;
        private System.Windows.Forms.ColumnHeader chField;
        private System.Windows.Forms.Label lbFieldDefinitions;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.ColumnHeader chFlowBlock;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private Core.Components.FlowBloxCheckBox cbRequired;
        private System.Windows.Forms.ImageList fieldView_imageList;
        private System.Windows.Forms.ImageList imageList;
    }
}