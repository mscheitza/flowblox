using FlowBlox.Core;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.UICore.Utilities;
using log4net.Filter;
using OfficeOpenXml.Export.HtmlExport;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlowBlox.Views
{
    public partial class RuntimeView : UserControl
    {
        private const int TextBoxMaxLength = 30000;

        private BaseRuntime runtime = null;

        private bool initialized = false;

        private string GetLine(FlowBloxLogLevel logLevel, string message)
        {
            return string.Join(" ", DateTime.Now, logLevel.ToString(), message);
        }

        public void AppendMessage(RichTextBox richTextBox, FlowBloxLogLevel logLevel, string message)
        {
            Color logColor = GetForeColorForLogLevel(logLevel);
            string logLine = GetLine(logLevel, message) + Environment.NewLine;

            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => AppendInternal(richTextBox, logLine, logColor)));
            }
            else
            {
                AppendInternal(richTextBox, logLine, logColor);
            }
        }

        private void AppendInternal(RichTextBox richTextBox, string logLine, Color logColor)
        {
            richTextBox.BeginUpdate();
            try
            {
                // Append with color
                int start = richTextBox.TextLength;
                richTextBox.SelectionStart = start;
                richTextBox.SelectionLength = 0;
                richTextBox.SelectionColor = logColor;
                richTextBox.AppendText(logLine);
                richTextBox.SelectionColor = richTextBox.ForeColor;

                // Trim from the beginning while preserving formatting
                if (richTextBox.TextLength > TextBoxMaxLength)
                {
                    int excess = richTextBox.TextLength - TextBoxMaxLength;
                    richTextBox.SelectionStart = 0;
                    richTextBox.SelectionLength = excess;
                    richTextBox.SelectedText = string.Empty;
                }

                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.ScrollToCaret();
            }
            finally
            {
                richTextBox.EndUpdate();
            }
        }

        private Color GetForeColorForLogLevel(FlowBloxLogLevel logLevel)
        {
            switch (logLevel)
            {
                case FlowBloxLogLevel.Error:
                    return Color.LightCoral;
                case FlowBloxLogLevel.Success:
                    return Color.LightGreen;
                case FlowBloxLogLevel.Warning:
                    return Color.Yellow;
                default:
                    return Color.White;
            }
        }

        public void InitializeRuntimeSettings()
        {
            this.initialized = false;

            bool stepwiseExecution;
            bool stopOnWarning;
            bool stopOnError;

            bool.TryParse(FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StepwiseExecution"].Value, out stepwiseExecution);
            bool.TryParse(FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StopOnWarning"].Value, out stopOnWarning);
            bool.TryParse(FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StopOnError"].Value, out stopOnError);

            this.cbStepwiseExecution.Checked = stepwiseExecution;
            this.cbStopOnWarning.Checked = stopOnWarning;
            this.cbStopOnError.Checked = stopOnError;

            this.initialized = true;
        }

        public void InitializeRuntime(BaseRuntime runtime)
        {
            this.InitializeRuntimeSettings();
            this.runtime = runtime;
            this.runtime.Finish += new BaseRuntime.FinishedEventHandler(Runtime_Finish);
            this.UpdateUI();
        }

        public RuntimeView()
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);
            Initialize();
        }

        internal new bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A) && richtTextBox.Focused)
            {
                richtTextBox.SelectAll();
                return true;
            }

            if (keyData == (Keys.Control | Keys.C) && richtTextBox.Focused)
            {
                richtTextBox.Copy();
                return true;
            }

            return false;
        }

        private void Initialize()
        {
            ControlHelper.EnableDoubleBuffer(toolStrip);
            ControlHelper.EnableOptimizedDoubleBuffer(toolStrip);
            ControlHelper.EnableDoubleBuffer(richtTextBox);
            ControlHelper.EnableOptimizedDoubleBuffer(richtTextBox);
            this.richtTextBox.Tag = FlowBloxStyleTags.StyleIgnore;
            UpdateUI();
        }

        void Runtime_Finish(object Result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BaseRuntime.FinishedEventHandler(Runtime_Finish), new object[1] { Result });
                return;
            }
            UpdateUI();
        }

        private bool TryAppend(string message, FlowBloxLogLevel logLevel)
        {
            try
            {
                AppendMessage(richtTextBox, logLevel, message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Append(string message, FlowBloxLogLevel logLevel) => TryAppend(message, logLevel);

        public void UpdateUI()
        {
            btPause.Enabled = runtime?.Running == true && !runtime.Pause;
            btContinue.Enabled = runtime?.Running == true && runtime.Pause;
            btStopExecution.Enabled = runtime?.Running == true && !runtime.Aborted;
            btOpenLogfile.Enabled = runtime?.Running == true && !runtime.Aborted;
        }

        private void btPause_Click(object sender, EventArgs e)
        {
            runtime.Pause = true;
            UpdateUI();
        }

        private void btAbort_Click(object sender, EventArgs e)
        {
            runtime.Aborted = true;
            UpdateUI();
        }

        private void btContinue_Click(object sender, EventArgs e)
        {
            runtime.Pause = false;
            UpdateUI();
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            richtTextBox.Text = string.Empty;
        }

        private void btShowLogfile_Click(object sender, EventArgs e)
        {
            if (runtime is not FlowBloxRuntime fBRuntime)
                return;

            string logfilePath = fBRuntime.GetLogfilePath();

            if (File.Exists(logfilePath))
                FlowBloxEditingHelper.OpenUsingEditor(logfilePath);
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            UpdateUI();
        }

        public void ContinueExecutionByUser()
        {
            if (btContinue.Enabled) btContinue_Click(null, null);
        }

        public void PauseExecutionByUser()
        {
            if (btStopExecution.Enabled) btPause_Click(null, null);
        }

        public void StopExecutionByUser()
        {
            if (btStopExecution.Enabled) btAbort_Click(null, null);
        }

        private void cbDebugMode_CheckedChanged(object sender, EventArgs e)
        {
            if (initialized)
            {
                bool value = cbStepwiseExecution.Checked;
                if (runtime != null)
                {
                    runtime.StepwiseExecution = value;
                }
                FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StepwiseExecution"].Value = value.ToString().ToLower();
                FlowBloxOptions.GetOptionInstance().Save();
            }
        }

        private void itmExportDir_Click(object sender, EventArgs e)
        {
            string OutputDirectory = FlowBloxOptions.GetOptionInstance().OptionCollection["General.OutputDir"].Value;
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            Process.Start("explorer.exe", $"\"{OutputDirectory}\"");
        }

        private void cbStopOnWarning_CheckedChanged(object sender, EventArgs e)
        {
            if (initialized)
            {
                bool Value = cbStopOnWarning.Checked;
                if (runtime != null)
                    runtime.StopOnWarning = Value;
                FlowBloxOptions.GetOptionInstance().OptionCollection["Runtime.StopOnWarning"].Value = Value.ToString().ToLower();
                FlowBloxOptions.GetOptionInstance().Save();
            }
        }
    }
}
