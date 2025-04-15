using FlowBlox.Core.Components;

namespace FlowBlox.Views
{
    partial class EditValueWindow
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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditValueWindow));
            btCancel = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            btApply = new System.Windows.Forms.Button();
            pictureBox_Logo = new System.Windows.Forms.PictureBox();
            lbDescription = new System.Windows.Forms.Label();
            lbValue = new System.Windows.Forms.Label();
            textBox_Value = new System.Windows.Forms.TextBox();
            textBox_Parameter = new System.Windows.Forms.TextBox();
            lbParameter = new System.Windows.Forms.Label();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            panel_Value = new System.Windows.Forms.Panel();
            comboBox_Value = new System.Windows.Forms.ComboBox();
            cbMaskRegex = new FlowBloxCheckBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Logo).BeginInit();
            tableLayoutPanel.SuspendLayout();
            flowLayoutPanel.SuspendLayout();
            panel_Value.SuspendLayout();
            SuspendLayout();
            // 
            // btCancel
            // 
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btCancel.ImageKey = "cancel";
            btCancel.ImageList = imageList;
            btCancel.Location = new System.Drawing.Point(283, 3);
            btCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btAbort_Click;
            // 
            // imageList
            // 
            imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = System.Drawing.Color.Transparent;
            imageList.Images.SetKeyName(0, "cancel");
            imageList.Images.SetKeyName(1, "accept");
            // 
            // btApply
            // 
            btApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.ImageKey = "accept";
            btApply.ImageList = imageList;
            btApply.Location = new System.Drawing.Point(401, 3);
            btApply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btApply.Name = "btApply";
            btApply.Size = new System.Drawing.Size(110, 30);
            btApply.TabIndex = 0;
            btApply.Text = "btApply_Text";
            btApply.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btApply.UseVisualStyleBackColor = true;
            btApply.Click += btApply_Click;
            // 
            // pictureBox_Logo
            // 
            pictureBox_Logo.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox_Logo.Image = (System.Drawing.Image)resources.GetObject("pictureBox_Logo.Image");
            pictureBox_Logo.Location = new System.Drawing.Point(4, 3);
            pictureBox_Logo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox_Logo.Name = "pictureBox_Logo";
            pictureBox_Logo.Size = new System.Drawing.Size(53, 54);
            pictureBox_Logo.TabIndex = 9;
            pictureBox_Logo.TabStop = false;
            // 
            // lbDescription
            // 
            lbDescription.AutoSize = true;
            lbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            lbDescription.Font = new System.Drawing.Font("Calibri", 8.5F);
            lbDescription.Location = new System.Drawing.Point(65, 0);
            lbDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new System.Drawing.Size(515, 60);
            lbDescription.TabIndex = 10;
            lbDescription.Text = "lbDescription_Text";
            lbDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbValue
            // 
            lbValue.AutoSize = true;
            lbValue.Dock = System.Windows.Forms.DockStyle.Fill;
            lbValue.Location = new System.Drawing.Point(65, 111);
            lbValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbValue.Name = "lbValue";
            lbValue.Size = new System.Drawing.Size(515, 23);
            lbValue.TabIndex = 11;
            lbValue.Tag = "style_header";
            lbValue.Text = "lbValue_Text";
            lbValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_Value
            // 
            textBox_Value.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox_Value.Location = new System.Drawing.Point(0, 0);
            textBox_Value.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox_Value.Name = "textBox_Value";
            textBox_Value.Size = new System.Drawing.Size(515, 22);
            textBox_Value.TabIndex = 0;
            // 
            // textBox_Parameter
            // 
            textBox_Parameter.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox_Parameter.Location = new System.Drawing.Point(65, 86);
            textBox_Parameter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox_Parameter.Name = "textBox_Parameter";
            textBox_Parameter.ReadOnly = true;
            textBox_Parameter.Size = new System.Drawing.Size(515, 22);
            textBox_Parameter.TabIndex = 12;
            textBox_Parameter.Visible = false;
            // 
            // lbParameter
            // 
            lbParameter.AutoSize = true;
            lbParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            lbParameter.Location = new System.Drawing.Point(65, 60);
            lbParameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbParameter.Name = "lbParameter";
            lbParameter.Size = new System.Drawing.Size(515, 23);
            lbParameter.TabIndex = 13;
            lbParameter.Tag = "style_header";
            lbParameter.Text = "lbParameter_Text";
            lbParameter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbParameter.Visible = false;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(pictureBox_Logo, 0, 0);
            tableLayoutPanel.Controls.Add(textBox_Parameter, 1, 2);
            tableLayoutPanel.Controls.Add(lbValue, 1, 3);
            tableLayoutPanel.Controls.Add(lbDescription, 1, 0);
            tableLayoutPanel.Controls.Add(lbParameter, 1, 1);
            tableLayoutPanel.Controls.Add(flowLayoutPanel, 1, 6);
            tableLayoutPanel.Controls.Add(panel_Value, 1, 4);
            tableLayoutPanel.Controls.Add(cbMaskRegex, 1, 5);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Font = new System.Drawing.Font("Calibri", 9F);
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 7;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel.Size = new System.Drawing.Size(584, 261);
            tableLayoutPanel.TabIndex = 14;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Controls.Add(btApply);
            flowLayoutPanel.Controls.Add(btCancel);
            flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel.Location = new System.Drawing.Point(65, 224);
            flowLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new System.Drawing.Size(515, 34);
            flowLayoutPanel.TabIndex = 2;
            // 
            // panel_Value
            // 
            panel_Value.Controls.Add(comboBox_Value);
            panel_Value.Controls.Add(textBox_Value);
            panel_Value.Dock = System.Windows.Forms.DockStyle.Fill;
            panel_Value.Location = new System.Drawing.Point(65, 137);
            panel_Value.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel_Value.Name = "panel_Value";
            panel_Value.Size = new System.Drawing.Size(515, 50);
            panel_Value.TabIndex = 0;
            // 
            // comboBox_Value
            // 
            comboBox_Value.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBox_Value.FormattingEnabled = true;
            comboBox_Value.Location = new System.Drawing.Point(0, 0);
            comboBox_Value.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBox_Value.Name = "comboBox_Value";
            comboBox_Value.Size = new System.Drawing.Size(515, 22);
            comboBox_Value.TabIndex = 0;
            // 
            // cbMaskRegex
            // 
            cbMaskRegex.Checked = false;
            cbMaskRegex.Dock = System.Windows.Forms.DockStyle.Fill;
            cbMaskRegex.Location = new System.Drawing.Point(64, 193);
            cbMaskRegex.Name = "cbMaskRegex";
            cbMaskRegex.Size = new System.Drawing.Size(517, 25);
            cbMaskRegex.TabIndex = 14;
            cbMaskRegex.Text = "cbMaskRegex_Text";
            // 
            // EditValueWindow
            // 
            AcceptButton = btApply;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(584, 261);
            Controls.Add(tableLayoutPanel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "EditValueWindow";
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "EditValueWindow_Text";
            ((System.ComponentModel.ISupportInitialize)pictureBox_Logo).EndInit();
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            flowLayoutPanel.ResumeLayout(false);
            panel_Value.ResumeLayout(false);
            panel_Value.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.PictureBox pictureBox_Logo;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.Label lbValue;
        private System.Windows.Forms.TextBox textBox_Value;
        private System.Windows.Forms.TextBox textBox_Parameter;
        private System.Windows.Forms.Label lbParameter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Panel panel_Value;
        private System.Windows.Forms.ComboBox comboBox_Value;
        private FlowBloxCheckBox cbMaskRegex;
        private System.Windows.Forms.ImageList imageList;
    }
}