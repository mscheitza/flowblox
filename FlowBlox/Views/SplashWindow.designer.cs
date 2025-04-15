namespace FlowBlox
{
    partial class SplashWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashWindow));
            pbLoading = new System.Windows.Forms.ProgressBar();
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            labelMessage = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pbLoading
            // 
            pbLoading.BackColor = System.Drawing.SystemColors.Control;
            pbLoading.Location = new System.Drawing.Point(90, 166);
            pbLoading.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbLoading.MarqueeAnimationSpeed = 10;
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new System.Drawing.Size(420, 23);
            pbLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            pbLoading.TabIndex = 6;
            // 
            // backgroundWorker
            // 
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            // 
            // labelMessage
            // 
            labelMessage.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelMessage.BackColor = System.Drawing.Color.Transparent;
            labelMessage.Font = new System.Drawing.Font("Calibri", 8.5F);
            labelMessage.ForeColor = System.Drawing.Color.White;
            labelMessage.Location = new System.Drawing.Point(90, 126);
            labelMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new System.Drawing.Size(420, 34);
            labelMessage.TabIndex = 8;
            labelMessage.Tag = "";
            labelMessage.Text = "loading...";
            labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = (System.Drawing.Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            pictureBox1.Location = new System.Drawing.Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(567, 148);
            pictureBox1.TabIndex = 9;
            pictureBox1.TabStop = false;
            // 
            // SplashWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(53, 53, 53);
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ClientSize = new System.Drawing.Size(591, 203);
            Controls.Add(labelMessage);
            Controls.Add(pbLoading);
            Controls.Add(pictureBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SplashWindow";
            Opacity = 0.92D;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Activated += SplashWindow_Activated;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ProgressBar pbLoading;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}