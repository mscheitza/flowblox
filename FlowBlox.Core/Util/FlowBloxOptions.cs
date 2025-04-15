using FlowBlox.Core.Constants;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Factories;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Save();
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
            {
                return true;
            }

            return false;
        }

        private string GetDefaultEditorPath()
        {
            string editorPath = string.Empty;

            foreach (string programFiles in new string[] { "%programfiles(x86)%", "%programfiles%" })
            {
                string nppPath = Environment.ExpandEnvironmentVariables(programFiles + @"\Notepad++\notepad++.exe");
                if (File.Exists(Environment.ExpandEnvironmentVariables(nppPath)))
                {
                    editorPath = nppPath;
                }
            }

            editorPath = editorPath.Equals(string.Empty) ? @"C:\Windows\System32\notepad.exe" : editorPath;

            return editorPath;
        }

        public void InitDefaults(bool overwrite)
        {
            List<OptionElement> defaultOptions = new List<OptionElement>
            {
                new OptionElement("Account.LoginData", "", "The encrypted login data for the user account. This option stores the login token securely using encryption.", OptionElement.OptionType.Password),
                new OptionElement("Account.LicenseToken", "", "The encrypted license token for the user account. This option stores the license token securely using encryption.", OptionElement.OptionType.Text),
                new OptionElement("General.EditorPath", GetDefaultEditorPath(), "Path to the default editor for directly editing or viewing data from FlowBlox. We recommend the free software Notepad++: https://notepad-plus-plus.org/", OptionElement.OptionType.Text),
                new OptionElement("General.Style", "Professional", "Set your style here. Available styles by default: \"Default\", \"Professional\". To apply a new style, you must change this setting and restart FlowBlox.", OptionElement.OptionType.Text),
                
                new OptionElement("General.ToolboxDir", @"%userprofile%\Documents\FlowBlox\toolbox", "The Toolbox directory is located by default in the local application data directory, but can be changed here.", OptionElement.OptionType.Text),
                new OptionElement("General.ToolboxUserFile",@"%userprofile%\Documents\FlowBlox\toolbox\userToolbox.json","The Toolbox user file is located by default in the local application data directory within the toolbox folder, but can be changed here. This file contains user-specific toolbox elements.",OptionElement.OptionType.Text),
                new OptionElement("General.ProjectDir", @"%userprofile%\Documents\FlowBlox\projects", "The Project directory is by default located in your Documents folder, but can be changed here. This location is more accessible and suitable for files that users might frequently manage.", OptionElement.OptionType.Text),
                new OptionElement("General.OutputDir", @"%userprofile%\Documents\FlowBlox\output", "The Output directory is by default located in your Documents folder. This location is selected as the start directory in the \"Save File\" dialog, for example, for table output. This makes it easily accessible and suitable for managing output files.", OptionElement.OptionType.Text),
                new OptionElement("General.ExtensionsDir", @"%localappdata%\FlowBlox\extensions", "The Extension directory is by default located in the local application data directory, but can be changed here. This location keeps extension files secure and separate from user documents.", OptionElement.OptionType.Text),
                new OptionElement("General.ProblemTraceDir", @"%localappdata%\FlowBlox\logs\runtime\problems", "The Problem Trace Directory is by default located in the local application data directory, but can be changed here. This location keeps traces captured by the runtime.", OptionElement.OptionType.Text),
                new OptionElement("General.RuntimeLogDir", @"%localappdata%\FlowBlox\logs\runtime", "The Log Directory is by default located in the local application data directory, but can be changed here. This location keeps logs created by the runtime.", OptionElement.OptionType.Text),
                new OptionElement("General.DeepCopierProtocolDir", @"%localappdata%\FlowBlox\copy_protocols", "The Deep-Copier Protocol Directory is by default located in the local application data directory, but can be changed here. This location keeps protocols created by the internal deep copier.", OptionElement.OptionType.Text),

                new OptionElement("General.ExtensionApiServiceBaseUrl", "https://www.flowblox.net/api/", "The URL for the REST API of the extension management system.", OptionElement.OptionType.Text),

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

                new OptionElement("Modifier.DefaultSeparator", ",", "If a modifier returns multiple result values, these values are separated by this delimiter.", OptionElement.OptionType.Text),

                new OptionElement("PropertyView.UIFramework", "WPF", "Defines which UI framework should be used for rendering property views. Valid values are 'WPF' and 'WinForms'.", OptionElement.OptionType.Text)

            };

            foreach(var optionsRegistration in FlowBloxServiceLocator.Instance.GetServices<IOptionsRegistration>())
            {
                optionsRegistration.OptionsInit(defaultOptions);
            }

            foreach (var component in
                FlowBloxProjectManager.Instance.ActiveProject?.CreateInstances<IFlowBloxComponent>() ??
                AppDomainInstanceFactory.CreateInstances<IFlowBloxComponent>())
            {
                component.OptionsInit(defaultOptions);
            }

            foreach (var option in defaultOptions)
            {
                if (!OptionCollection.ContainsKey(option.Name) || overwrite)
                    OptionCollection[option.Name] = option;
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
                foreach (OptionElement optionElement in OptionCollection.Values)
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
