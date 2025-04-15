namespace FlowBlox.Views
{
    partial class OptionWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionWindow));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            btDelete = new System.Windows.Forms.Button();
            imageList = new System.Windows.Forms.ImageList(components);
            btAdd = new System.Windows.Forms.Button();
            btRevert = new System.Windows.Forms.Button();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btCancel = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.Panel();
            optionView1 = new OptionView();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            textBoxSearch = new System.Windows.Forms.TextBox();
            treeViewOptions = new System.Windows.Forms.TreeView();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.68904F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64.31096F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 1);
            tableLayoutPanel1.Controls.Add(panel1, 1, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            tableLayoutPanel1.Size = new System.Drawing.Size(849, 547);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(btDelete);
            flowLayoutPanel2.Controls.Add(btAdd);
            flowLayoutPanel2.Controls.Add(btRevert);
            flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new System.Drawing.Point(3, 475);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(296, 69);
            flowLayoutPanel2.TabIndex = 3;
            // 
            // btDelete
            // 
            btDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btDelete.ImageKey = "delete";
            btDelete.ImageList = imageList;
            btDelete.Location = new System.Drawing.Point(183, 3);
            btDelete.Name = "btDelete";
            btDelete.Size = new System.Drawing.Size(110, 30);
            btDelete.TabIndex = 0;
            btDelete.Text = "btDelete_Text";
            btDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btDelete.UseVisualStyleBackColor = true;
            btDelete.Click += btDeleteOption_Click;
            // 
            // imageList
            // 
            imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = System.Drawing.Color.Transparent;
            imageList.Images.SetKeyName(0, "recover");
            imageList.Images.SetKeyName(1, "delete");
            imageList.Images.SetKeyName(2, "add");
            imageList.Images.SetKeyName(3, "cancel");
            imageList.Images.SetKeyName(4, "accept");
            imageList.Images.SetKeyName(5, "option");
            // 
            // btAdd
            // 
            btAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btAdd.ImageKey = "add";
            btAdd.ImageList = imageList;
            btAdd.Location = new System.Drawing.Point(67, 3);
            btAdd.Name = "btAdd";
            btAdd.Size = new System.Drawing.Size(110, 30);
            btAdd.TabIndex = 1;
            btAdd.Text = "btAdd_Text";
            btAdd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btAdd.UseVisualStyleBackColor = true;
            btAdd.Click += btAddOption_Click;
            // 
            // btRevert
            // 
            btRevert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btRevert.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btRevert.ImageKey = "recover";
            btRevert.ImageList = imageList;
            btRevert.Location = new System.Drawing.Point(67, 39);
            btRevert.Name = "btRevert";
            btRevert.Size = new System.Drawing.Size(226, 30);
            btRevert.TabIndex = 2;
            btRevert.Text = "btRevert_Text";
            btRevert.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btRevert.UseVisualStyleBackColor = true;
            btRevert.Click += btRevertOptions_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btCancel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(305, 475);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(541, 69);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // btCancel
            // 
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btCancel.ImageKey = "cancel";
            btCancel.ImageList = imageList;
            btCancel.Location = new System.Drawing.Point(428, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(110, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "btCancel_Text";
            btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(optionView1);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(305, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(541, 466);
            panel1.TabIndex = 2;
            // 
            // optionView1
            // 
            optionView1.BackColor = System.Drawing.Color.WhiteSmoke;
            optionView1.Dock = System.Windows.Forms.DockStyle.Fill;
            optionView1.Font = new System.Drawing.Font("Segoe UI", 9F);
            optionView1.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
            optionView1.Location = new System.Drawing.Point(0, 0);
            optionView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            optionView1.Name = "optionView1";
            optionView1.Size = new System.Drawing.Size(541, 466);
            optionView1.TabIndex = 0;
            optionView1.Visible = false;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(textBoxSearch, 0, 0);
            tableLayoutPanel2.Controls.Add(treeViewOptions, 0, 1);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new System.Drawing.Size(296, 466);
            tableLayoutPanel2.TabIndex = 4;
            // 
            // textBoxSearch
            // 
            textBoxSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxSearch.Location = new System.Drawing.Point(3, 3);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new System.Drawing.Size(290, 22);
            textBoxSearch.TabIndex = 0;
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            // 
            // treeViewOptions
            // 
            treeViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewOptions.Location = new System.Drawing.Point(3, 33);
            treeViewOptions.Name = "treeViewOptions";
            treeViewOptions.Size = new System.Drawing.Size(290, 430);
            treeViewOptions.TabIndex = 1;
            treeViewOptions.AfterSelect += treeViewOptions_AfterSelect;
            // 
            // OptionWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(849, 547);
            Controls.Add(tableLayoutPanel1);
            Font = new System.Drawing.Font("Calibri", 9F);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "OptionWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "OptionWindow_Text";
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btDelete;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btRevert;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.TreeView treeViewOptions;
        private OptionView optionView1;
    }
}