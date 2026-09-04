using FlowBlox.Core.Enums;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace FlowBlox.UICore.Views
{
    internal sealed class RuntimeLogAppender
    {
        private const int TextBoxMaxLength = 30000;
        private const int LogFlushIntervalMilliseconds = 100;

        private readonly RichTextBox _logTextBox;
        private readonly object _sync = new();
        private readonly Queue<RuntimeLogEntry> _pendingLogEntries = new();
        private readonly DispatcherTimer _flushTimer;
        private bool _flushScheduled;
        private int _textLength;

        public RuntimeLogAppender(RichTextBox logTextBox)
        {
            _logTextBox = logTextBox ?? throw new ArgumentNullException(nameof(logTextBox));
            _flushTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(LogFlushIntervalMilliseconds),
                DispatcherPriority.Background,
                (_, _) => FlushPendingLogEntries(),
                _logTextBox.Dispatcher);
            _flushTimer.Stop();
        }

        public void Append(string message, FlowBloxLogLevel logLevel)
        {
            var entry = new RuntimeLogEntry(GetLine(logLevel, message), logLevel);
            var scheduleFlush = false;

            lock (_sync)
            {
                _pendingLogEntries.Enqueue(entry);

                if (!_flushScheduled)
                {
                    _flushScheduled = true;
                    scheduleFlush = true;
                }
            }

            if (!scheduleFlush)
                return;

            if (!_logTextBox.Dispatcher.CheckAccess())
            {
                _logTextBox.Dispatcher.BeginInvoke(StartFlushTimer, DispatcherPriority.Background);
                return;
            }

            StartFlushTimer();
        }

        public void Clear()
        {
            lock (_sync)
            {
                _pendingLogEntries.Clear();
                _flushScheduled = false;
            }

            _flushTimer.Stop();
            ClearLogText();
        }

        private static string GetLine(FlowBloxLogLevel logLevel, string message)
            => string.Join(" ", DateTime.Now, logLevel.ToString(), message);

        private void StartFlushTimer()
        {
            if (_flushTimer.IsEnabled)
                return;

            _flushTimer.Start();
        }

        private void FlushPendingLogEntries()
        {
            if (!_logTextBox.Dispatcher.CheckAccess())
            {
                _logTextBox.Dispatcher.BeginInvoke(FlushPendingLogEntries, DispatcherPriority.Background);
                return;
            }

            _flushTimer.Stop();

            List<RuntimeLogEntry> entries;
            lock (_sync)
            {
                if (_pendingLogEntries.Count == 0)
                {
                    _flushScheduled = false;
                    return;
                }

                entries = new List<RuntimeLogEntry>(_pendingLogEntries.Count);
                while (_pendingLogEntries.Count > 0)
                    entries.Add(_pendingLogEntries.Dequeue());
            }

            _logTextBox.BeginChange();
            try
            {
                AppendLogEntries(entries);
                TrimLogText();
            }
            finally
            {
                _logTextBox.EndChange();
            }

            _logTextBox.ScrollToEnd();

            lock (_sync)
            {
                if (_pendingLogEntries.Count == 0)
                {
                    _flushScheduled = false;
                    return;
                }
            }

            _flushTimer.Start();
        }

        private void AppendLogEntries(IReadOnlyList<RuntimeLogEntry> entries)
        {
            var paragraph = EnsureLogParagraph();
            var builder = new StringBuilder();
            FlowBloxLogLevel? currentLogLevel = null;

            foreach (var entry in entries)
            {
                if (currentLogLevel != null && currentLogLevel != entry.LogLevel)
                {
                    AppendRun(paragraph, builder, currentLogLevel.Value);
                    builder.Clear();
                }

                currentLogLevel = entry.LogLevel;
                builder.AppendLine(entry.Line);
                _textLength += entry.Line.Length + Environment.NewLine.Length;
            }

            if (currentLogLevel != null && builder.Length > 0)
                AppendRun(paragraph, builder, currentLogLevel.Value);
        }

        private Paragraph EnsureLogParagraph()
        {
            if (_logTextBox.Document.Blocks.FirstBlock is Paragraph paragraph)
                return paragraph;

            _logTextBox.Document.Blocks.Clear();
            paragraph = new Paragraph { Margin = new Thickness(0) };
            _logTextBox.Document.Blocks.Add(paragraph);
            return paragraph;
        }

        private void TrimLogText()
        {
            var excess = _textLength - TextBoxMaxLength;
            if (excess <= 0)
                return;

            var trimEnd = _logTextBox.Document.ContentStart.GetPositionAtOffset(excess, LogicalDirection.Forward);
            if (trimEnd == null)
            {
                ClearLogText();
                return;
            }

            new TextRange(_logTextBox.Document.ContentStart, trimEnd).Text = string.Empty;
            _textLength = TextBoxMaxLength;
        }

        private void ClearLogText()
        {
            _logTextBox.Document.Blocks.Clear();
            _logTextBox.Document.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
            _textLength = 0;
        }

        private static void AppendRun(Paragraph paragraph, StringBuilder builder, FlowBloxLogLevel logLevel)
        {
            paragraph.Inlines.Add(new Run(builder.ToString())
            {
                Foreground = GetForegroundForLogLevel(logLevel)
            });
        }

        private static Brush GetForegroundForLogLevel(FlowBloxLogLevel logLevel)
            => logLevel switch
            {
                FlowBloxLogLevel.Error => Brushes.LightCoral,
                FlowBloxLogLevel.Success => Brushes.LightGreen,
                FlowBloxLogLevel.Warning => Brushes.Yellow,
                _ => Brushes.White
            };

        private readonly record struct RuntimeLogEntry(string Line, FlowBloxLogLevel LogLevel);
    }
}