namespace FlowBlox.AppWindow.Contents
{
    partial class ProjectPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectPanel));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            mainPanel = new Core.DoubleBufferedPanel();
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            itmEditElement = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            itmInsightInput = new System.Windows.Forms.ToolStripMenuItem();
            itmInsightOutput = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            itmManageNotifications = new System.Windows.Forms.ToolStripMenuItem();
            itmBreakPoint = new System.Windows.Forms.ToolStripMenuItem();
            itmIndex = new System.Windows.Forms.ToolStripMenuItem();
            itmDefineIndex = new System.Windows.Forms.ToolStripMenuItem();
            itmRemoveIndex = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            itmRefresh = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            itmDeleteElement = new System.Windows.Forms.ToolStripMenuItem();
            itmDeleteConnection = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip_Mode = new System.Windows.Forms.ToolStrip();
            btSelectionMode = new System.Windows.Forms.ToolStripButton();
            btConnectionMode = new System.Windows.Forms.ToolStripButton();
            toolStrip_Runtime = new System.Windows.Forms.ToolStrip();
            itmSelection = new System.Windows.Forms.ToolStripDropDownButton();
            itmSelection_Left = new System.Windows.Forms.ToolStripMenuItem();
            itmSelection_Right = new System.Windows.Forms.ToolStripMenuItem();
            itmSelection_Up = new System.Windows.Forms.ToolStripMenuItem();
            itmSelection_Down = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            itmSelection_All = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip_Label1 = new System.Windows.Forms.ToolStripLabel();
            btStopExecution = new System.Windows.Forms.ToolStripButton();
            btPause = new System.Windows.Forms.ToolStripButton();
            btExecute = new System.Windows.Forms.ToolStripButton();
            toolStrip_Label0 = new System.Windows.Forms.ToolStripLabel();
            btGridSettings = new System.Windows.Forms.ToolStripButton();
            legendPanel = new System.Windows.Forms.FlowLayoutPanel();
            labelRecursiveCall = new System.Windows.Forms.Label();
            pictureBoxRecursiveCall = new System.Windows.Forms.PictureBox();
            labelIterationContext = new System.Windows.Forms.Label();
            pictureBoxIterationContext = new System.Windows.Forms.PictureBox();
            labelInvoke = new System.Windows.Forms.Label();
            pictureBoxInvoke = new System.Windows.Forms.PictureBox();
            background_Align = new System.ComponentModel.BackgroundWorker();
            imageList_Grid = new System.Windows.Forms.ImageList(components);
            tableLayoutPanel1.SuspendLayout();
            contextMenuStrip.SuspendLayout();
            toolStrip_Mode.SuspendLayout();
            toolStrip_Runtime.SuspendLayout();
            legendPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRecursiveCall).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIterationContext).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxInvoke).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(mainPanel, 0, 2);
            tableLayoutPanel1.Controls.Add(toolStrip_Mode, 0, 1);
            tableLayoutPanel1.Controls.Add(toolStrip_Runtime, 0, 0);
            tableLayoutPanel1.Controls.Add(legendPanel, 0, 3);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new System.Drawing.Size(800, 453);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // mainPanel
            // 
            mainPanel.AllowDrop = true;
            mainPanel.BackgroundImage = (System.Drawing.Image)resources.GetObject("mainPanel.BackgroundImage");
            mainPanel.ContextMenuStrip = contextMenuStrip;
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(3, 63);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new System.Drawing.Size(794, 357);
            mainPanel.TabIndex = 11;
            mainPanel.Scroll += mainPanel_Scroll;
            mainPanel.Click += mainPanel_Click;
            mainPanel.DragDrop += mainPanel_DragDrop;
            mainPanel.DragEnter += mainPanel_DragEnter;
            mainPanel.Paint += mainPanel_Paint;
            mainPanel.MouseDown += mainPanel_MouseDown;
            mainPanel.MouseMove += mainPanel_MouseMove;
            mainPanel.MouseUp += mainPanel_MouseUp;
            mainPanel.Resize += mainPanel_Resize;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { itmEditElement, toolStripSeparator2, itmInsightInput, itmInsightOutput, toolStripSeparator14, itmManageNotifications, itmBreakPoint, itmIndex, toolStripSeparator7, itmRefresh, toolStripSeparator9, itmDeleteElement, itmDeleteConnection });
            contextMenuStrip.Name = "contextGridElement";
            contextMenuStrip.Size = new System.Drawing.Size(242, 226);
            // 
            // itmEditElement
            // 
            itmEditElement.Image = (System.Drawing.Image)resources.GetObject("itmEditElement.Image");
            itmEditElement.Name = "itmEditElement";
            itmEditElement.Size = new System.Drawing.Size(241, 22);
            itmEditElement.Text = "itmEditElement_Text";
            itmEditElement.Click += itmEditElement_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(238, 6);
            // 
            // itmInsightInput
            // 
            itmInsightInput.Image = (System.Drawing.Image)resources.GetObject("itmInsightInput.Image");
            itmInsightInput.Name = "itmInsightInput";
            itmInsightInput.Size = new System.Drawing.Size(241, 22);
            itmInsightInput.Text = "itmInsightInput_Text";
            itmInsightInput.Click += itmInsightInput_Click;
            // 
            // itmInsightOutput
            // 
            itmInsightOutput.Image = (System.Drawing.Image)resources.GetObject("itmInsightOutput.Image");
            itmInsightOutput.Name = "itmInsightOutput";
            itmInsightOutput.Size = new System.Drawing.Size(241, 22);
            itmInsightOutput.Text = "itmInsightOutput_Text";
            itmInsightOutput.Click += itmInsightOutput_Click;
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(238, 6);
            // 
            // itmManageNotifications
            // 
            itmManageNotifications.Image = (System.Drawing.Image)resources.GetObject("itmManageNotifications.Image");
            itmManageNotifications.Name = "itmManageNotifications";
            itmManageNotifications.Size = new System.Drawing.Size(241, 22);
            itmManageNotifications.Text = "itmManageNotifications_Text";
            itmManageNotifications.Click += itmManageNotifications_Click;
            // 
            // itmBreakPoint
            // 
            itmBreakPoint.Image = (System.Drawing.Image)resources.GetObject("itmBreakPoint.Image");
            itmBreakPoint.Name = "itmBreakPoint";
            itmBreakPoint.Size = new System.Drawing.Size(241, 22);
            itmBreakPoint.Text = "itmBreakPoint_Text";
            itmBreakPoint.Click += itmBreakPoint_Click;
            // 
            // itmIndex
            // 
            itmIndex.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmDefineIndex, itmRemoveIndex });
            itmIndex.Image = (System.Drawing.Image)resources.GetObject("itmIndex.Image");
            itmIndex.Name = "itmIndex";
            itmIndex.Size = new System.Drawing.Size(241, 22);
            itmIndex.Text = "itmIndex_Text";
            // 
            // itmDefineIndex
            // 
            itmDefineIndex.Image = (System.Drawing.Image)resources.GetObject("itmDefineIndex.Image");
            itmDefineIndex.Name = "itmDefineIndex";
            itmDefineIndex.Size = new System.Drawing.Size(190, 22);
            itmDefineIndex.Text = "itmDefineIndex_Text";
            itmDefineIndex.Click += itmDefineIndex_Click;
            // 
            // itmRemoveIndex
            // 
            itmRemoveIndex.Image = (System.Drawing.Image)resources.GetObject("itmRemoveIndex.Image");
            itmRemoveIndex.Name = "itmRemoveIndex";
            itmRemoveIndex.Size = new System.Drawing.Size(190, 22);
            itmRemoveIndex.Text = "itmRemoveIndex_Text";
            itmRemoveIndex.Click += itmRemoveIndex_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(238, 6);
            toolStripSeparator7.Visible = false;
            // 
            // itmRefresh
            // 
            itmRefresh.Image = (System.Drawing.Image)resources.GetObject("itmRefresh.Image");
            itmRefresh.Name = "itmRefresh";
            itmRefresh.ShortcutKeys = System.Windows.Forms.Keys.F5;
            itmRefresh.Size = new System.Drawing.Size(241, 22);
            itmRefresh.Text = "itmRefresh_Text";
            itmRefresh.Click += itmRefresh_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(238, 6);
            // 
            // itmDeleteElement
            // 
            itmDeleteElement.Image = (System.Drawing.Image)resources.GetObject("itmDeleteElement.Image");
            itmDeleteElement.Name = "itmDeleteElement";
            itmDeleteElement.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            itmDeleteElement.Size = new System.Drawing.Size(241, 22);
            itmDeleteElement.Text = "itmDeleteElement_Text";
            itmDeleteElement.Click += itmDeleteElement_Click;
            // 
            // itmDeleteConnection
            // 
            itmDeleteConnection.Image = (System.Drawing.Image)resources.GetObject("itmDeleteConnection.Image");
            itmDeleteConnection.Name = "itmDeleteConnection";
            itmDeleteConnection.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            itmDeleteConnection.Size = new System.Drawing.Size(241, 22);
            itmDeleteConnection.Text = "itmDeleteConnection_Text";
            itmDeleteConnection.Click += itmDeleteConnection_Click;
            // 
            // toolStrip_Mode
            // 
            toolStrip_Mode.BackColor = System.Drawing.SystemColors.Control;
            toolStrip_Mode.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStrip_Mode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btSelectionMode, btConnectionMode });
            toolStrip_Mode.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            toolStrip_Mode.Location = new System.Drawing.Point(0, 30);
            toolStrip_Mode.Name = "toolStrip_Mode";
            toolStrip_Mode.Size = new System.Drawing.Size(800, 30);
            toolStrip_Mode.TabIndex = 9;
            toolStrip_Mode.Text = "toolStrip1";
            // 
            // btSelectionMode
            // 
            btSelectionMode.Image = (System.Drawing.Image)resources.GetObject("btSelectionMode.Image");
            btSelectionMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            btSelectionMode.Name = "btSelectionMode";
            btSelectionMode.Size = new System.Drawing.Size(143, 20);
            btSelectionMode.Text = "btSelectionMode_Text";
            btSelectionMode.CheckedChanged += buttonSelectionMode_CheckedChanged;
            btSelectionMode.Click += buttonSelectionMode_Click;
            // 
            // btConnectionMode
            // 
            btConnectionMode.Image = (System.Drawing.Image)resources.GetObject("btConnectionMode.Image");
            btConnectionMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            btConnectionMode.Name = "btConnectionMode";
            btConnectionMode.Size = new System.Drawing.Size(157, 20);
            btConnectionMode.Text = "btConnectionMode_Text";
            btConnectionMode.CheckedChanged += buttonConnectionMode_CheckedChanged;
            btConnectionMode.Click += buttonConnectionMode_Click;
            // 
            // toolStrip_Runtime
            // 
            toolStrip_Runtime.BackColor = System.Drawing.Color.FromArgb(70, 70, 70);
            toolStrip_Runtime.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStrip_Runtime.GripMargin = new System.Windows.Forms.Padding(0);
            toolStrip_Runtime.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip_Runtime.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { itmSelection, toolStrip_Label1, btStopExecution, btPause, btExecute, toolStrip_Label0, btGridSettings });
            toolStrip_Runtime.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            toolStrip_Runtime.Location = new System.Drawing.Point(0, 0);
            toolStrip_Runtime.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStrip_Runtime.Name = "toolStrip_Runtime";
            toolStrip_Runtime.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip_Runtime.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            toolStrip_Runtime.Size = new System.Drawing.Size(798, 30);
            toolStrip_Runtime.TabIndex = 10;
            toolStrip_Runtime.Text = "toolStrip1";
            // 
            // itmSelection
            // 
            itmSelection.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmSelection_Left, itmSelection_Right, itmSelection_Up, itmSelection_Down, toolStripSeparator13, itmSelection_All });
            itmSelection.ForeColor = System.Drawing.SystemColors.ControlLight;
            itmSelection.Image = (System.Drawing.Image)resources.GetObject("itmSelection.Image");
            itmSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
            itmSelection.Name = "itmSelection";
            itmSelection.ShowDropDownArrow = false;
            itmSelection.Size = new System.Drawing.Size(72, 20);
            itmSelection.Text = "Auswahl";
            itmSelection.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // itmSelection_Left
            // 
            itmSelection_Left.Image = (System.Drawing.Image)resources.GetObject("itmSelection_Left.Image");
            itmSelection_Left.Name = "itmSelection_Left";
            itmSelection_Left.ShortcutKeyDisplayString = "";
            itmSelection_Left.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left;
            itmSelection_Left.Size = new System.Drawing.Size(299, 22);
            itmSelection_Left.Text = "Auswahl: Alle Elemente Links";
            itmSelection_Left.Click += itmSelection_Left_Click;
            // 
            // itmSelection_Right
            // 
            itmSelection_Right.Image = (System.Drawing.Image)resources.GetObject("itmSelection_Right.Image");
            itmSelection_Right.Name = "itmSelection_Right";
            itmSelection_Right.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right;
            itmSelection_Right.Size = new System.Drawing.Size(299, 22);
            itmSelection_Right.Text = "Auswahl: Alle Elemente Rechts";
            itmSelection_Right.Click += itmSelection_Right_Click;
            // 
            // itmSelection_Up
            // 
            itmSelection_Up.Image = (System.Drawing.Image)resources.GetObject("itmSelection_Up.Image");
            itmSelection_Up.Name = "itmSelection_Up";
            itmSelection_Up.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up;
            itmSelection_Up.Size = new System.Drawing.Size(299, 22);
            itmSelection_Up.Text = "Auswahl: Alle Elemente Oben";
            itmSelection_Up.Click += itmSelection_Up_Click;
            // 
            // itmSelection_Down
            // 
            itmSelection_Down.Image = (System.Drawing.Image)resources.GetObject("itmSelection_Down.Image");
            itmSelection_Down.Name = "itmSelection_Down";
            itmSelection_Down.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down;
            itmSelection_Down.Size = new System.Drawing.Size(299, 22);
            itmSelection_Down.Text = "Auswahl: Alle Elemente Unten";
            itmSelection_Down.Click += itmSelection_Down_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new System.Drawing.Size(296, 6);
            // 
            // itmSelection_All
            // 
            itmSelection_All.Image = (System.Drawing.Image)resources.GetObject("itmSelection_All.Image");
            itmSelection_All.Name = "itmSelection_All";
            itmSelection_All.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            itmSelection_All.Size = new System.Drawing.Size(299, 22);
            itmSelection_All.Text = "Alle Elemente";
            itmSelection_All.Click += itmSelection_All_Click;
            // 
            // toolStrip_Label1
            // 
            toolStrip_Label1.Font = new System.Drawing.Font("Calibri", 11.25F);
            toolStrip_Label1.ForeColor = System.Drawing.SystemColors.ControlLight;
            toolStrip_Label1.Name = "toolStrip_Label1";
            toolStrip_Label1.Size = new System.Drawing.Size(15, 18);
            toolStrip_Label1.Text = "|";
            // 
            // btStopExecution
            // 
            btStopExecution.ForeColor = System.Drawing.SystemColors.ControlLight;
            btStopExecution.Image = (System.Drawing.Image)resources.GetObject("btStopExecution.Image");
            btStopExecution.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btStopExecution.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            btStopExecution.ImageTransparentColor = System.Drawing.Color.Magenta;
            btStopExecution.Name = "btStopExecution";
            btStopExecution.Size = new System.Drawing.Size(140, 20);
            btStopExecution.Text = "btStopExecution_Text";
            btStopExecution.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            btStopExecution.Click += itmStopExecution_Click;
            // 
            // btPause
            // 
            btPause.ForeColor = System.Drawing.SystemColors.ControlLight;
            btPause.Image = (System.Drawing.Image)resources.GetObject("btPause.Image");
            btPause.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btPause.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            btPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            btPause.Name = "btPause";
            btPause.Size = new System.Drawing.Size(95, 20);
            btPause.Text = "btPause_Text";
            btPause.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            btPause.Click += btPause_Click;
            // 
            // btExecute
            // 
            btExecute.ForeColor = System.Drawing.SystemColors.ControlLight;
            btExecute.Image = (System.Drawing.Image)resources.GetObject("btExecute.Image");
            btExecute.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btExecute.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            btExecute.ImageTransparentColor = System.Drawing.Color.Magenta;
            btExecute.Name = "btExecute";
            btExecute.Size = new System.Drawing.Size(105, 20);
            btExecute.Text = "btExecute_Text";
            btExecute.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            btExecute.Click += itmExecute_Click;
            // 
            // toolStrip_Label0
            // 
            toolStrip_Label0.Font = new System.Drawing.Font("Calibri", 11.25F);
            toolStrip_Label0.ForeColor = System.Drawing.SystemColors.ControlLight;
            toolStrip_Label0.Name = "toolStrip_Label0";
            toolStrip_Label0.Size = new System.Drawing.Size(135, 18);
            toolStrip_Label0.Text = "|FlowBlox.Runtime|";
            // 
            // btGridSettings
            // 
            btGridSettings.ForeColor = System.Drawing.SystemColors.ControlLight;
            btGridSettings.Image = (System.Drawing.Image)resources.GetObject("btGridSettings.Image");
            btGridSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btGridSettings.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            btGridSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            btGridSettings.Name = "btGridSettings";
            btGridSettings.Size = new System.Drawing.Size(128, 20);
            btGridSettings.Text = "btGridSettings_Text";
            btGridSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            btGridSettings.Click += btGridSettings_Click;
            // 
            // legendPanel
            // 
            legendPanel.BackColor = System.Drawing.Color.FromArgb(70, 70, 70);
            legendPanel.Controls.Add(labelRecursiveCall);
            legendPanel.Controls.Add(pictureBoxRecursiveCall);
            legendPanel.Controls.Add(labelIterationContext);
            legendPanel.Controls.Add(pictureBoxIterationContext);
            legendPanel.Controls.Add(labelInvoke);
            legendPanel.Controls.Add(pictureBoxInvoke);
            legendPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            legendPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            legendPanel.Location = new System.Drawing.Point(3, 426);
            legendPanel.Name = "legendPanel";
            legendPanel.Size = new System.Drawing.Size(794, 24);
            legendPanel.TabIndex = 12;
            legendPanel.Tag = "style_ignore";
            // 
            // labelRecursiveCall
            // 
            labelRecursiveCall.AutoSize = true;
            labelRecursiveCall.Dock = System.Windows.Forms.DockStyle.Fill;
            labelRecursiveCall.ForeColor = System.Drawing.SystemColors.ControlLight;
            labelRecursiveCall.Location = new System.Drawing.Point(663, 0);
            labelRecursiveCall.Name = "labelRecursiveCall";
            labelRecursiveCall.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            labelRecursiveCall.Size = new System.Drawing.Size(128, 26);
            labelRecursiveCall.TabIndex = 5;
            labelRecursiveCall.Text = "labelRecursiveCall_Text";
            labelRecursiveCall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxRecursiveCall
            // 
            pictureBoxRecursiveCall.Location = new System.Drawing.Point(637, 3);
            pictureBoxRecursiveCall.Name = "pictureBoxRecursiveCall";
            pictureBoxRecursiveCall.Size = new System.Drawing.Size(20, 20);
            pictureBoxRecursiveCall.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxRecursiveCall.TabIndex = 4;
            pictureBoxRecursiveCall.TabStop = false;
            // 
            // labelIterationContext
            // 
            labelIterationContext.AutoSize = true;
            labelIterationContext.Dock = System.Windows.Forms.DockStyle.Fill;
            labelIterationContext.ForeColor = System.Drawing.SystemColors.ControlLight;
            labelIterationContext.Location = new System.Drawing.Point(487, 0);
            labelIterationContext.Name = "labelIterationContext";
            labelIterationContext.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            labelIterationContext.Size = new System.Drawing.Size(144, 26);
            labelIterationContext.TabIndex = 3;
            labelIterationContext.Text = "labelIterationContext_Text";
            labelIterationContext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxIterationContext
            // 
            pictureBoxIterationContext.Location = new System.Drawing.Point(461, 3);
            pictureBoxIterationContext.Name = "pictureBoxIterationContext";
            pictureBoxIterationContext.Size = new System.Drawing.Size(20, 20);
            pictureBoxIterationContext.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxIterationContext.TabIndex = 2;
            pictureBoxIterationContext.TabStop = false;
            // 
            // labelInvoke
            // 
            labelInvoke.AutoSize = true;
            labelInvoke.Dock = System.Windows.Forms.DockStyle.Fill;
            labelInvoke.ForeColor = System.Drawing.SystemColors.ControlLight;
            labelInvoke.Location = new System.Drawing.Point(362, 0);
            labelInvoke.Name = "labelInvoke";
            labelInvoke.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            labelInvoke.Size = new System.Drawing.Size(93, 26);
            labelInvoke.TabIndex = 1;
            labelInvoke.Text = "labelInvoke_Text";
            labelInvoke.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxInvoke
            // 
            pictureBoxInvoke.Location = new System.Drawing.Point(336, 3);
            pictureBoxInvoke.Name = "pictureBoxInvoke";
            pictureBoxInvoke.Size = new System.Drawing.Size(20, 20);
            pictureBoxInvoke.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxInvoke.TabIndex = 0;
            pictureBoxInvoke.TabStop = false;
            // 
            // background_Align
            // 
            background_Align.DoWork += Background_Align_DoWork;
            background_Align.RunWorkerCompleted += Background_Align_RunWorkerCompleted;
            // 
            // imageList_Grid
            // 
            imageList_Grid.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList_Grid.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList_Grid.ImageStream");
            imageList_Grid.TransparentColor = System.Drawing.Color.Transparent;
            imageList_Grid.Images.SetKeyName(0, "CardinalityContainer");
            // 
            // ProjectPanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 478);
            Controls.Add(tableLayoutPanel1);
            KeyPreview = true;
            Name = "ProjectPanel";
            Padding = new System.Windows.Forms.Padding(0, 25, 0, 0);
            Text = "Projekt";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            contextMenuStrip.ResumeLayout(false);
            toolStrip_Mode.ResumeLayout(false);
            toolStrip_Mode.PerformLayout();
            toolStrip_Runtime.ResumeLayout(false);
            toolStrip_Runtime.PerformLayout();
            legendPanel.ResumeLayout(false);
            legendPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRecursiveCall).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIterationContext).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxInvoke).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip_Mode;
        private System.Windows.Forms.ToolStripButton btSelectionMode;
        private System.Windows.Forms.ToolStripButton btConnectionMode;
        private System.Windows.Forms.ToolStrip toolStrip_Runtime;
        private System.Windows.Forms.ToolStripDropDownButton itmSelection;
        private System.Windows.Forms.ToolStripMenuItem itmSelection_Left;
        private System.Windows.Forms.ToolStripMenuItem itmSelection_Right;
        private System.Windows.Forms.ToolStripMenuItem itmSelection_Up;
        private System.Windows.Forms.ToolStripMenuItem itmSelection_Down;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem itmSelection_All;
        private System.Windows.Forms.ToolStripLabel toolStrip_Label1;
        private System.Windows.Forms.ToolStripButton btStopExecution;
        private System.Windows.Forms.ToolStripButton btPause;
        private System.Windows.Forms.ToolStripButton btExecute;
        private System.Windows.Forms.ToolStripLabel toolStrip_Label0;
        private System.Windows.Forms.ToolStripButton btGridSettings;
        private Core.DoubleBufferedPanel mainPanel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem itmEditElement;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem itmManageNotifications;
        private System.Windows.Forms.ToolStripMenuItem itmBreakPoint;
        private System.Windows.Forms.ToolStripMenuItem itmIndex;
        private System.Windows.Forms.ToolStripMenuItem itmDefineIndex;
        private System.Windows.Forms.ToolStripMenuItem itmRemoveIndex;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem itmRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem itmDeleteElement;
        private System.ComponentModel.BackgroundWorker background_Align;
        private System.Windows.Forms.ImageList imageList_Grid;
        private System.Windows.Forms.ToolStripMenuItem itmInsightInput;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem itmInsightOutput;
        private System.Windows.Forms.ToolStripMenuItem itmDeleteConnection;
        private System.Windows.Forms.FlowLayoutPanel legendPanel;
        private System.Windows.Forms.Label labelInvoke;
        private System.Windows.Forms.PictureBox pictureBoxInvoke;
        private System.Windows.Forms.Label labelIterationContext;
        private System.Windows.Forms.PictureBox pictureBoxIterationContext;
        private System.Windows.Forms.PictureBox pictureBoxRecursiveCall;
        private System.Windows.Forms.Label labelRecursiveCall;
    }
}
