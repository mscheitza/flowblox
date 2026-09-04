using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class RuntimeViewControl : UserControl
    {
        private readonly RuntimeLogAppender _logAppender;

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
            _logAppender = new RuntimeLogAppender(LogTextBox);
        }

        public ICommand ClearCommand
        {
            get => (ICommand)GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }

        private RuntimeViewModel ViewModel => DataContext as RuntimeViewModel;

        public void InitializeRuntime(BaseRuntime runtime) => ViewModel?.InitializeRuntime(runtime);

        public void Append(string message, FlowBloxLogLevel logLevel) => _logAppender.Append(message, logLevel);

        public void ContinueExecutionByUser() => ViewModel?.ContinueExecutionByUser();

        public void PauseExecutionByUser() => ViewModel?.PauseExecutionByUser();

        public void StopExecutionByUser() => ViewModel?.StopExecutionByUser();

        private void Clear() => _logAppender.Clear();

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
