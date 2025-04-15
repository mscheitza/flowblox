namespace FlowBlox.Views
{
    partial class ProblemsView
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
            listView = new System.Windows.Forms.ListView();
            chTimestamp = new System.Windows.Forms.ColumnHeader();
            chCriticality = new System.Windows.Forms.ColumnHeader();
            chMessage = new System.Windows.Forms.ColumnHeader();
            chException = new System.Windows.Forms.ColumnHeader();
            chName = new System.Windows.Forms.ColumnHeader();
            SuspendLayout();
            // 
            // listView
            // 
            listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { chTimestamp, chName, chCriticality, chMessage, chException });
            listView.Dock = System.Windows.Forms.DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.HoverSelection = true;
            listView.Location = new System.Drawing.Point(0, 0);
            listView.Name = "listView";
            listView.Size = new System.Drawing.Size(1057, 303);
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = System.Windows.Forms.View.Details;
            listView.DoubleClick += listView_DoubleClick;
            // 
            // chTimestamp
            // 
            chTimestamp.Text = "Timestamp";
            chTimestamp.Width = 150;
            // 
            // chCriticality
            // 
            chCriticality.Text = "Criticality";
            chCriticality.Width = 150;
            // 
            // chMessage
            // 
            chMessage.Text = "Message";
            chMessage.Width = 300;
            // 
            // chException
            // 
            chException.Text = "Exception";
            chException.Width = 300;
            // 
            // chName
            // 
            chName.Text = "Name";
            chName.Width = 200;
            // 
            // ProblemsView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(listView);
            Name = "ProblemsView";
            Size = new System.Drawing.Size(1057, 303);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader chTimestamp;
        private System.Windows.Forms.ColumnHeader chCriticality;
        private System.Windows.Forms.ColumnHeader chMessage;
        private System.Windows.Forms.ColumnHeader chException;
        private System.Windows.Forms.ColumnHeader chName;
    }
}
