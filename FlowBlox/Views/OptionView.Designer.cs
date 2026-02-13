namespace FlowBlox.Views
{
    partial class OptionView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionView));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tbDescription = new System.Windows.Forms.RichTextBox();
            labelDescription = new System.Windows.Forms.Label();
            labelName = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            labelType = new System.Windows.Forms.Label();
            cbType = new System.Windows.Forms.ComboBox();
            labelIsPlaceholderEnabled = new System.Windows.Forms.Label();
            cbIsPlaceholderEnabled = new System.Windows.Forms.ComboBox();
            labelValue = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            cbBooleanValue = new System.Windows.Forms.ComboBox();
            tbValue = new System.Windows.Forms.TextBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btApply = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            pictureBox1 = new System.Windows.Forms.PictureBox();
            labelTitleHeader = new System.Windows.Forms.Label();
            openRegexDialog = new System.Windows.Forms.OpenFileDialog();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tbDescription, 1, 2);
            tableLayoutPanel1.Controls.Add(labelDescription, 1, 1);
            tableLayoutPanel1.Controls.Add(labelName, 1, 3);
            tableLayoutPanel1.Controls.Add(tbName, 1, 4);
            tableLayoutPanel1.Controls.Add(labelType, 1, 5);
            tableLayoutPanel1.Controls.Add(cbType, 1, 6);
            tableLayoutPanel1.Controls.Add(labelIsPlaceholderEnabled, 1, 7);
            tableLayoutPanel1.Controls.Add(cbIsPlaceholderEnabled, 1, 8);
            tableLayoutPanel1.Controls.Add(labelValue, 1, 9);
            tableLayoutPanel1.Controls.Add(panel1, 1, 10);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 11);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(labelTitleHeader, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Font = new System.Drawing.Font("Calibri", 9F);
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 12;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new System.Drawing.Size(604, 409);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tbDescription
            // 
            tbDescription.BackColor = System.Drawing.Color.White;
            tbDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            tbDescription.Font = new System.Drawing.Font("Calibri", 8.5F);
            tbDescription.Location = new System.Drawing.Point(65, 88);
            tbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbDescription.Name = "tbDescription";
            tbDescription.ReadOnly = true;
            tbDescription.Size = new System.Drawing.Size(535, 19);
            tbDescription.TabIndex = 26;
            tbDescription.Text = "";
            tbDescription.LinkClicked += tbDesc_LinkClicked;
            // 
            // labelDescription
            // 
            labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            labelDescription.Font = new System.Drawing.Font("Calibri", 9F);
            labelDescription.Location = new System.Drawing.Point(65, 60);
            labelDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new System.Drawing.Size(535, 25);
            labelDescription.TabIndex = 25;
            labelDescription.Tag = "style_header";
            labelDescription.Text = "labelDescription_Text";
            labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Dock = System.Windows.Forms.DockStyle.Fill;
            labelName.Location = new System.Drawing.Point(65, 110);
            labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelName.Name = "labelName";
            labelName.Size = new System.Drawing.Size(535, 25);
            labelName.TabIndex = 18;
            labelName.Tag = "style_header";
            labelName.Text = "labelName_Text";
            labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbName
            // 
            tbName.BackColor = System.Drawing.Color.White;
            tbName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tbName.Dock = System.Windows.Forms.DockStyle.Fill;
            tbName.Font = new System.Drawing.Font("Courier New", 8.25F);
            tbName.ForeColor = System.Drawing.Color.DarkSlateGray;
            tbName.Location = new System.Drawing.Point(65, 138);
            tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbName.Multiline = true;
            tbName.Name = "tbName";
            tbName.ReadOnly = true;
            tbName.Size = new System.Drawing.Size(535, 19);
            tbName.TabIndex = 17;
            tbName.TextChanged += tbName_TextChanged;
            // 
            // labelType
            // 
            labelType.AutoSize = true;
            labelType.Dock = System.Windows.Forms.DockStyle.Fill;
            labelType.Location = new System.Drawing.Point(65, 160);
            labelType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelType.Name = "labelType";
            labelType.Size = new System.Drawing.Size(535, 25);
            labelType.TabIndex = 24;
            labelType.Tag = "style_header";
            labelType.Text = "labelType_Text";
            labelType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbType
            // 
            cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            cbType.FormattingEnabled = true;
            cbType.Location = new System.Drawing.Point(65, 188);
            cbType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbType.Name = "cbType";
            cbType.Size = new System.Drawing.Size(318, 22);
            cbType.TabIndex = 23;
            cbType.SelectedIndexChanged += cbType_SelectedIndexChanged;
            // 
            // labelIsPlaceholderEnabled
            // 
            labelIsPlaceholderEnabled.AutoSize = true;
            labelIsPlaceholderEnabled.Dock = System.Windows.Forms.DockStyle.Fill;
            labelIsPlaceholderEnabled.Location = new System.Drawing.Point(65, 212);
            labelIsPlaceholderEnabled.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelIsPlaceholderEnabled.Name = "labelIsPlaceholderEnabled";
            labelIsPlaceholderEnabled.Size = new System.Drawing.Size(535, 25);
            labelIsPlaceholderEnabled.TabIndex = 30;
            labelIsPlaceholderEnabled.Tag = "style_header";
            labelIsPlaceholderEnabled.Text = "labelIsPlaceholderEnabled_Text";
            labelIsPlaceholderEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbIsPlaceholderEnabled
            // 
            cbIsPlaceholderEnabled.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbIsPlaceholderEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            cbIsPlaceholderEnabled.FormattingEnabled = true;
            cbIsPlaceholderEnabled.Location = new System.Drawing.Point(65, 240);
            cbIsPlaceholderEnabled.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbIsPlaceholderEnabled.Name = "cbIsPlaceholderEnabled";
            cbIsPlaceholderEnabled.Size = new System.Drawing.Size(318, 22);
            cbIsPlaceholderEnabled.TabIndex = 31;
            cbIsPlaceholderEnabled.SelectedIndexChanged += cbShowInFieldSelection_SelectedIndexChanged;
            // 
            // labelValue
            // 
            labelValue.AutoSize = true;
            labelValue.Dock = System.Windows.Forms.DockStyle.Fill;
            labelValue.Location = new System.Drawing.Point(65, 264);
            labelValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelValue.Name = "labelValue";
            labelValue.Size = new System.Drawing.Size(535, 29);
            labelValue.TabIndex = 14;
            labelValue.Tag = "style_header";
            labelValue.Text = "labelValue_Text";
            labelValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            panel1.Controls.Add(cbBooleanValue);
            panel1.Controls.Add(tbValue);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(65, 296);
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(535, 69);
            panel1.TabIndex = 3;
            // 
            // cbBooleanValue
            // 
            cbBooleanValue.Dock = System.Windows.Forms.DockStyle.Top;
            cbBooleanValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbBooleanValue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            cbBooleanValue.FormattingEnabled = true;
            cbBooleanValue.Location = new System.Drawing.Point(0, 0);
            cbBooleanValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbBooleanValue.Name = "cbBooleanValue";
            cbBooleanValue.Size = new System.Drawing.Size(535, 22);
            cbBooleanValue.TabIndex = 32;
            cbBooleanValue.Visible = false;
            cbBooleanValue.SelectedIndexChanged += cbBooleanValue_SelectedIndexChanged;
            // 
            // tbValue
            // 
            tbValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tbValue.Dock = System.Windows.Forms.DockStyle.Fill;
            tbValue.Font = new System.Drawing.Font("Courier New", 8.25F);
            tbValue.ForeColor = System.Drawing.Color.DarkSlateGray;
            tbValue.Location = new System.Drawing.Point(0, 0);
            tbValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbValue.Multiline = true;
            tbValue.Name = "tbValue";
            tbValue.Size = new System.Drawing.Size(535, 69);
            tbValue.TabIndex = 15;
            tbValue.TextChanged += tbValue_TextChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btApply);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(65, 371);
            flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(535, 35);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // btApply
            // 
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.ImageKey = "accept";
            btApply.ImageList = imageList;
            btApply.Location = new System.Drawing.Point(420, 3);
            btApply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btApply.Name = "btApply";
            btApply.Size = new System.Drawing.Size(111, 30);
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
            imageList.Images.SetKeyName(0, "accept");
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new System.Drawing.Point(4, 3);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(53, 53);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // labelTitleHeader
            // 
            labelTitleHeader.AutoSize = true;
            labelTitleHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            labelTitleHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            labelTitleHeader.Location = new System.Drawing.Point(65, 0);
            labelTitleHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelTitleHeader.Name = "labelTitleHeader";
            labelTitleHeader.Size = new System.Drawing.Size(535, 60);
            labelTitleHeader.TabIndex = 2;
            labelTitleHeader.Text = "labelTitleHeader_Text";
            labelTitleHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openRegexDialog
            // 
            openRegexDialog.Filter = "Text-Dateien|*.txt|Alle Dateien|*.*";
            // 
            // OptionView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "OptionView";
            Size = new System.Drawing.Size(604, 409);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelTitleHeader;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbValue;
        private System.Windows.Forms.ComboBox cbBooleanValue;
        private System.Windows.Forms.Label labelValue;
        private System.Windows.Forms.OpenFileDialog openRegexDialog;
        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.ComboBox cbType;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.RichTextBox tbDescription;
        private System.Windows.Forms.ImageList imageList;

        private System.Windows.Forms.Label labelIsPlaceholderEnabled;
        private System.Windows.Forms.ComboBox cbIsPlaceholderEnabled;
    }
}
