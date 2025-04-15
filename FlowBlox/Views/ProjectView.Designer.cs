namespace FlowBlox.Views
{
    partial class ProjectView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectView));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pictureBox = new System.Windows.Forms.PictureBox();
            tbProjectName = new System.Windows.Forms.TextBox();
            tbAuthor = new System.Windows.Forms.TextBox();
            lbDescription = new System.Windows.Forms.Label();
            lbCreator = new System.Windows.Forms.Label();
            tbProjectDescription = new System.Windows.Forms.TextBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btApply = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            btCancel = new System.Windows.Forms.Button();
            tbNoteOnOpening = new System.Windows.Forms.TextBox();
            lbProjectDescription = new System.Windows.Forms.Label();
            lbNoteOnOpening = new System.Windows.Forms.Label();
            lbProjectName = new System.Windows.Forms.Label();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pictureBox, 0, 0);
            tableLayoutPanel1.Controls.Add(tbProjectName, 1, 2);
            tableLayoutPanel1.Controls.Add(tbAuthor, 1, 4);
            tableLayoutPanel1.Controls.Add(lbDescription, 1, 0);
            tableLayoutPanel1.Controls.Add(lbCreator, 1, 3);
            tableLayoutPanel1.Controls.Add(tbProjectDescription, 1, 6);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 9);
            tableLayoutPanel1.Controls.Add(tbNoteOnOpening, 1, 8);
            tableLayoutPanel1.Controls.Add(lbProjectDescription, 1, 5);
            tableLayoutPanel1.Controls.Add(lbNoteOnOpening, 1, 7);
            tableLayoutPanel1.Controls.Add(lbProjectName, 1, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 10;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new System.Drawing.Size(625, 408);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // pictureBox
            // 
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Location = new System.Drawing.Point(3, 3);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(48, 52);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // tbProjectName
            // 
            tbProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            tbProjectName.Location = new System.Drawing.Point(57, 91);
            tbProjectName.Name = "tbProjectName";
            tbProjectName.Size = new System.Drawing.Size(565, 22);
            tbProjectName.TabIndex = 0;
            // 
            // tbAuthor
            // 
            tbAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            tbAuthor.Location = new System.Drawing.Point(57, 146);
            tbAuthor.Name = "tbAuthor";
            tbAuthor.Size = new System.Drawing.Size(565, 22);
            tbAuthor.TabIndex = 1;
            // 
            // lbDescription
            // 
            lbDescription.AutoSize = true;
            lbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            lbDescription.Font = new System.Drawing.Font("Calibri", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lbDescription.Location = new System.Drawing.Point(57, 0);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new System.Drawing.Size(565, 58);
            lbDescription.TabIndex = 0;
            lbDescription.Text = "lbDescription_Text";
            lbDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbCreator
            // 
            lbCreator.AutoSize = true;
            lbCreator.BackColor = System.Drawing.SystemColors.Control;
            lbCreator.Dock = System.Windows.Forms.DockStyle.Fill;
            lbCreator.ForeColor = System.Drawing.SystemColors.ControlText;
            lbCreator.Location = new System.Drawing.Point(57, 113);
            lbCreator.Name = "lbCreator";
            lbCreator.Size = new System.Drawing.Size(565, 30);
            lbCreator.TabIndex = 34;
            lbCreator.Tag = "style_header";
            lbCreator.Text = "lbCreator_Text";
            lbCreator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbProjectDescription
            // 
            tbProjectDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            tbProjectDescription.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tbProjectDescription.Location = new System.Drawing.Point(57, 201);
            tbProjectDescription.Multiline = true;
            tbProjectDescription.Name = "tbProjectDescription";
            tbProjectDescription.Size = new System.Drawing.Size(565, 64);
            tbProjectDescription.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btApply);
            flowLayoutPanel1.Controls.Add(btCancel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(57, 371);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(565, 34);
            flowLayoutPanel1.TabIndex = 5;
            // 
            // btApply
            // 
            btApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btApply.ImageKey = "accept";
            btApply.ImageList = imageList;
            btApply.Location = new System.Drawing.Point(452, 3);
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
            btCancel.Location = new System.Drawing.Point(336, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // tbNoteOnOpening
            // 
            tbNoteOnOpening.Dock = System.Windows.Forms.DockStyle.Fill;
            tbNoteOnOpening.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tbNoteOnOpening.Location = new System.Drawing.Point(57, 301);
            tbNoteOnOpening.Multiline = true;
            tbNoteOnOpening.Name = "tbNoteOnOpening";
            tbNoteOnOpening.Size = new System.Drawing.Size(565, 64);
            tbNoteOnOpening.TabIndex = 4;
            // 
            // lbProjectDescription
            // 
            lbProjectDescription.AutoSize = true;
            lbProjectDescription.BackColor = System.Drawing.SystemColors.Control;
            lbProjectDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            lbProjectDescription.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lbProjectDescription.ForeColor = System.Drawing.SystemColors.ControlText;
            lbProjectDescription.Location = new System.Drawing.Point(57, 170);
            lbProjectDescription.Margin = new System.Windows.Forms.Padding(3, 2, 3, 1);
            lbProjectDescription.Name = "lbProjectDescription";
            lbProjectDescription.Size = new System.Drawing.Size(565, 27);
            lbProjectDescription.TabIndex = 1;
            lbProjectDescription.Tag = "style_header";
            lbProjectDescription.Text = "lbProjectDescription_Text";
            lbProjectDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbNoteOnOpening
            // 
            lbNoteOnOpening.AutoSize = true;
            lbNoteOnOpening.BackColor = System.Drawing.SystemColors.Control;
            lbNoteOnOpening.Dock = System.Windows.Forms.DockStyle.Fill;
            lbNoteOnOpening.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lbNoteOnOpening.ForeColor = System.Drawing.SystemColors.ControlText;
            lbNoteOnOpening.Location = new System.Drawing.Point(57, 270);
            lbNoteOnOpening.Margin = new System.Windows.Forms.Padding(3, 2, 3, 1);
            lbNoteOnOpening.Name = "lbNoteOnOpening";
            lbNoteOnOpening.Size = new System.Drawing.Size(565, 27);
            lbNoteOnOpening.TabIndex = 3;
            lbNoteOnOpening.Tag = "style_header";
            lbNoteOnOpening.Text = "lbNoteOnOpening_Text";
            lbNoteOnOpening.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbProjectName
            // 
            lbProjectName.AutoSize = true;
            lbProjectName.BackColor = System.Drawing.SystemColors.Control;
            lbProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            lbProjectName.ForeColor = System.Drawing.SystemColors.ControlText;
            lbProjectName.Location = new System.Drawing.Point(57, 58);
            lbProjectName.Name = "lbProjectName";
            lbProjectName.Size = new System.Drawing.Size(565, 30);
            lbProjectName.TabIndex = 35;
            lbProjectName.Tag = "style_header";
            lbProjectName.Text = "lbProjectName_Text";
            lbProjectName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProjectView
            // 
            AcceptButton = btApply;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(625, 408);
            Controls.Add(tableLayoutPanel1);
            Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ProjectView";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "ProjectView_Text";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btApply;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox tbProjectDescription;
        private System.Windows.Forms.TextBox tbProjectName;
        private System.Windows.Forms.TextBox tbAuthor;
        private System.Windows.Forms.Label lbCreator;
        private System.Windows.Forms.TextBox tbNoteOnOpening;
        private System.Windows.Forms.Label lbProjectDescription;
        private System.Windows.Forms.Label lbNoteOnOpening;
        private System.Windows.Forms.Label lbProjectName;
        private System.Windows.Forms.ImageList imageList;
    }
}