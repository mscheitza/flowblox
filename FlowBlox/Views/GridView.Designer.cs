namespace FlowBlox.Views
{
    partial class GridView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridView));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            panel1 = new System.Windows.Forms.Panel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            lbWidth = new System.Windows.Forms.Label();
            tbHeight = new System.Windows.Forms.TextBox();
            tbWidth = new System.Windows.Forms.TextBox();
            lbHeight = new System.Windows.Forms.Label();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btApply = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            btCancel = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            label1 = new System.Windows.Forms.Label();
            lbDescription = new System.Windows.Forms.Label();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel1, 1, 2);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 3);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(label1, 1, 0);
            tableLayoutPanel1.Controls.Add(lbDescription, 1, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new System.Drawing.Size(545, 280);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel2);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(55, 107);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(487, 130);
            panel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(lbWidth, 0, 0);
            tableLayoutPanel2.Controls.Add(tbHeight, 0, 3);
            tableLayoutPanel2.Controls.Add(tbWidth, 0, 1);
            tableLayoutPanel2.Controls.Add(lbHeight, 0, 2);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new System.Drawing.Size(487, 130);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // lbWidth
            // 
            lbWidth.AutoSize = true;
            lbWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            lbWidth.Location = new System.Drawing.Point(3, 0);
            lbWidth.Name = "lbWidth";
            lbWidth.Size = new System.Drawing.Size(481, 25);
            lbWidth.TabIndex = 1;
            lbWidth.Tag = "style_header";
            lbWidth.Text = "lbWidth_Text";
            lbWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbHeight
            // 
            tbHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            tbHeight.Location = new System.Drawing.Point(3, 93);
            tbHeight.Name = "tbHeight";
            tbHeight.Size = new System.Drawing.Size(481, 22);
            tbHeight.TabIndex = 1;
            // 
            // tbWidth
            // 
            tbWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            tbWidth.Location = new System.Drawing.Point(3, 28);
            tbWidth.Name = "tbWidth";
            tbWidth.Size = new System.Drawing.Size(481, 22);
            tbWidth.TabIndex = 0;
            // 
            // lbHeight
            // 
            lbHeight.AutoSize = true;
            lbHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            lbHeight.Location = new System.Drawing.Point(3, 65);
            lbHeight.Name = "lbHeight";
            lbHeight.Size = new System.Drawing.Size(481, 25);
            lbHeight.TabIndex = 2;
            lbHeight.Tag = "style_header";
            lbHeight.Text = "lbHeight_Text";
            lbHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btApply);
            flowLayoutPanel1.Controls.Add(btCancel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(55, 243);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(487, 34);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // btApply
            // 
            btApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.ImageKey = "accept";
            btApply.ImageList = imageList;
            btApply.Location = new System.Drawing.Point(374, 3);
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
            btCancel.Location = new System.Drawing.Point(258, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 2;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new System.Drawing.Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(46, 50);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            label1.Location = new System.Drawing.Point(55, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(487, 56);
            label1.TabIndex = 2;
            label1.Text = "FlowBlox.Grid";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbDescription
            // 
            lbDescription.AutoSize = true;
            lbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            lbDescription.Font = new System.Drawing.Font("Calibri", 9F);
            lbDescription.Location = new System.Drawing.Point(55, 56);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new System.Drawing.Size(487, 48);
            lbDescription.TabIndex = 4;
            lbDescription.Text = "lbDescription_Text";
            lbDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GridView
            // 
            AcceptButton = btApply;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(545, 280);
            Controls.Add(tableLayoutPanel1);
            Font = new System.Drawing.Font("Calibri", 9F);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "GridView";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "GridView_Text";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.TextBox tbHeight;
        private System.Windows.Forms.Label lbHeight;
        private System.Windows.Forms.Label lbWidth;
        private System.Windows.Forms.TextBox tbWidth;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ImageList imageList;
    }
}