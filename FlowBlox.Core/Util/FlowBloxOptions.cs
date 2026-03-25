using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Factories;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider.Project;
using System.Xml;

namespace FlowBlox.Core.Util
{
    public class FlowBloxOptions
    {
        private static FlowBloxOptions _instance = null;

        public SortedDictionary<string, OptionElement> OptionCollection { get; } = new SortedDictionary<string, OptionElement>();
       
        private FlowBloxOptions()
        {
            LoadOptions();
            InitDefaults(false);
        }

        private void LoadOptions()
        {
            string globalConfigurationXmlContent = File.Exists(GlobalPaths.GlobalConfigurationXml) ?
                File.ReadAllText(GlobalPaths.GlobalConfigurationXml) : string.Empty;

            XmlDocument xmlDocument = new XmlDocument();
            if (!globalConfigurationXmlContent.Equals(string.Empty))
            {
                xmlDocument.LoadXml(globalConfigurationXmlContent);

                XmlNodeList xnlOptions = xmlDocument.SelectNodes("//option_elements/option_element");

                foreach (XmlNode xnOption in xnlOptions)
                {
                    OptionElement optionElement = OptionElement.FromXml(xnOption);
                    OptionCollection[optionElement.Name] = optionElement;
                }
            }
        }

        public bool HasOption(string optionName)
        {
            if (OptionCollection.ContainsKey(optionName))
                return true;

            return false;
        }

        private string GetDefaultEditorPath()
        {
            string editorPath = string.Empty;

            foreach (string programFiles in new string[] { "%programfiles(x86)%", "%programfiles%" })
            {
                string nppPath = Environment.ExpandEnvironmentVariables(programFiles + @"\Notepad++\notepad++.exe");
                if (File.Exists(Environment.ExpandEnvironmentVariables(nppPath)))
                    editorPath = nppPath;
            }

            editorPath = editorPath.Equals(string.Empty) ? 
                @"C:\Windows\System32\notepad.exe" : 
                editorPath;

            return editorPath;
        }

        public void InitDefaults(bool overwrite)
        {
            var defaultOptions = new List<OptionElement>
            {
                new OptionElement("Account.LoginData", "", "The encrypted login data for the user account. This option stores the login token securely using encryption.", OptionElement.OptionType.Password),
                new OptionElement("Account.LicenseToken", "", "The encrypted license token for the user account. This option stores the license token securely using encryption.", OptionElement.OptionType.Text),
                new OptionElement("Editor.DefaultPath", GetDefaultEditorPath(), "Path to the default editor for directly editing or viewing data from FlowBlox. We recommend the free software Notepad++: https://notepad-plus-plus.org/", OptionElement.OptionType.Text),
                new OptionElement("UI.Style", "Professional", "Set your style here. Available styles by default: \"Default\", \"Professional\". To apply a new style, you must change this setting and restart FlowBlox.", OptionElement.OptionType.Text),
                new OptionElement("UI.Culture", "", "Optional UI culture to force language/formatting, for example 'de-DE' or 'en-US'. Leave empty to use the operating system culture.", OptionElement.OptionType.Text),
                new OptionElement("FieldView.MaxDisplayLength", "3000", "Maximum number of characters displayed per field value in the FieldView UI. Full content remains available when opening/editing the field value.", OptionElement.OptionType.Integer),

                new OptionElement("Paths.ToolboxDir", @"%userprofile%\Documents\FlowBlox\toolbox", "Toolbox directory path.", OptionElement.OptionType.Text),
                new OptionElement("Paths.ToolboxUserFile", @"%userprofile%\Documents\FlowBlox\toolbox\userToolbox.json", "Toolbox user file path.", OptionElement.OptionType.Text),
                new OptionElement("Paths.ProjectDir", @"%userprofile%\Documents\FlowBlox\projects", "Project directory path.", OptionElement.OptionType.Text, isPlaceholderEnabled: true),
                new OptionElement("Paths.InputDir", @"%userprofile%\Documents\FlowBlox\input", "Input directory path.", OptionElement.OptionType.Text, isPlaceholderEnabled: true),
                new OptionElement("Paths.OutputDir", @"%userprofile%\Documents\FlowBlox\output", "Output directory path.", OptionElement.OptionType.Text, isPlaceholderEnabled: true),
                new OptionElement("Paths.ExtensionsDir", @"%localappdata%\FlowBlox\extensions", "Extensions directory path.", OptionElement.OptionType.Text),
                new OptionElement("Paths.ProblemTraceDir", @"%localappdata%\FlowBlox\logs\runtime\problems", "Problem trace directory path.", OptionElement.OptionType.Text),
                new OptionElement("Paths.RuntimeLogDir", @"%localappdata%\FlowBlox\logs\runtime", "Runtime log directory path.", OptionElement.OptionType.Text, isPlaceholderEnabled: true),
                new OptionElement("Paths.ApplicationLogDir", @"%localappdata%\FlowBlox\logs\application", "Application log directory path.", OptionElement.OptionType.Text, isPlaceholderEnabled: true),
                new OptionElement("Paths.DeepCopierProtocolDir", @"%localappdata%\FlowBlox\copy_protocols", "Deep copier protocol directory.", OptionElement.OptionType.Text),
                new OptionElement("Paths.ToolboxCacheDir", @"%localappdata%\FlowBlox\toolbox", "Global toolbox cache directory path.", OptionElement.OptionType.Text),

                new OptionElement("Api.ExtensionServiceBaseUrl", "https://www.flowblox.net/api/", "The URL for the REST API of the extension management system.", OptionElement.OptionType.Text),
                new OptionElement("Api.ProjectServiceBaseUrl", "https://www.flowblox.net/api/", "The URL for the REST API of the project space.", OptionElement.OptionType.Text),

                new OptionElement("Runtime.AutoRestart", "false", "Should the runtime automatically restart once execution is completed?", OptionElement.OptionType.Boolean, "Auto Restart"),
                new OptionElement("Runtime.AutoRestart.CacheMode", "Keep", "Should the cache be cleared or kept in case of an automatic restart? Possible values: \"Keep\" or \"Clear\" Note: This option can also be changed after runtime start.", OptionElement.OptionType.Text),
                new OptionElement("Runtime.StepTimeunit", "0", "With this option, you can set the time unit between two element executions. Helpful for debugging. (specified in milliseconds)", OptionElement.OptionType.Integer),
                new OptionElement("Runtime.StepwiseExecution", "false", "With this option, you can set whether the runtime execution is paused after each element execution. Helpful for debugging.", OptionElement.OptionType.Boolean, "Stepwise Execution"),
                new OptionElement("Runtime.StopOnWarning", "false", "With this option, you can set whether the execution should be paused at every runtime warning. Helpful for debugging.", OptionElement.OptionType.Boolean, "Stop on Warning"),
                new OptionElement("Runtime.StopOnError", "true", "With this option, you can set whether the execution should be paused at every runtime error. Helpful for debugging.", OptionElement.OptionType.Boolean, "Stop on Error"),
                new OptionElement("Runtime.Window.LogAction", "false", "Should the action be logged in the runtime window?", OptionElement.OptionType.Boolean, "Log Action"),
                new OptionElement("Runtime.Window.LogStatus", "true", "Should the status be logged in the runtime window?", OptionElement.OptionType.Boolean, "Log Status"),
                new OptionElement("Runtime.CacheMode", "AutoClear", "With this option, you can set how FlowBlox should behave before re-execution when extraction elements have already processed web pages from a past execution. Possible values: AskUser, AutoClear", OptionElement.OptionType.Text),
                new OptionElement("Runtime.ProblemTracing.MaxFieldValueLength", "200", "With this option you can specify the maximum field value length recorded. Set to a low value for performance/memory reasons.", OptionElement.OptionType.Integer),

                new OptionElement("Grid.DefaultSize", "3000,1000", "When a new project is created, the grid size width, height is set to this value by default.", OptionElement.OptionType.Text),
                new OptionElement("MainPanel.DockSettings", "", "Stores the layout and visibility settings of all dockable panels within the main application window. This includes the default docking positions, size dimensions (width, height), and display states (shown/hidden) of each panel.", OptionElement.OptionType.Text),

                new OptionElement("Modifier.DefaultSeparator", ",", "If a modifier returns multiple result values, these values are separated by this delimiter.", OptionElement.OptionType.Text, isPlaceholderEnabled: true),

                new OptionElement("AI.Onnx.Provider", "Default", "Global ONNX execution provider used by the application. Allowed values: Default (CPU), CUDA, DirectML, OpenVINO. Changing this requires restarting the application.", OptionElement.OptionType.Text)
            };

            foreach(var optionsRegistration in FlowBloxServiceLocator.Instance.GetServices<IOptionsRegistration>())
            {
                optionsRegistration.OptionsInit(defaultOptions, [.. OptionCollection.Values]);
            }

            foreach (var component in
                FlowBloxProjectManager.Instance.ActiveProject?.CreateInstances<IFlowBloxComponent>() ??
                AppDomainInstanceFactory.CreateInstances<IFlowBloxComponent>())
            {
                component.OptionsInit(defaultOptions, [.. OptionCollection.Values]);
            }

            lock (_saveLock)
            {
                foreach (var option in defaultOptions)
                {
                    bool hasOptionValue = false;
                    if (OptionCollection.TryGetValue(option.Name, out OptionElement resolvedOption))
                        hasOptionValue = !string.IsNullOrWhiteSpace(resolvedOption.Value);

                    if (!hasOptionValue || overwrite)
                        OptionCollection[option.Name] = option;
                }
            }
        }

        public List<OptionElement> GetOptions()
        {
            return OptionCollection.Values.ToList();
        }

        private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        private readonly object _saveLock = new object();

        public void Save()
        {
            lock (_saveLock)
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlNode xnOptions = xmlDocument.CreateElement("option_elements");
                var optionElements = OptionCollection.Values.ToList();
                foreach (OptionElement optionElement in optionElements)
                {
                    xnOptions.AppendChild(optionElement.SaveXml(xmlDocument));
                }
                xmlDocument.AppendChild(xnOptions);

                string directory = Path.GetDirectoryName(GlobalPaths.GlobalConfigurationXml);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                try
                {
                    using (FileStream fileStream = new FileStream(GlobalPaths.GlobalConfigurationXml, FileMode.Create, FileAccess.Write))
                    using (XmlWriter writer = XmlWriter.Create(fileStream, _xmlWriterSettings))
                    {
                        xmlDocument.Save(writer);
                    }
                }
                catch (IOException e)
                {
                    FlowBloxLogManager.Instance.GetLogger().Exception(e);
                }
            }
        }

        public static bool HasOptionInstance => _instance != null;

        private static readonly object _lockObject = new object();

        public static FlowBloxOptions GetOptionInstance()
        {
            lock (_lockObject)
            {
                if (_instance == null)
                    _instance = new FlowBloxOptions();
            }

            return _instance;
        }

        public OptionElement GetOption(string name)
        {
            if (_instance.OptionCollection.TryGetValue(name, out var option))
                return option;

            return null;
        }
    }
}
