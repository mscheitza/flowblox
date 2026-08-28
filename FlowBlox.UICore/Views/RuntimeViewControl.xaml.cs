using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Views
{
    public partial class RuntimeViewControl : UserControl
    {
        private const int TextBoxMaxLength = 30000;

        public static readonly DependencyProperty ClearCommandProperty =
            DependencyProperty.Register(
                nameof(ClearCommand),
                typeof(ICommand),
                typeof(RuntimeViewControl),
                new PropertyMetadata(null));

        public RuntimeViewControl()
        {
            InitializeComponent();
            ClearCommand = new RelayCommand(Clear);
        }

        public ICommand ClearCommand
        {
            get => (ICommand)GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }

        private RuntimeViewModel ViewModel => DataContext as RuntimeViewModel;

        public void InitializeRuntime(BaseRuntime runtime) => ViewModel?.InitializeRuntime(runtime);

        public void Append(string message, FlowBloxLogLevel logLevel)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Append(message, logLevel));
                return;
            }

            EnsureLogParagraph().Inlines.Add(new Run(GetLine(logLevel, message) + Environment.NewLine)
            {
                Foreground = GetForegroundForLogLevel(logLevel)
            });

            TrimLogText();
            LogTextBox.ScrollToEnd();
        }

        public void ContinueExecutionByUser() => ViewModel?.ContinueExecutionByUser();

        public void PauseExecutionByUser() => ViewModel?.PauseExecutionByUser();

        public void StopExecutionByUser() => ViewModel?.StopExecutionByUser();

        private static string GetLine(FlowBloxLogLevel logLevel, string message)
            => string.Join(" ", DateTime.Now, logLevel.ToString(), message);

        private Paragraph EnsureLogParagraph()
        {
            if (LogTextBox.Document.Blocks.FirstBlock is Paragraph paragraph)
                return paragraph;

            LogTextBox.Document.Blocks.Clear();
            paragraph = new Paragraph { Margin = new Thickness(0) };
            LogTextBox.Document.Blocks.Add(paragraph);
            return paragraph;
        }

        private void TrimLogText()
        {
            var textRange = new TextRange(LogTextBox.Document.ContentStart, LogTextBox.Document.ContentEnd);
            var excess = textRange.Text.Length - TextBoxMaxLength;
            if (excess <= 0)
                return;

            var trimEnd = LogTextBox.Document.ContentStart.GetPositionAtOffset(excess, LogicalDirection.Forward);
            if (trimEnd != null)
                new TextRange(LogTextBox.Document.ContentStart, trimEnd).Text = string.Empty;
        }

        private static Brush GetForegroundForLogLevel(FlowBloxLogLevel logLevel)
        {
            return logLevel switch
            {
                FlowBloxLogLevel.Error => Brushes.LightCoral,
                FlowBloxLogLevel.Success => Brushes.LightGreen,
                FlowBloxLogLevel.Warning => Brushes.Yellow,
                _ => Brushes.White
            };
        }

        private void Clear()
        {
            LogTextBox.Document.Blocks.Clear();
            LogTextBox.Document.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
        }

        private void LogTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A)
            {
                LogTextBox.SelectAll();
                e.Handled = true;
            }
        }
    }
}