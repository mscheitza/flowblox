namespace FlowBlox.Views
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            btOk = new System.Windows.Forms.Button();
            labelTitle = new System.Windows.Forms.Label();
            labelVersion = new System.Windows.Forms.Label();
            labelCopyright = new System.Windows.Forms.Label();
            pbLogo = new System.Windows.Forms.PictureBox();
            groupBoxProductInformation = new System.Windows.Forms.GroupBox();
            labelDescription = new System.Windows.Forms.Label();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)pbLogo).BeginInit();
            groupBoxProductInformation.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // btOk
            // 
            btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btOk.Location = new System.Drawing.Point(355, 3);
            btOk.Name = "btOk";
            btOk.Size = new System.Drawing.Size(75, 25);
            btOk.TabIndex = 25;
            btOk.Text = "&OK";
            btOk.Click += btOk_Click;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new System.Drawing.Font("Calibri", 9F);
            labelTitle.Location = new System.Drawing.Point(6, 29);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(88, 14);
            labelTitle.TabIndex = 30;
            labelTitle.Text = "labelTitle_Text";
            // 
            // labelVersion
            // 
            labelVersion.AutoSize = true;
            labelVersion.Font = new System.Drawing.Font("Calibri", 9F);
            labelVersion.Location = new System.Drawing.Point(6, 90);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(104, 14);
            labelVersion.TabIndex = 31;
            labelVersion.Text = "labelVersion_Text";
            // 
            // labelCopyright
            // 
            labelCopyright.AutoSize = true;
            labelCopyright.Font = new System.Drawing.Font("Calibri", 9F);
            labelCopyright.Location = new System.Drawing.Point(6, 118);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new System.Drawing.Size(113, 14);
            labelCopyright.TabIndex = 32;
            labelCopyright.Text = "labelCopyright_Text";
            // 
            // pbLogo
            // 
            pbLogo.BackColor = System.Drawing.Color.FromArgb(53, 53, 53);
            pbLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            pbLogo.Image = (System.Drawing.Image)resources.GetObject("pbLogo.Image");
            pbLogo.Location = new System.Drawing.Point(8, 8);
            pbLogo.Name = "pbLogo";
            pbLogo.Size = new System.Drawing.Size(433, 114);
            pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pbLogo.TabIndex = 33;
            pbLogo.TabStop = false;
            // 
            // groupBoxProductInformation
            // 
            groupBoxProductInformation.Controls.Add(labelDescription);
            groupBoxProductInformation.Controls.Add(labelTitle);
            groupBoxProductInformation.Controls.Add(labelVersion);
            groupBoxProductInformation.Controls.Add(labelCopyright);
            groupBoxProductInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxProductInformation.Font = new System.Drawing.Font("Calibri", 9F);
            groupBoxProductInformation.Location = new System.Drawing.Point(8, 128);
            groupBoxProductInformation.Name = "groupBoxProductInformation";
            groupBoxProductInformation.Size = new System.Drawing.Size(433, 173);
            groupBoxProductInformation.TabIndex = 34;
            groupBoxProductInformation.TabStop = false;
            groupBoxProductInformation.Text = "groupBoxProductInformation_Text";
            // 
            // labelDescription
            // 
            labelDescription.AutoSize = true;
            labelDescription.Font = new System.Drawing.Font("Calibri", 9F);
            labelDescription.Location = new System.Drawing.Point(6, 61);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new System.Drawing.Size(125, 14);
            labelDescription.TabIndex = 33;
            labelDescription.Text = "labelDescription_Text";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pbLogo, 0, 0);
            tableLayoutPanel1.Controls.Add(groupBoxProductInformation, 0, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 2);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            tableLayoutPanel1.Size = new System.Drawing.Size(449, 344);
            tableLayoutPanel1.TabIndex = 35;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btOk);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(8, 307);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(433, 29);
            flowLayoutPanel1.TabIndex = 35;
            // 
            // About
            // 
            AcceptButton = btOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btOk;
            ClientSize = new System.Drawing.Size(449, 344);
            Controls.Add(tableLayoutPanel1);
            Font = new System.Drawing.Font("Calibri", 9F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "About";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "About_Text";
            ((System.ComponentModel.ISupportInitialize)pbLogo).EndInit();
            groupBoxProductInformation.ResumeLayout(false);
            groupBoxProductInformation.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.GroupBox groupBoxProductInformation;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}

