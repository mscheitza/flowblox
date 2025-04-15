namespace FlowBlox.Grid.Views.Main
{
    partial class FlowBlockMainView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlowBlockMainView));
            btApply = new System.Windows.Forms.Button();
            btCancel = new System.Windows.Forms.Button();
            pictureBoxLogo = new System.Windows.Forms.PictureBox();
            flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            mainPanel = new System.Windows.Forms.Panel();
            labelDescription = new System.Windows.Forms.Label();
            toolStrip = new System.Windows.Forms.ToolStrip();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            flowLayoutPanel.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // btApply
            // 
            btApply.AutoSize = true;
            btApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btApply.Image = (System.Drawing.Image)resources.GetObject("btApply.Image");
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.Location = new System.Drawing.Point(617, 3);
            btApply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btApply.Name = "btApply";
            btApply.Size = new System.Drawing.Size(110, 30);
            btApply.TabIndex = 1;
            btApply.Text = "btApply_Text";
            btApply.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btApply.UseVisualStyleBackColor = true;
            btApply.Click += btApply_Click;
            // 
            // btCancel
            // 
            btCancel.AutoSize = true;
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            btCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btCancel.Image = (System.Drawing.Image)resources.GetObject("btCancel.Image");
            btCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btCancel.Location = new System.Drawing.Point(499, 3);
            btCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 0;
            btCancel.Text = "&Zurück";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxLogo.Image = (System.Drawing.Image)resources.GetObject("pictureBoxLogo.Image");
            pictureBoxLogo.Location = new System.Drawing.Point(4, 3);
            pictureBoxLogo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new System.Drawing.Size(53, 54);
            pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Controls.Add(btApply);
            flowLayoutPanel.Controls.Add(btCancel);
            flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel.Location = new System.Drawing.Point(65, 413);
            flowLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new System.Drawing.Size(731, 34);
            flowLayoutPanel.TabIndex = 5;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(pictureBoxLogo, 0, 0);
            tableLayoutPanel.Controls.Add(flowLayoutPanel, 1, 3);
            tableLayoutPanel.Controls.Add(mainPanel, 1, 2);
            tableLayoutPanel.Controls.Add(labelDescription, 1, 0);
            tableLayoutPanel.Controls.Add(toolStrip, 1, 1);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 4;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel.Size = new System.Drawing.Size(800, 450);
            tableLayoutPanel.TabIndex = 1;
            // 
            // mainPanel
            // 
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(64, 89);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new System.Drawing.Size(733, 318);
            mainPanel.TabIndex = 8;
            // 
            // labelDescription
            // 
            labelDescription.AutoSize = true;
            labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            labelDescription.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            labelDescription.Location = new System.Drawing.Point(64, 0);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new System.Drawing.Size(733, 60);
            labelDescription.TabIndex = 9;
            labelDescription.Text = "label1";
            labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip
            // 
            toolStrip.Location = new System.Drawing.Point(61, 60);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new System.Drawing.Size(739, 25);
            toolStrip.TabIndex = 10;
            toolStrip.Text = "toolStrip2";
            // 
            // FlowBlockMainView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(tableLayoutPanel);
            Name = "FlowBlockMainView";
            Text = "FlowBlockMainView";
            FormClosed += FlowBlockMainView_FormClosed;
            Shown += FlowBlockMainView_Shown;
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            flowLayoutPanel.ResumeLayout(false);
            flowLayoutPanel.PerformLayout();
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TextBox tbElementName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btManualExecution;
        private System.Windows.Forms.ToolStrip toolStrip;
    }
}