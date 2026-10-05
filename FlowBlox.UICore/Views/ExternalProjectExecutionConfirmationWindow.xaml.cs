using System.Diagnostics;
using System.IO;
using System.Windows;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Util.Resources;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class ExternalProjectExecutionConfirmationWindow : MetroWindow
    {
        private readonly string _projectFile;

        public ExternalProjectExecutionConfirmationWindow(string projectFile, string reason)
        {
            InitializeComponent();
            _projectFile = projectFile ?? string.Empty;

            ReasonText.Text = reason;
            ProjectFileText.Text = string.IsNullOrWhiteSpace(_projectFile)
                ? Text("Message_ExternalProjectExecution_ActiveAuxiliaryProject")
                : string.Format(Text("Message_ExternalProjectExecution_ProjectFile"), _projectFile);
            ReviewButton.IsEnabled = File.Exists(_projectFile) && File.Exists(ResolveFlowBloxExecutable());
        }

        private static string Text(string key) =>
            FlowBloxResourceUtil.GetLocalizedString(key, typeof(Resources.AiAssistantChatView));

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_projectFile))
                return;

            var executable = ResolveFlowBloxExecutable();
            if (!File.Exists(executable))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                Arguments = $"--project=\"{_projectFile.Replace("\"", string.Empty)}\"",
                UseShellExecute = true
            });
        }

        private static string ResolveFlowBloxExecutable() =>
            Path.Combine(GlobalPaths.CurrentDirectory, "FlowBlox.exe");

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
