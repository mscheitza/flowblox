namespace FlowBlox.Views
{
    partial class InsertTextOrField
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertTextOrField));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btApply = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            btCancel = new System.Windows.Forms.Button();
            lbInsertText = new System.Windows.Forms.Label();
            lbInsertField = new System.Windows.Forms.Label();
            pbLogo_UserValue = new System.Windows.Forms.PictureBox();
            pbFieldValue = new System.Windows.Forms.PictureBox();
            panel1 = new System.Windows.Forms.Panel();
            tbSelectedField = new System.Windows.Forms.TextBox();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            btSelectField = new System.Windows.Forms.Button();
            lbParameterHeader = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            lbParameter = new System.Windows.Forms.Label();
            tbValue = new System.Windows.Forms.TextBox();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo_UserValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbFieldValue).BeginInit();
            panel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 6);
            tableLayoutPanel1.Controls.Add(lbInsertText, 1, 2);
            tableLayoutPanel1.Controls.Add(lbInsertField, 1, 4);
            tableLayoutPanel1.Controls.Add(pbLogo_UserValue, 0, 2);
            tableLayoutPanel1.Controls.Add(pbFieldValue, 0, 4);
            tableLayoutPanel1.Controls.Add(panel1, 1, 5);
            tableLayoutPanel1.Controls.Add(lbParameterHeader, 1, 0);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(lbParameter, 1, 1);
            tableLayoutPanel1.Controls.Add(tbValue, 1, 3);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new System.Drawing.Size(629, 282);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btApply);
            flowLayoutPanel1.Controls.Add(btCancel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(53, 232);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(573, 47);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // btApply
            // 
            btApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.ImageKey = "accept";
            btApply.ImageList = imageList;
            btApply.Location = new System.Drawing.Point(460, 3);
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
            btCancel.Location = new System.Drawing.Point(344, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // lbInsertText
            // 
            lbInsertText.AutoSize = true;
            lbInsertText.Dock = System.Windows.Forms.DockStyle.Fill;
            lbInsertText.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold);
            lbInsertText.Location = new System.Drawing.Point(53, 60);
            lbInsertText.Name = "lbInsertText";
            lbInsertText.Size = new System.Drawing.Size(573, 34);
            lbInsertText.TabIndex = 5;
            lbInsertText.Tag = "style_header";
            lbInsertText.Text = "lbInsertText_Text";
            lbInsertText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbInsertField
            // 
            lbInsertField.AutoSize = true;
            lbInsertField.Dock = System.Windows.Forms.DockStyle.Fill;
            lbInsertField.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold);
            lbInsertField.Location = new System.Drawing.Point(53, 133);
            lbInsertField.Name = "lbInsertField";
            lbInsertField.Size = new System.Drawing.Size(573, 34);
            lbInsertField.TabIndex = 6;
            lbInsertField.Tag = "style_header";
            lbInsertField.Text = "lbInsertField_Text";
            lbInsertField.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbLogo_UserValue
            // 
            pbLogo_UserValue.Dock = System.Windows.Forms.DockStyle.Fill;
            pbLogo_UserValue.Image = (System.Drawing.Image)resources.GetObject("pbLogo_UserValue.Image");
            pbLogo_UserValue.Location = new System.Drawing.Point(3, 63);
            pbLogo_UserValue.Name = "pbLogo_UserValue";
            pbLogo_UserValue.Size = new System.Drawing.Size(44, 28);
            pbLogo_UserValue.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pbLogo_UserValue.TabIndex = 7;
            pbLogo_UserValue.TabStop = false;
            // 
            // pbFieldValue
            // 
            pbFieldValue.Dock = System.Windows.Forms.DockStyle.Fill;
            pbFieldValue.Image = (System.Drawing.Image)resources.GetObject("pbFieldValue.Image");
            pbFieldValue.Location = new System.Drawing.Point(3, 136);
            pbFieldValue.Name = "pbFieldValue";
            pbFieldValue.Size = new System.Drawing.Size(44, 28);
            pbFieldValue.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pbFieldValue.TabIndex = 8;
            pbFieldValue.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbSelectedField);
            panel1.Controls.Add(flowLayoutPanel2);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(53, 170);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(573, 56);
            panel1.TabIndex = 12;
            // 
            // tbSelectedField
            // 
            tbSelectedField.Dock = System.Windows.Forms.DockStyle.Top;
            tbSelectedField.Font = new System.Drawing.Font("Courier New", 8.25F);
            tbSelectedField.Location = new System.Drawing.Point(0, 0);
            tbSelectedField.Name = "tbSelectedField";
            tbSelectedField.ReadOnly = true;
            tbSelectedField.Size = new System.Drawing.Size(573, 20);
            tbSelectedField.TabIndex = 0;
            tbSelectedField.TextChanged += tbSelectedField_TextChanged;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Controls.Add(btSelectField);
            flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowLayoutPanel2.Location = new System.Drawing.Point(0, 20);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            flowLayoutPanel2.Size = new System.Drawing.Size(573, 36);
            flowLayoutPanel2.TabIndex = 1;
            // 
            // btSelectField
            // 
            btSelectField.Image = (System.Drawing.Image)resources.GetObject("btSelectField.Image");
            btSelectField.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btSelectField.Location = new System.Drawing.Point(434, 3);
            btSelectField.Name = "btSelectField";
            btSelectField.Size = new System.Drawing.Size(136, 30);
            btSelectField.TabIndex = 4;
            btSelectField.Text = "btSelectField_Text";
            btSelectField.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btSelectField.UseVisualStyleBackColor = true;
            btSelectField.Click += btSelectField_Click;
            // 
            // lbParameterHeader
            // 
            lbParameterHeader.AutoSize = true;
            lbParameterHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            lbParameterHeader.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbParameterHeader.ForeColor = System.Drawing.SystemColors.ControlText;
            lbParameterHeader.Location = new System.Drawing.Point(53, 0);
            lbParameterHeader.Name = "lbParameterHeader";
            lbParameterHeader.Size = new System.Drawing.Size(573, 30);
            lbParameterHeader.TabIndex = 13;
            lbParameterHeader.Tag = "style_header";
            lbParameterHeader.Text = "lbParameterHeader_Text";
            lbParameterHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new System.Drawing.Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(44, 24);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 15;
            pictureBox1.TabStop = false;
            // 
            // lbParameter
            // 
            lbParameter.AutoSize = true;
            lbParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            lbParameter.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbParameter.ForeColor = System.Drawing.SystemColors.ControlText;
            lbParameter.Location = new System.Drawing.Point(53, 30);
            lbParameter.Name = "lbParameter";
            lbParameter.Size = new System.Drawing.Size(573, 30);
            lbParameter.TabIndex = 16;
            lbParameter.Text = "lbParameter_Text";
            lbParameter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbValue
            // 
            tbValue.Dock = System.Windows.Forms.DockStyle.Fill;
            tbValue.Location = new System.Drawing.Point(53, 97);
            tbValue.Name = "tbValue";
            tbValue.Size = new System.Drawing.Size(573, 22);
            tbValue.TabIndex = 17;
            tbValue.TextChanged += tbValue_TextChanged;
            // 
            // InsertTextOrField
            // 
            AcceptButton = btApply;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(629, 282);
            Controls.Add(tableLayoutPanel1);
            Font = new System.Drawing.Font("Calibri", 9F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "InsertTextOrField";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "InsertTextOrField_Text";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbLogo_UserValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbFieldValue).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbInsertText;
        private System.Windows.Forms.Label lbInsertField;
        private System.Windows.Forms.PictureBox pbLogo_UserValue;
        private System.Windows.Forms.PictureBox pbFieldValue;
        private System.Windows.Forms.Label lbInsertTextDescription;
        private System.Windows.Forms.Label lbInsertFieldDescrption;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbSelectedField;
        private System.Windows.Forms.Label lbParameterHeader;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Label lbParameter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btSelectField;
        private System.Windows.Forms.TextBox tbValue;
    }
}