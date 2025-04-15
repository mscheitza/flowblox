namespace FlowBlox.Views
{
    partial class PropertyWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyWindow));
            flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            btApply = new System.Windows.Forms.Button();
            btCancel = new System.Windows.Forms.Button();
            flowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Controls.Add(btApply);
            flowLayoutPanel.Controls.Add(btCancel);
            flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowLayoutPanel.Location = new System.Drawing.Point(0, 417);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            flowLayoutPanel.Size = new System.Drawing.Size(800, 33);
            flowLayoutPanel.TabIndex = 0;
            // 
            // btApply
            // 
            btApply.AutoSize = true;
            btApply.Image = (System.Drawing.Image)resources.GetObject("btApply.Image");
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.Location = new System.Drawing.Point(687, 3);
            btApply.Name = "btApply";
            btApply.RightToLeft = System.Windows.Forms.RightToLeft.No;
            btApply.Size = new System.Drawing.Size(110, 30);
            btApply.TabIndex = 0;
            btApply.Text = "btApply_Text";
            btApply.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btApply.UseVisualStyleBackColor = true;
            btApply.Click += btApply_Click;
            // 
            // btCancel
            // 
            btCancel.AutoSize = true;
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.Image = (System.Drawing.Image)resources.GetObject("btCancel.Image");
            btCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btCancel.Location = new System.Drawing.Point(571, 3);
            btCancel.Name = "btCancel";
            btCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            // 
            // PropertyWindow
            // 
            AcceptButton = btApply;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(flowLayoutPanel);
            Name = "PropertyWindow";
            Text = "PropertyView";
            FormClosing += PropertyWindow_FormClosing;
            flowLayoutPanel.ResumeLayout(false);
            flowLayoutPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.Button btCancel;
    }
}