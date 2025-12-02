using System;

namespace FlowBlox.AppWindow
{
    partial class AppWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppWindow));
            mnItmMisc = new System.Windows.Forms.ToolStripMenuItem();
            itmOpenOutputDir = new System.Windows.Forms.ToolStripMenuItem();
            itmOpenProjectDir = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            itmOpenRuntimeLogDirectory = new System.Windows.Forms.ToolStripMenuItem();
            itmOpenApplicationLogDirectory = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            itmOptions = new System.Windows.Forms.ToolStripMenuItem();
            itmCreateProject = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            saveProjectDialog = new System.Windows.Forms.SaveFileDialog();
            openProjectDialog = new System.Windows.Forms.OpenFileDialog();
            menuStrip = new System.Windows.Forms.MenuStrip();
            mnItmProject = new System.Windows.Forms.ToolStripMenuItem();
            itmOpenProject = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            itmEditProject = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            itmSaveProject = new System.Windows.Forms.ToolStripMenuItem();
            itmSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            itmCloseProject = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            itmQuitApplication = new System.Windows.Forms.ToolStripMenuItem();
            mnItmEdit = new System.Windows.Forms.ToolStripMenuItem();
            itmUndo = new System.Windows.Forms.ToolStripMenuItem();
            itmRedo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            itmCopy = new System.Windows.Forms.ToolStripMenuItem();
            itmPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnItmWindows = new System.Windows.Forms.ToolStripMenuItem();
            itmUserFields = new System.Windows.Forms.ToolStripMenuItem();
            itmDockablePanels = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            itmToolbox = new System.Windows.Forms.ToolStripMenuItem();
            mnItmAccount = new System.Windows.Forms.ToolStripMenuItem();
            itmRegister = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            itmLogin = new System.Windows.Forms.ToolStripMenuItem();
            itmLogout = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            itmManageLicenses = new System.Windows.Forms.ToolStripMenuItem();
            mnItmHelp = new System.Windows.Forms.ToolStripMenuItem();
            itmVisitOnline = new System.Windows.Forms.ToolStripMenuItem();
            itmGitHub = new System.Windows.Forms.ToolStripMenuItem();
            itmCheckForNewVersion = new System.Windows.Forms.ToolStripMenuItem();
            itmReportProblem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            itmAbout = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblApplicationVersion = new System.Windows.Forms.ToolStripStatusLabel();
            lblVersion = new System.Windows.Forms.ToolStripStatusLabel();
            imageList = new System.Windows.Forms.ImageList(components);
            Background_TrialInfo = new System.ComponentModel.BackgroundWorker();
            saveFileDialog_GridImage = new System.Windows.Forms.SaveFileDialog();
            panelSeparator = new System.Windows.Forms.Panel();
            WarningStrip = new System.Windows.Forms.StatusStrip();
            labelWarning = new System.Windows.Forms.ToolStripStatusLabel();
            dockPanel = new FlowBlox.AppWindow.Contents.BufferedDockPanel();
            itmResetDockablePanels = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            WarningStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mnItmMisc
            // 
            mnItmMisc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmOpenOutputDir, itmOpenProjectDir, toolStripSeparator20, itmOpenRuntimeLogDirectory, itmOpenApplicationLogDirectory, toolStripSeparator8, itmOptions });
            mnItmMisc.ForeColor = System.Drawing.SystemColors.ControlText;
            mnItmMisc.Name = "mnItmMisc";
            mnItmMisc.Size = new System.Drawing.Size(106, 19);
            mnItmMisc.Text = "mnItmMisc_Text";
            // 
            // itmOpenOutputDir
            // 
            itmOpenOutputDir.Image = (System.Drawing.Image)resources.GetObject("itmOpenOutputDir.Image");
            itmOpenOutputDir.Name = "itmOpenOutputDir";
            itmOpenOutputDir.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.O;
            itmOpenOutputDir.Size = new System.Drawing.Size(401, 22);
            itmOpenOutputDir.Text = "itmOpenOutputDir_Text";
            itmOpenOutputDir.Click += itmOpenOutputDir_Click;
            // 
            // itmOpenProjectDir
            // 
            itmOpenProjectDir.Image = (System.Drawing.Image)resources.GetObject("itmOpenProjectDir.Image");
            itmOpenProjectDir.Name = "itmOpenProjectDir";
            itmOpenProjectDir.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.P;
            itmOpenProjectDir.Size = new System.Drawing.Size(401, 22);
            itmOpenProjectDir.Text = "itmOpenProjectDir_Text";
            itmOpenProjectDir.Click += itmOpenProjectDir_Click;
            // 
            // toolStripSeparator20
            // 
            toolStripSeparator20.Name = "toolStripSeparator20";
            toolStripSeparator20.Size = new System.Drawing.Size(398, 6);
            // 
            // itmOpenRuntimeLogDirectory
            // 
            itmOpenRuntimeLogDirectory.Image = (System.Drawing.Image)resources.GetObject("itmOpenRuntimeLogDirectory.Image");
            itmOpenRuntimeLogDirectory.Name = "itmOpenRuntimeLogDirectory";
            itmOpenRuntimeLogDirectory.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.L;
            itmOpenRuntimeLogDirectory.Size = new System.Drawing.Size(401, 22);
            itmOpenRuntimeLogDirectory.Text = "itmOpenRuntimeLogDirectory_Text";
            itmOpenRuntimeLogDirectory.Click += itmOpenRuntimeLogDirectory_Click;
            // 
            // itmOpenApplicationLogDirectory
            // 
            itmOpenApplicationLogDirectory.Image = (System.Drawing.Image)resources.GetObject("itmOpenApplicationLogDirectory.Image");
            itmOpenApplicationLogDirectory.Name = "itmOpenApplicationLogDirectory";
            itmOpenApplicationLogDirectory.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.L;
            itmOpenApplicationLogDirectory.Size = new System.Drawing.Size(401, 22);
            itmOpenApplicationLogDirectory.Text = "itmOpenApplicationLogDirectory_Text";
            itmOpenApplicationLogDirectory.Click += itmOpenApplicationLogDirectory_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(398, 6);
            // 
            // itmOptions
            // 
            itmOptions.Image = (System.Drawing.Image)resources.GetObject("itmOptions.Image");
            itmOptions.Name = "itmOptions";
            itmOptions.Size = new System.Drawing.Size(401, 22);
            itmOptions.Text = "itmOptions_Text";
            itmOptions.Click += itmOptions_Click;
            // 
            // itmCreateProject
            // 
            itmCreateProject.Image = (System.Drawing.Image)resources.GetObject("itmCreateProject.Image");
            itmCreateProject.Name = "itmCreateProject";
            itmCreateProject.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P;
            itmCreateProject.Size = new System.Drawing.Size(232, 22);
            itmCreateProject.Text = "itmCreateProject_Text";
            itmCreateProject.Click += rbNewProject_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new System.Drawing.Size(125, 19);
            toolStripMenuItem3.Text = "toolStripMenuItem3";
            // 
            // saveProjectDialog
            // 
            saveProjectDialog.Filter = "*.dsprj (FlowBlox Project)|*.dsprj";
            // 
            // openProjectDialog
            // 
            openProjectDialog.Filter = "*.dsprj (FlowBlox Project)|*.dsprj";
            // 
            // menuStrip
            // 
            menuStrip.BackColor = System.Drawing.SystemColors.Control;
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnItmProject, mnItmEdit, mnItmWindows, mnItmMisc, mnItmAccount, mnItmHelp });
            menuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(1404, 23);
            menuStrip.TabIndex = 4;
            menuStrip.Text = "menuStrip1";
            // 
            // mnItmProject
            // 
            mnItmProject.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmCreateProject, itmOpenProject, toolStripSeparator11, itmEditProject, toolStripSeparator2, itmSaveProject, itmSaveAs, toolStripSeparator3, itmCloseProject, toolStripSeparator4, itmQuitApplication });
            mnItmProject.ForeColor = System.Drawing.SystemColors.ControlText;
            mnItmProject.Name = "mnItmProject";
            mnItmProject.Size = new System.Drawing.Size(118, 19);
            mnItmProject.Text = "mnItmProject_Text";
            // 
            // itmOpenProject
            // 
            itmOpenProject.Image = (System.Drawing.Image)resources.GetObject("itmOpenProject.Image");
            itmOpenProject.Name = "itmOpenProject";
            itmOpenProject.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            itmOpenProject.Size = new System.Drawing.Size(232, 22);
            itmOpenProject.Text = "itmOpenProject_Text";
            itmOpenProject.Click += rbOpenProject_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(229, 6);
            // 
            // itmEditProject
            // 
            itmEditProject.Image = (System.Drawing.Image)resources.GetObject("itmEditProject.Image");
            itmEditProject.Name = "itmEditProject";
            itmEditProject.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            itmEditProject.Size = new System.Drawing.Size(232, 22);
            itmEditProject.Text = "itmEditProject_Text";
            itmEditProject.Click += itmEditProject_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(229, 6);
            // 
            // itmSaveProject
            // 
            itmSaveProject.Image = (System.Drawing.Image)resources.GetObject("itmSaveProject.Image");
            itmSaveProject.Name = "itmSaveProject";
            itmSaveProject.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            itmSaveProject.Size = new System.Drawing.Size(232, 22);
            itmSaveProject.Text = "itmSaveProject_Text";
            itmSaveProject.Click += rbSaveProject_Click;
            // 
            // itmSaveAs
            // 
            itmSaveAs.Image = (System.Drawing.Image)resources.GetObject("itmSaveAs.Image");
            itmSaveAs.Name = "itmSaveAs";
            itmSaveAs.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S;
            itmSaveAs.Size = new System.Drawing.Size(232, 22);
            itmSaveAs.Text = "itmSaveAs_Text";
            itmSaveAs.Click += rbSaveProjectAs_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(229, 6);
            // 
            // itmCloseProject
            // 
            itmCloseProject.Image = (System.Drawing.Image)resources.GetObject("itmCloseProject.Image");
            itmCloseProject.Name = "itmCloseProject";
            itmCloseProject.Size = new System.Drawing.Size(232, 22);
            itmCloseProject.Text = "itmCloseProject_Text";
            itmCloseProject.Click += itmCloseProject_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(229, 6);
            // 
            // itmQuitApplication
            // 
            itmQuitApplication.Image = (System.Drawing.Image)resources.GetObject("itmQuitApplication.Image");
            itmQuitApplication.Name = "itmQuitApplication";
            itmQuitApplication.Size = new System.Drawing.Size(232, 22);
            itmQuitApplication.Text = "itmQuitApplication_Text";
            itmQuitApplication.Click += itmQuitApplication_Click;
            // 
            // mnItmEdit
            // 
            mnItmEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmUndo, itmRedo, toolStripSeparator10, itmCopy, itmPaste });
            mnItmEdit.Name = "mnItmEdit";
            mnItmEdit.Size = new System.Drawing.Size(101, 19);
            mnItmEdit.Text = "mnItmEdit_Text";
            // 
            // itmUndo
            // 
            itmUndo.Image = (System.Drawing.Image)resources.GetObject("itmUndo.Image");
            itmUndo.Name = "itmUndo";
            itmUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            itmUndo.Size = new System.Drawing.Size(190, 22);
            itmUndo.Text = "itmUndo_Text";
            itmUndo.Click += itmUndo_Click;
            // 
            // itmRedo
            // 
            itmRedo.Image = (System.Drawing.Image)resources.GetObject("itmRedo.Image");
            itmRedo.Name = "itmRedo";
            itmRedo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y;
            itmRedo.Size = new System.Drawing.Size(190, 22);
            itmRedo.Text = "itmRedo_Text";
            itmRedo.Click += itmRedo_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(187, 6);
            // 
            // itmCopy
            // 
            itmCopy.Image = (System.Drawing.Image)resources.GetObject("itmCopy.Image");
            itmCopy.Name = "itmCopy";
            itmCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            itmCopy.Size = new System.Drawing.Size(190, 22);
            itmCopy.Text = "itmCopy_Text";
            itmCopy.Click += itmCopy_Click;
            // 
            // itmPaste
            // 
            itmPaste.Image = (System.Drawing.Image)resources.GetObject("itmPaste.Image");
            itmPaste.Name = "itmPaste";
            itmPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            itmPaste.Size = new System.Drawing.Size(190, 22);
            itmPaste.Text = "itmPaste_Text";
            itmPaste.Click += itmPaste_Click;
            // 
            // mnItmWindows
            // 
            mnItmWindows.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmUserFields, itmDockablePanels, itmResetDockablePanels, toolStripSeparator6, itmToolbox });
            mnItmWindows.ForeColor = System.Drawing.SystemColors.ControlText;
            mnItmWindows.Name = "mnItmWindows";
            mnItmWindows.Size = new System.Drawing.Size(130, 19);
            mnItmWindows.Text = "mnItmWindows_Text";
            // 
            // itmUserFields
            // 
            itmUserFields.Image = (System.Drawing.Image)resources.GetObject("itmUserFields.Image");
            itmUserFields.Name = "itmUserFields";
            itmUserFields.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U;
            itmUserFields.Size = new System.Drawing.Size(215, 22);
            itmUserFields.Text = "itmUserFields_Text";
            itmUserFields.Click += itmUserFields_Click;
            // 
            // itmDockablePanels
            // 
            itmDockablePanels.Name = "itmDockablePanels";
            itmDockablePanels.Size = new System.Drawing.Size(215, 22);
            itmDockablePanels.Text = "itmDockablePanels_Text";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(212, 6);
            // 
            // itmToolbox
            // 
            itmToolbox.Image = (System.Drawing.Image)resources.GetObject("itmToolbox.Image");
            itmToolbox.Name = "itmToolbox";
            itmToolbox.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T;
            itmToolbox.Size = new System.Drawing.Size(215, 22);
            itmToolbox.Text = "itmToolbox_Text";
            itmToolbox.Click += itmToolbox_Click;
            // 
            // mnItmAccount
            // 
            mnItmAccount.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmRegister, toolStripSeparator1, itmLogin, itmLogout, toolStripSeparator5, itmManageLicenses });
            mnItmAccount.Name = "mnItmAccount";
            mnItmAccount.Size = new System.Drawing.Size(126, 19);
            mnItmAccount.Text = "mnItmAccount_Text";
            // 
            // itmRegister
            // 
            itmRegister.Image = (System.Drawing.Image)resources.GetObject("itmRegister.Image");
            itmRegister.Name = "itmRegister";
            itmRegister.Size = new System.Drawing.Size(205, 22);
            itmRegister.Text = "itmRegister_Text";
            itmRegister.Click += itmRegister_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
            // 
            // itmLogin
            // 
            itmLogin.Image = (System.Drawing.Image)resources.GetObject("itmLogin.Image");
            itmLogin.Name = "itmLogin";
            itmLogin.Size = new System.Drawing.Size(205, 22);
            itmLogin.Text = "itmLogin_Text";
            itmLogin.Click += itmLogin_Click;
            // 
            // itmLogout
            // 
            itmLogout.Image = (System.Drawing.Image)resources.GetObject("itmLogout.Image");
            itmLogout.Name = "itmLogout";
            itmLogout.Size = new System.Drawing.Size(205, 22);
            itmLogout.Text = "itmLogout_Text";
            itmLogout.Click += itmLogout_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(202, 6);
            // 
            // itmManageLicenses
            // 
            itmManageLicenses.Image = (System.Drawing.Image)resources.GetObject("itmManageLicenses.Image");
            itmManageLicenses.Name = "itmManageLicenses";
            itmManageLicenses.Size = new System.Drawing.Size(205, 22);
            itmManageLicenses.Text = "itmManageLicenses_Text";
            // 
            // mnItmHelp
            // 
            mnItmHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { itmVisitOnline, itmGitHub, itmCheckForNewVersion, itmReportProblem, toolStripSeparator15, itmAbout });
            mnItmHelp.ForeColor = System.Drawing.SystemColors.ControlText;
            mnItmHelp.Name = "mnItmHelp";
            mnItmHelp.Size = new System.Drawing.Size(106, 19);
            mnItmHelp.Text = "mnItmHelp_Text";
            // 
            // itmVisitOnline
            // 
            itmVisitOnline.Image = (System.Drawing.Image)resources.GetObject("itmVisitOnline.Image");
            itmVisitOnline.Name = "itmVisitOnline";
            itmVisitOnline.Size = new System.Drawing.Size(230, 22);
            itmVisitOnline.Text = "itmVisitOnline_Text";
            itmVisitOnline.Click += itmVisitOnline_Click;
            // 
            // itmGitHub
            // 
            itmGitHub.Image = (System.Drawing.Image)resources.GetObject("itmGitHub.Image");
            itmGitHub.Name = "itmGitHub";
            itmGitHub.Size = new System.Drawing.Size(230, 22);
            itmGitHub.Text = "itmGitHub_Text";
            itmGitHub.Click += itmGitHub_Click;
            // 
            // itmCheckForNewVersion
            // 
            itmCheckForNewVersion.Image = (System.Drawing.Image)resources.GetObject("itmCheckForNewVersion.Image");
            itmCheckForNewVersion.Name = "itmCheckForNewVersion";
            itmCheckForNewVersion.Size = new System.Drawing.Size(230, 22);
            itmCheckForNewVersion.Text = "itmCheckForNewVersion_Text";
            itmCheckForNewVersion.Click += itmCheckForNewVersion_Click;
            // 
            // itmReportProblem
            // 
            itmReportProblem.Image = (System.Drawing.Image)resources.GetObject("itmReportProblem.Image");
            itmReportProblem.Name = "itmReportProblem";
            itmReportProblem.Size = new System.Drawing.Size(230, 22);
            itmReportProblem.Text = "itmReportProblem_Text";
            itmReportProblem.Click += itmReportProblem_Click;
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new System.Drawing.Size(227, 6);
            // 
            // itmAbout
            // 
            itmAbout.Image = (System.Drawing.Image)resources.GetObject("itmAbout.Image");
            itmAbout.Name = "itmAbout";
            itmAbout.Size = new System.Drawing.Size(230, 22);
            itmAbout.Text = "itmAbout_Text";
            itmAbout.Click += itmAbout_Click;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = System.Drawing.Color.LightSlateGray;
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblApplicationVersion, lblVersion });
            statusStrip.Location = new System.Drawing.Point(0, 939);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1404, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 5;
            // 
            // lblApplicationVersion
            // 
            lblApplicationVersion.Font = new System.Drawing.Font("Calibri", 9F);
            lblApplicationVersion.ForeColor = System.Drawing.Color.White;
            lblApplicationVersion.Name = "lblApplicationVersion";
            lblApplicationVersion.Size = new System.Drawing.Size(235, 17);
            lblApplicationVersion.Text = "Welcome. You are using flowblox $Version";
            // 
            // lblVersion
            // 
            lblVersion.Font = new System.Drawing.Font("Calibri", 9F);
            lblVersion.ForeColor = System.Drawing.Color.White;
            lblVersion.Image = (System.Drawing.Image)resources.GetObject("lblVersion.Image");
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(162, 17);
            lblVersion.Text = "Trial Version - 30 days left";
            // 
            // imageList
            // 
            imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = System.Drawing.Color.Transparent;
            imageList.Images.SetKeyName(0, "TrialLow");
            imageList.Images.SetKeyName(1, "TrialNormal");
            imageList.Images.SetKeyName(2, "Full");
            // 
            // Background_TrialInfo
            // 
            Background_TrialInfo.DoWork += Background_TrialInfo_DoWork;
            Background_TrialInfo.RunWorkerCompleted += Background_TrialInfo_RunWorkerCompleted;
            // 
            // saveFileDialog_GridImage
            // 
            saveFileDialog_GridImage.Filter = "24-Bit-Bitmap|*.bmp";
            // 
            // panelSeparator
            // 
            panelSeparator.BackColor = System.Drawing.Color.WhiteSmoke;
            panelSeparator.Dock = System.Windows.Forms.DockStyle.Top;
            panelSeparator.ForeColor = System.Drawing.SystemColors.Control;
            panelSeparator.Location = new System.Drawing.Point(0, 23);
            panelSeparator.Name = "panelSeparator";
            panelSeparator.Size = new System.Drawing.Size(1404, 2);
            panelSeparator.TabIndex = 6;
            // 
            // WarningStrip
            // 
            WarningStrip.BackColor = System.Drawing.Color.DarkGoldenrod;
            WarningStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { labelWarning });
            WarningStrip.Location = new System.Drawing.Point(0, 667);
            WarningStrip.Name = "WarningStrip";
            WarningStrip.Size = new System.Drawing.Size(1404, 22);
            WarningStrip.SizingGrip = false;
            WarningStrip.TabIndex = 7;
            WarningStrip.Text = "statusStrip1";
            WarningStrip.Visible = false;
            // 
            // labelWarning
            // 
            labelWarning.ForeColor = System.Drawing.Color.WhiteSmoke;
            labelWarning.Name = "labelWarning";
            labelWarning.Size = new System.Drawing.Size(58, 17);
            labelWarning.Text = "$Warning";
            // 
            // dockPanel
            // 
            dockPanel.BackColor = System.Drawing.SystemColors.Control;
            dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            dockPanel.Location = new System.Drawing.Point(0, 25);
            dockPanel.Name = "dockPanel";
            dockPanel.Size = new System.Drawing.Size(1404, 914);
            dockPanel.TabIndex = 8;
            dockPanel.ContentAdded += DockPanel_ContentAdded;
            dockPanel.ContentRemoved += DockPanel_ContentRemoved;
            // 
            // itmResetDockablePanels
            // 
            itmResetDockablePanels.Image = (System.Drawing.Image)resources.GetObject("itmResetDockablePanels.Image");
            itmResetDockablePanels.Name = "itmResetDockablePanels";
            itmResetDockablePanels.Size = new System.Drawing.Size(229, 22);
            itmResetDockablePanels.Text = "itmResetDockablePanels_Text";
            itmResetDockablePanels.Click += itmResetDockablePanels_Click;
            // 
            // AppWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ClientSize = new System.Drawing.Size(1404, 961);
            Controls.Add(dockPanel);
            Controls.Add(WarningStrip);
            Controls.Add(panelSeparator);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Segoe UI", 8.25F);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            Name = "AppWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "FlowBlox";
            FormClosing += AppWindow_FormClosing;
            Load += AppWindow_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            WarningStrip.ResumeLayout(false);
            WarningStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.SaveFileDialog saveProjectDialog;
        private System.Windows.Forms.OpenFileDialog openProjectDialog;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnItmProject;
        private System.Windows.Forms.ToolStripMenuItem itmCreateProject;
        private System.Windows.Forms.ToolStripMenuItem itmOpenProject;
        private System.Windows.Forms.ToolStripMenuItem itmSaveProject;
        private System.Windows.Forms.ToolStripMenuItem itmSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem itmCloseProject;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem itmQuitApplication;
        private System.Windows.Forms.ToolStripMenuItem mnItmWindows;
        private System.Windows.Forms.ToolStripMenuItem mnItmMisc;
        private System.Windows.Forms.ToolStripMenuItem itmOptions;
        private System.Windows.Forms.ToolStripMenuItem itmUserFields;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem itmToolbox;
        private System.Windows.Forms.ToolStripMenuItem itmEditProject;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem itmOpenOutputDir;
        private System.Windows.Forms.ToolStripMenuItem mnItmHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem itmAbout;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblVersion;
        private System.Windows.Forms.ImageList imageList;
        private System.ComponentModel.BackgroundWorker Background_TrialInfo;
        private System.Windows.Forms.ToolStripStatusLabel lblApplicationVersion;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.SaveFileDialog saveFileDialog_GridImage;
        private System.Windows.Forms.Panel panelSeparator;
        private System.Windows.Forms.ToolStripMenuItem mnItmEdit;
        private System.Windows.Forms.ToolStripMenuItem itmUndo;
        private System.Windows.Forms.ToolStripMenuItem itmRedo;
        private System.Windows.Forms.ToolStripMenuItem itmReportProblem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripMenuItem itmOpenApplicationLogDirectory;
        public System.Windows.Forms.StatusStrip WarningStrip;
        public System.Windows.Forms.ToolStripStatusLabel labelWarning;
        private System.Windows.Forms.ToolStripMenuItem itmOpenProjectDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem itmCopy;
        private System.Windows.Forms.ToolStripMenuItem itmPaste;
        private FlowBlox.AppWindow.Contents.BufferedDockPanel dockPanel;
        private System.Windows.Forms.ToolStripMenuItem itmOpenRuntimeLogDirectory;
        private System.Windows.Forms.ToolStripMenuItem mnItmAccount;
        private System.Windows.Forms.ToolStripMenuItem itmLogin;
        private System.Windows.Forms.ToolStripMenuItem itmLogout;
        private System.Windows.Forms.ToolStripMenuItem itmRegister;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem itmManageLicenses;
        private System.Windows.Forms.ToolStripMenuItem itmDockablePanels;
        private System.Windows.Forms.ToolStripMenuItem itmVisitOnline;
        private System.Windows.Forms.ToolStripMenuItem itmGitHub;
        private System.Windows.Forms.ToolStripMenuItem itmCheckForNewVersion;
        private System.Windows.Forms.ToolStripMenuItem itmResetDockablePanels;
    }
}

