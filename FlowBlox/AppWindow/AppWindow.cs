using FlowBlox.AppWindow.ContentFactories;
using FlowBlox.AppWindow.Contents;
using FlowBlox.Core;
using FlowBlox.Core.Authentication;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Interceptors;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.ObjectManager;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Services;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Util.WPF;
using FlowBlox.Grid.Provider;
using FlowBlox.UICore.Views;
using FlowBlox.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static FlowBlox.Core.Interceptors.RuntimeBacktraceInterceptor;

namespace FlowBlox.AppWindow
{
    public partial class AppWindow : Form
    {
        private static AppWindow _appWindow;
        public static AppWindow Instance
        {
            get
            {
                if (_appWindow == null)
                    _appWindow = new AppWindow();

                return _appWindow;
            }
        }

        public static void InitApp()
        {
            foreach (var assemblyPreloader in FlowBloxServiceLocator.Instance.GetServices<IAssemblyPreloader>())
            {
                assemblyPreloader.PreloadReferencedAssemblies();
            }
        }

        private DockableObjectManagerInitializer _objectManagerInitializer;

        private ProjectPanel _dockContentProjectPanel;

        private string _recentProjectPath;
        private ComponentLibraryPanel _componentLibraryPanel;
        private DockContentUserControlWrapper<FieldView> _fieldViewPanel;
        private DockContentUserControlWrapper<RuntimeView> _runtimeViewPanel;
        private DockContentUserControlWrapper<ProblemsView> _problemsViewPanel;

        private FlowBloxProjectComponentProvider _componentProvider;

        public AppWindow()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);

            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();

            this.UpdateUI();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_componentLibraryPanel?.ProcessCmdKey(ref msg, keyData) == true)
                return true;

            if (_fieldViewPanel?.UserControl.ProcessCmdKey(ref msg, keyData) == true)
                return true;

            if (_dockContentProjectPanel?.ProcessCmdKey(ref msg, keyData) == true)
                return true;

            if (_runtimeViewPanel?.UserControl.ProcessCmdKey(ref msg, keyData) == true)
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public bool IsRuntimeActive => _dockContentProjectPanel.IsRuntimeActive;

        private string _runtimeLogfilePath;
        public string RuntimeLogfilePath
        {
            get
            {
                var runtimeLogfilePath = _dockContentProjectPanel?.RuntimeLogfilePath;
                if (!string.IsNullOrEmpty(runtimeLogfilePath))
                    _runtimeLogfilePath = runtimeLogfilePath;
                return _runtimeLogfilePath;
            }
        }

        public void UpdateUI()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(UpdateUI));
                return;
            }

            var project = FlowBloxProjectManager.Instance.ActiveProject;
            var isRuntimeActive = false;
            if (_dockContentProjectPanel?.IsRuntimeActive == true)
                isRuntimeActive = true;

            dockPanel.Visible = project != null;
            BackColor = project != null ?
                Color.FromKnownColor(KnownColor.Control) :
                Color.FromArgb(53, 53, 53);

            itmCreateProject.Enabled = !isRuntimeActive;
            itmOpenProject.Enabled = !isRuntimeActive;
            itmCloseProject.Enabled = (project != null) && !isRuntimeActive;
            itmUserFields.Enabled = (project != null) && !isRuntimeActive;

            itmEditProject.Enabled = (project != null) && (!isRuntimeActive);
            itmSaveProject.Enabled = (project != null) && (!isRuntimeActive);
            itmSaveAs.Enabled = (project != null) && (!isRuntimeActive);

            itmDockablePanels.Enabled = project != null;
            itmResetDockablePanels.Enabled = project != null;

            itmOpenRuntimeLogDirectory.Enabled = !string.IsNullOrEmpty(RuntimeLogfilePath);

            itmLogin.Enabled = !FlowBloxAccountManager.Instance.IsLoggedIn;
            itmLogout.Enabled = FlowBloxAccountManager.Instance.IsLoggedIn;

            if (isRuntimeActive && !_runtimeViewPanel.IsHidden)
            {
                _runtimeViewPanel.Activate();
            }

            var changelist = _componentProvider.GetCurrentChangelist();
            this.itmUndo.Enabled = (changelist != null) && (!isRuntimeActive) && (changelist.ChangeIndex > -1);
            this.itmRedo.Enabled = (changelist != null) && (!isRuntimeActive) && (changelist.ChangeIndex < changelist.Changes.Count - 1);

            this._dockContentProjectPanel?.UpdateUI();
            this._componentLibraryPanel?.UpdateUI();
        }

        private void InitializeDockPanel(bool exceptProjectPanel = false)
        {
            this.dockPanel.SuspendLayout();
            
            this.dockPanel.Theme = new VS2015DarkTheme();

            foreach (var dockContent in dockPanel.Contents
                .OfType<DockContent>()
                .Where(x => !exceptProjectPanel || x is not ProjectPanel).ToList())
            {
                dockContent.Close();
            }

            if (!exceptProjectPanel)
            {
                var projectPanelFactory = new ProjectPanelFactory(dockPanel);
                _dockContentProjectPanel = projectPanelFactory.Create();
            }

            var componentLibraryPanelFactory = new ComponentLibraryPanelFactory(dockPanel);
            _componentLibraryPanel = componentLibraryPanelFactory.Create();

            var fieldViewPanelFactory = new FieldViewPanelFactory(dockPanel);
            _fieldViewPanel = fieldViewPanelFactory.Create();

            _objectManagerInitializer = new DockableObjectManagerInitializer(dockPanel);
            _objectManagerInitializer.InitializeAllObjectManager();

            var problemsViewPanelFactory = new ProblemsViewPanelFactory(dockPanel);
            _problemsViewPanel = problemsViewPanelFactory.Create();

            var runtimeViewPanelFactory = new RuntimeViewPanelFactory(dockPanel);
            _runtimeViewPanel = runtimeViewPanelFactory.Create();

            this.dockPanel.ResumeLayout();
        }

        private void DockPanel_ContentAdded(object sender, DockContentEventArgs e)
        {
            var dockContent = ((DockContent)e.Content);

            var toolstripMenuItem = new ToolStripMenuItem()
            {
                Text = dockContent.Text
            };

            toolstripMenuItem.Click += (s, e2) =>
            {
                dockContent.Show();
            };

            itmDockablePanels.DropDownItems.Add(toolstripMenuItem);

            FlowBloxStyle.ApplyStyle(this.menuStrip);
        }

        private void DockPanel_ContentRemoved(object sender, DockContentEventArgs e)
        {
            var removedContent = (DockContent)e.Content;
            foreach (ToolStripItem item in itmDockablePanels.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Text == removedContent.Name)
                {
                    itmDockablePanels.DropDownItems.Remove(item);
                    break;
                }
            }
        }

        private void UpdateUI_ProjectName()
        {
            var project = FlowBloxProjectManager.Instance.ActiveProject;
            if (!string.IsNullOrEmpty(project?.ProjectName))
            {
                this.Text = FlowBloxResourceUtil.GetLocalizedString("AppWindow_Text", typeof(FlowBloxMainUITexts)) + " \"" + project.ProjectName + "\"";
            }
            else
            {
                this.Text = FlowBloxResourceUtil.GetLocalizedString("AppWindow_Text", typeof(FlowBloxMainUITexts));
            }
        }

        private void rbNewProject_Click(object sender, EventArgs e)
        {
            bool createProject = true;

            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                DialogResult Result = FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_NewProjectConfirm_Message", typeof(FlowBloxMainUITexts)) + "\r\n" +
                        "\r\n" +
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveReminder_Message", typeof(FlowBloxMainUITexts)),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_NewProjectConfirm_Title", typeof(FlowBloxMainUITexts)), FlowBloxMessageBox.Buttons.YesNo, FlowBloxMessageBox.Icons.Question
                    );

                if (Result == DialogResult.No)
                {
                    createProject = false;
                }
            }

            if (createProject)
            {
                this.CloseProject();
                this.CreateProject();
            }

            UpdateUI();
        }

        private void UnloadProject()
        {
            try
            {
                FlowBloxProjectManager.Instance.ActiveProject = null;
                FlowBloxProjectManager.Instance.ActiveProjectPath = null;
            }
            catch (ProjectExtensionsUnloadException ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();

                logger.Error("Failed to unload project due to remaining extensions.", ex);

                if (ex.RemainingAssemblies?.Any() == true)
                {
                    logger.Warn("Remaining Assemblies:");
                    foreach (var asm in ex.RemainingAssemblies)
                        logger.Warn($" - {asm}");
                }

                if (ex.UnloadableExtensions?.Any() == true)
                {
                    logger.Warn("Unloadable Extensions:");
                    foreach (var ext in ex.UnloadableExtensions)
                        logger.Warn($" - {ext}");
                }

                string title = FlowBloxResourceUtil.GetLocalizedString("AppWindow_UnloadProjectFailed_Title", typeof(FlowBloxMainUITexts));
                string message = string.Format(
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_UnloadProjectFailed_Message", typeof(FlowBloxMainUITexts)),
                    string.Join(Environment.NewLine, ex.RemainingAssemblies.Concat(ex.UnloadableExtensions)));

                FlowBloxMessageBox.Show(
                    this,
                    message,
                    title,
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Exclamation
                );

                string exePath = System.Windows.Forms.Application.ExecutablePath;
                using var _ = Process.Start(exePath);
                Environment.Exit(0);
            }
        }



        private void CreateProject()
        {
            var project = new FlowBloxProject();
            this._recentProjectPath = string.Empty;

            ProjectView projectView = new ProjectView(project, ProjectViewMode.CreateProject);
            projectView.ShowDialog(this);
            if (string.IsNullOrEmpty(project.ProjectName))
            {
                UnloadProject();
            }
            else
            {
                FlowBloxProjectManager.Instance.ActiveProject = project;
                FlowBloxProjectManager.Instance.ActiveProjectPath = _recentProjectPath;
                this.OnAfterUIRegistryInitialized();
                OnAfterProjectCreated();
                UpdateUI_ProjectName();
            }
        }

        private void OnAfterProjectCreated()
        {
            this._dockContentProjectPanel.OnAfterProjectCreated();
            this.UpdateUI();
        }

        private void OnAfterUIRegistryInitialized()
        {
            InitializeDockPanel();
            this._fieldViewPanel.UserControl.OnAfterUIRegistryInitialized();
            this._dockContentProjectPanel.OnAfterUIRegistryInitialized();
        }

        private void CloseProject()
        {
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                UnloadProject();
                this.itmDockablePanels.DropDownItems.Clear();
                this._dockContentProjectPanel?.OnAfterProjectClosed();
                UpdateUI();
                UpdateUI_ProjectName();
            }
        }

        private void rbOpenProject_Click(object sender, EventArgs e)
        {
            bool openProject = true;

            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                DialogResult Result = FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_OpenProjectConfirm_Message", typeof(FlowBloxMainUITexts)) + "\r\n" +
                        "\r\n" +
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveReminder_Message", typeof(FlowBloxMainUITexts)),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_OpenProjectConfirm_Title", typeof(FlowBloxMainUITexts)), FlowBloxMessageBox.Buttons.YesNo, FlowBloxMessageBox.Icons.Question
                    );

                if (Result == DialogResult.No)
                {
                    openProject = false;
                }
            }

            if (openProject)
            {
                openProjectDialog.InitialDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ProjectDir"].Value;
                if (!Directory.Exists(openProjectDialog.InitialDirectory))
                {
                    Directory.CreateDirectory(openProjectDialog.InitialDirectory);
                }
                if (openProjectDialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.CloseProject();

                    this._recentProjectPath = openProjectDialog.FileName;

                    UnloadProject();
                    OpenProjectFromRecentProjectPath();
                }
            }
            else
            {
                UpdateUI();
            }
        }

        private void rbSaveProject_Click(object sender, EventArgs e)
        {
            try
            {
                if (_recentProjectPath.Equals(string.Empty))
                {
                    saveProjectDialog.InitialDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ProjectDir"].Value;

                    if (!Directory.Exists(saveProjectDialog.InitialDirectory))
                        Directory.CreateDirectory(saveProjectDialog.InitialDirectory);

                    if (saveProjectDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        _recentProjectPath = saveProjectDialog.FileName;

                        OnBeforeSaveProject(FlowBloxProjectManager.Instance.ActiveProject);
                        FlowBloxProjectManager.Instance.ActiveProject.Save(_recentProjectPath);
                        FlowBloxProjectManager.Instance.ActiveProjectPath = _recentProjectPath;
                    }
                }
                else
                {
                    OnBeforeSaveProject(FlowBloxProjectManager.Instance.ActiveProject);
                    FlowBloxProjectManager.Instance.ActiveProject.Save(_recentProjectPath);
                }
            }
            catch (Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);

                FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveFailed_Message", typeof(FlowBloxMainUITexts)) + ex.Message,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveFailed_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Error
                    );
            }

            UpdateUI();
        }

        private void rbSaveProjectAs_Click(object sender, EventArgs e)
        {
            try
            {
                saveProjectDialog.InitialDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ProjectDir"].Value;

                if (!Directory.Exists(saveProjectDialog.InitialDirectory))
                {
                    Directory.CreateDirectory(saveProjectDialog.InitialDirectory);
                }

                if (saveProjectDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _recentProjectPath = saveProjectDialog.FileName;

                    OnBeforeSaveProject(FlowBloxProjectManager.Instance.ActiveProject);
                    FlowBloxProjectManager.Instance.ActiveProject.Save(_recentProjectPath);
                    FlowBloxProjectManager.Instance.ActiveProjectPath = _recentProjectPath;
                }
            }
            catch (Exception Exception)
            {
                FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveFailed_Message", typeof(FlowBloxMainUITexts)) + Exception.Message,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveFailed_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Error
                    );
            }

            UpdateUI();
        }

        private void itmQuitApplication_Click(object sender, EventArgs e)
        {
            if (FlowBloxProjectManager.Instance.ActiveProject != null)
            {
                DialogResult Result = FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveBeforeExit_Message", typeof(FlowBloxMainUITexts)) + FlowBloxProjectManager.Instance.ActiveProject.ProjectName + "\"?",
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveBeforeExit_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.YesNo,
                        FlowBloxMessageBox.Icons.Question
                    );

                if (Result == DialogResult.Yes)
                {
                    if (_recentProjectPath.Equals(string.Empty))
                    {
                        if (saveProjectDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            _recentProjectPath = saveProjectDialog.FileName;

                            OnBeforeSaveProject(FlowBloxProjectManager.Instance.ActiveProject);
                            FlowBloxProjectManager.Instance.ActiveProject.Save(_recentProjectPath);
                            FlowBloxProjectManager.Instance.ActiveProjectPath = _recentProjectPath;
                        }
                    }
                    else
                    {
                        OnBeforeSaveProject(FlowBloxProjectManager.Instance.ActiveProject);
                        FlowBloxProjectManager.Instance.ActiveProject.Save(_recentProjectPath);
                    }
                }
            }

            this.Close();
        }

        private void OnBeforeSaveProject(FlowBloxProject project)
        {
            this._dockContentProjectPanel.OnBeforeSaveProject(project);
        }

        private void itmOptions_Click(object sender, EventArgs e)
        {
            OptionWindow OptionWindow = new OptionWindow(null);
            OptionWindow.ShowDialog(this);
            UpdateUI();
        }

        private void itmUserFields_Click(object sender, EventArgs e)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            UserFieldObjectManager userFieldObjectManager = new UserFieldObjectManager(registry);
            var propertyViewWpf = new UICore.Views.PropertyWindow(new PropertyWindowArgs(userFieldObjectManager, deepCopy: false, canSave: false));
            propertyViewWpf.Height = 800;
            WindowsFormWPFHelper.ShowDialog(propertyViewWpf, this);
            UpdateUI();
        }

        private void itmCloseProject_Click(object sender, EventArgs e)
        {
            if (FlowBloxProjectManager.Instance.ActiveProject == null)
                return;

            DialogResult Result = FlowBloxMessageBox.Show
                (
                    this,
                    string.Format(FlowBloxResourceUtil.GetLocalizedString("AppWindow_CloseProjectConfirm_Message", typeof(FlowBloxMainUITexts)), FlowBloxProjectManager.Instance.ActiveProject.ProjectName) + "\r\n" +
                    "\r\n" +
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_SaveReminder_Message", typeof(FlowBloxMainUITexts)),
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_CloseProjectConfirm_Title", typeof(FlowBloxMainUITexts)), FlowBloxMessageBox.Buttons.YesNo, FlowBloxMessageBox.Icons.Question
                );

            if (Result == DialogResult.Yes)
            {
                this.CloseProject();
            }

            UpdateUI();
        }

        private void itmRefresh_Click(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void OpenProjectFromRecentProjectPath()
        {
            FlowBloxProject project;
            try
            {
                project = FlowBloxProject.FromFile(_recentProjectPath);
                FlowBloxProjectManager.Instance.ActiveProject = project;
                FlowBloxProjectManager.Instance.ActiveProjectPath = _recentProjectPath;
                this.OnAfterUIRegistryInitialized();
                this.OnAfterProjectOpened(project);
            }
            catch (Exception e)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(e);

                FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_ProjectLoadFailed_Message", typeof(FlowBloxMainUITexts)),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_ProjectLoadFailed_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Warning
                    );

                FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_ErrorReport_Message", typeof(FlowBloxMainUITexts)) + "\r\n" +
                        "\r\n" +
                        "Exception:" + "\r\n" +
                        e.ToString(),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_ErrorReport_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Info
                    );

                return;
            }

            if (!project.Notice.Equals(string.Empty))
            {
                FlowBloxMessageBox.Show
                    (
                        this,
                        project.Notice,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_ProjectNotice_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Info
                    );
            }

            UpdateUI_ProjectName();
        }

        private void OnAfterProjectOpened(FlowBloxProject project)
        {
            this._dockContentProjectPanel.OnAfterProjectOpened(project);
            this.UpdateUI();
        }

        private void AppWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dockContentProjectPanel?.IsRuntimeActive == true)
            {
                DialogResult Result = FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_RuntimeActiveClose_Message", typeof(FlowBloxMainUITexts)),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_RuntimeActiveClose_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Info
                    );

                e.Cancel = true;
            }
            else
            {
                try
                {
                    Environment.Exit(0);
                }
                catch (Exception)
                {
                }
            }
        }

        private void itmToolbox_Click(object sender, EventArgs e)
        {
            var dialog = new ToolboxWindow();
            WindowsFormWPFHelper.ShowDialog(dialog, this.FindForm());
        }

        private void itmEditProject_Click(object sender, EventArgs e)
        {
            if (FlowBloxProjectManager.Instance.ActiveProject == null)
                return;

            ProjectView ProjectView = new ProjectView(FlowBloxProjectManager.Instance.ActiveProject, ProjectViewMode.EditProject);
            ProjectView.ShowDialog(this);
            UpdateUI();
            UpdateUI_ProjectName();
        }

        private void AppWindow_Load(object sender, EventArgs e)
        {
            InitVersion();
            InitProjectFile();
            Background_TrialInfo.RunWorkerAsync();
        }

        public void SetProjectFile(string projectFile) => _recentProjectPath = projectFile;

        private void InitProjectFile()
        {
            if (File.Exists(_recentProjectPath))
                OpenProjectFromRecentProjectPath();
        }

        private void itmOpenOutputDir_Click(object sender, EventArgs e)
        {
            string outputDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.OutputDir"].Value;
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = outputDirectory,
                UseShellExecute = true
            });
        }

        private void itmAbout_Click(object sender, EventArgs e)
        {
            About AboutWindow = new About();
            AboutWindow.ShowDialog(this);
            UpdateUI();
        }

        private void InitVersion()
        {
            lblApplicationVersion.Text = lblApplicationVersion.Text.Replace("$Version", Application.ProductVersion);
        }

        private void Background_TrialInfo_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(8000);
        }

        private void Background_TrialInfo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statusStrip.Visible = false;
        }

        private void itmLicense_Click(object sender, EventArgs e)
        {

        }

        private void itmSaveGridImage_Click(object sender, EventArgs e)
        {
            try
            {
                string OutputDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.OutputDir"].Value;
                saveFileDialog_GridImage.InitialDirectory = OutputDirectory;
                if (!Directory.Exists(OutputDirectory))
                {
                    Directory.CreateDirectory(OutputDirectory);
                }
                if (saveFileDialog_GridImage.ShowDialog(this) == DialogResult.OK)
                    _dockContentProjectPanel.SaveInnerPanelBitmap(saveFileDialog_GridImage.FileName);
            }
            catch (Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);

                FlowBloxMessageBox.Show
                    (
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_GridExportFailed_Message", typeof(FlowBloxMainUITexts)),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_GridExportFailed_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Warning
                    );
            }

            UpdateUI();
        }

        private void View_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateUI();
        }

        private void itmUndo_Click(object sender, EventArgs e)
        {
            _dockContentProjectPanel.Undo();
        }

        private void itmRedo_Click(object sender, EventArgs e)
        {
            _dockContentProjectPanel.Redo();
        }

        private void itmOpenApplicationLogDirectory_Click(object sender, EventArgs e)
        {
            var logger = FlowBloxLogManager.Instance.GetLogger();
            var logPath = logger.GetLogfilePath();

            if (File.Exists(logPath))
            {
                var logDirectory = Path.GetDirectoryName(logPath);
                Process.Start("explorer.exe", logDirectory);
            }
        }

        private void itmOpenRuntimeLogDirectory_Click(object sender, EventArgs e)
        {
            var logger = FlowBloxLogManager.Instance.GetLogger();
            var logPath = logger.GetLogfilePath();

            if (File.Exists(logPath))
            {
                var logDirectory = Path.GetDirectoryName(logPath);
                Process.Start("explorer.exe", logDirectory);
            }
        }

        private void itmOpenProjectDir_Click(object sender, EventArgs e)
        {
            string projectDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ProjectDir"].Value;
            if (!Directory.Exists(projectDirectory))
                Directory.CreateDirectory(projectDirectory);

            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = projectDirectory,
                UseShellExecute = true
            });
        }

        internal void ReloadAllObjectManager()
        {
            _objectManagerInitializer.Reload();
        }

        private void Runtime_LogMessageCreated(BaseRuntime runtime, string message, FlowBloxLogLevel logLevel)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BaseRuntime.LogMessageCreatedEventHandler(Runtime_LogMessageCreated), new object[3] { runtime, message, logLevel });
                return;
            }
            _runtimeViewPanel.UserControl.Append(message, logLevel);
        }

        private void Runtime_ProblemTraceCreated(BaseRuntime runtime, ProblemTrace problemTrace)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ProblemTraceCreatedEventHandler(Runtime_ProblemTraceCreated), new object[2] { runtime, problemTrace });
                return;
            }

            _problemsViewPanel.UserControl.Append(problemTrace);
        }

        internal void OnBeforeRuntimeStarted(BaseRuntime runtime)
        {
            runtime.LogMessageCreated += Runtime_LogMessageCreated;

            foreach (var backtraceInterceptor in runtime.Interceptors.OfType<RuntimeBacktraceInterceptor>())
            {
                backtraceInterceptor.ProblemTraceCreated += Runtime_ProblemTraceCreated;
            }

            _objectManagerInitializer.Recreate();

            _runtimeViewPanel.UserControl.InitializeRuntime(runtime);

            UpdateUI();

            _runtimeViewPanel.Focus();
        }

        internal void OnAfterRuntimeFinished()
        {
            UpdateUI();

            _objectManagerInitializer.Recreate();
        }

        private void itmPaste_Click(object sender, EventArgs e)
        {
            if (this._dockContentProjectPanel != null)
                this._dockContentProjectPanel.Paste();
        }

        private void itmCopy_Click(object sender, EventArgs e)
        {
            if (this._dockContentProjectPanel != null)
                this._dockContentProjectPanel.Copy();
        }

        private void itmRegister_Click(object sender, EventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            WindowsFormWPFHelper.ShowDialog(registrationWindow, this);
        }

        private void itmLogin_Click(object sender, EventArgs e)
        {

            var loginWindow = new LoginWindow();
            if (WindowsFormWPFHelper.ShowDialog(loginWindow, this) == true)
            {
                if (loginWindow.Tag != null)
                {
                    var (token, userData) = ((string, FbUserData))loginWindow.Tag;
                    FlowBloxAccountManager.Instance.ActiveUser = userData;
                    FlowBloxAccountManager.Instance.UserToken = token;
                    UpdateUI();
                }
                else
                {
                    FlowBloxAccountManager.Instance.ActiveUser = null;
                    FlowBloxAccountManager.Instance.UserToken = null;
                }
            }
        }

        private void itmLogout_Click(object sender, EventArgs e)
        {
            FlowBloxAccountManager.Instance.ActiveUser = null;
            FlowBloxAccountManager.Instance.UserToken = null;

            UpdateUI();
        }

        private void itmVisitOnline_Click(object sender, EventArgs e) => OpenUrl("https://www.flowblox.net/");

        private void itmGitHub_Click(object sender, EventArgs e) => OpenUrl("https://github.com/mscheitza/flowblox");

        private async void itmCheckForNewVersion_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsRunningPackaged())
                {
                    FlowBloxMessageBox.Show(
                        this,
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_NotPackaged_Message", typeof(FlowBloxMainUITexts)),
                        FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_NotPackaged_Title", typeof(FlowBloxMainUITexts)),
                        FlowBloxMessageBox.Buttons.OK,
                        FlowBloxMessageBox.Icons.Info);
                    return;
                }

                var result = await Package.Current.CheckUpdateAvailabilityAsync();
                switch (result.Availability)
                {
                    case PackageUpdateAvailability.NoUpdates:
                        FlowBloxMessageBox.Show(
                            this,
                            FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_NoUpdates_Message", typeof(FlowBloxMainUITexts)),
                            FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_NoUpdates_Title", typeof(FlowBloxMainUITexts)),
                            FlowBloxMessageBox.Buttons.OK,
                            FlowBloxMessageBox.Icons.Info);
                        return;

                    case PackageUpdateAvailability.Available:
                    case PackageUpdateAvailability.Required:
                        await InstallUpdateNowAsync();
                        return;

                    case PackageUpdateAvailability.Unknown:
                        FlowBloxMessageBox.Show(
                            this,
                            FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_UnknownSource_Message", typeof(FlowBloxMainUITexts)),
                            FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_UnknownSource_Title", typeof(FlowBloxMainUITexts)),
                            FlowBloxMessageBox.Buttons.OK,
                            FlowBloxMessageBox.Icons.Warning);
                        OpenUrl("https://www.flowblox.net/");
                        return;

                    case PackageUpdateAvailability.Error:
                    default:
                        FlowBloxMessageBox.Show(
                            this,
                            FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_Error_Message", typeof(FlowBloxMainUITexts)),
                            FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_Error_Title", typeof(FlowBloxMainUITexts)),
                            FlowBloxMessageBox.Buttons.OK,
                            FlowBloxMessageBox.Icons.Error);
                        return;
                }
            }
            catch (Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);
                FlowBloxMessageBox.Show(
                    this,
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_Exception_Message", typeof(FlowBloxMainUITexts)),
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateCheck_Exception_Title", typeof(FlowBloxMainUITexts)),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Error);
            }
        }

        private static bool IsRunningPackaged()
        {
            try
            {
                var _ = Package.Current;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task InstallUpdateNowAsync()
        {
            var appInstallerUri = Package.Current.GetAppInstallerInfo()?.Uri;
            if (appInstallerUri == null)
                appInstallerUri = new Uri("https://flowblox.net/app/FlowBlox.appinstaller");

            var pm = new PackageManager();

            var options = AddPackageByAppInstallerOptions.ForceTargetAppShutdown;

            try
            {
                // Starts the update immediately; Windows will close the app automatically if necessary.
                var op = pm.AddPackageByAppInstallerFileAsync(appInstallerUri, options, pm.GetDefaultPackageVolume());
                var result = await op.AsTask();

                if (result != null &&
                    result.ErrorText != null &&
                    result.ExtendedErrorCode != null)
                {
                    throw new InvalidOperationException($"An error occurred when attempting to start the update: {result.ErrorText}", result.ExtendedErrorCode);
                }
            }
            catch (Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);

                FlowBloxMessageBox.Show(
                    null,
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateInstall_Failed_Message", typeof(FlowBloxMainUITexts)),
                    FlowBloxResourceUtil.GetLocalizedString("AppWindow_UpdateInstall_Failed_Title", typeof(FlowBloxMainUITexts)),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Error);
            }
        }

        private void itmReportProblem_Click(object sender, EventArgs e) => OpenUrl("https://www.flowblox.net/reportproblem");

        /// <summary>
        /// Opens a URL in the default browser.
        /// </summary>
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                var logger = FlowBloxLogManager.Instance.GetLogger();
                logger.Exception(ex);

                FlowBloxMessageBox.Show
                (
                    this,
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString(
                            "AppWindow_OpenUrlFailed_Message",
                            typeof(FlowBloxMainUITexts)),
                        url),
                    FlowBloxResourceUtil.GetLocalizedString(
                        "AppWindow_OpenUrlFailed_Title",
                        typeof(FlowBloxMainUITexts)),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Warning
                );
            }
        }

        private void itmResetDockablePanels_Click(object sender, EventArgs e)
        {
            FlowBloxOptions.GetOptionInstance().GetOption("MainPanel.DockSettings").Value = string.Empty;
            FlowBloxOptions.GetOptionInstance().Save();
            InitializeDockPanel(true);
        }

        internal T GetAccessibleComponent<T>() where T : System.Windows.Forms.Control
        {
            if (typeof(T) == typeof(ProjectPanel))
                return (T)(object)_dockContentProjectPanel;

            return default(T);
        }
    }
}
