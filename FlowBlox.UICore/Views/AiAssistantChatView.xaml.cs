using FlowBlox.UICore.ViewModels;
using FlowBlox.UICore.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FlowBlox.UICore.Views
{
    public partial class AiAssistantChatView : UserControl
    {
        private const double ScrollEndTolerance = 2d;
        private bool _isTranscriptScrolledToBottom = true;
        private ScrollViewer? _transcriptScrollViewer;

        public AiAssistantChatView()
        {
            InitializeComponent();
            PromptTextBox.PreviewKeyDown += PromptTextBox_PreviewKeyDown;
            Loaded += AiAssistantChatView_Loaded;
            DataContextChanged += AiAssistantChatView_DataContextChanged;
            Unloaded += AiAssistantChatView_Unloaded;
        }

        private AiAssistantChatViewModel? SubscribedViewModel { get; set; }

        private void PromptTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not AiAssistantChatViewModel vm)
                return;

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var caretIndex = PromptTextBox.CaretIndex;
                PromptTextBox.Text = PromptTextBox.Text.Insert(caretIndex, Environment.NewLine);
                PromptTextBox.CaretIndex = caretIndex + Environment.NewLine.Length;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (vm.SubmitCommand.CanExecute(null))
                {
                    vm.SubmitCommand.Execute(null);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (vm.StopCommand.CanExecute(null))
                {
                    vm.StopCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void AiAssistantChatView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SubscribeViewModel(DataContext as AiAssistantChatViewModel);
            Dispatcher.BeginInvoke(new Action(SubscribeTranscriptScrollViewer), DispatcherPriority.Loaded);
        }

        private void AiAssistantChatView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeViewModel();
            SubscribeViewModel(e.NewValue as AiAssistantChatViewModel);
        }

        private void SubscribeViewModel(AiAssistantChatViewModel? vm)
        {
            if (vm == null || ReferenceEquals(SubscribedViewModel, vm))
                return;

            SubscribedViewModel = vm;
            vm.Transcript.CollectionChanged += Transcript_CollectionChanged;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void AiAssistantChatView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UnsubscribeTranscriptScrollViewer();
            UnsubscribeViewModel();
        }

        private void Transcript_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isTranscriptScrolledToBottom &&
                (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset))
            {
                ScrollTranscriptToBottom();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isTranscriptScrolledToBottom &&
                string.Equals(e.PropertyName, nameof(AiAssistantChatViewModel.HasCommunicationStatus), StringComparison.Ordinal))
            {
                ScrollTranscriptToBottom();
            }
        }

        private void ScrollTranscriptToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TranscriptListBox.Items.Count <= 0)
                    return;

                TranscriptListBox.ScrollIntoView(TranscriptListBox.Items[^1]);
            }), DispatcherPriority.Background);
        }

        private void SubscribeTranscriptScrollViewer()
        {
            var scrollViewer = VisualTreeHelperExtensions.FindFirstChild<ScrollViewer>(TranscriptListBox);
            if (scrollViewer == null || ReferenceEquals(_transcriptScrollViewer, scrollViewer))
                return;

            UnsubscribeTranscriptScrollViewer();
            _transcriptScrollViewer = scrollViewer;
            _isTranscriptScrolledToBottom = IsScrollViewerAtBottom(scrollViewer);
            scrollViewer.ScrollChanged += TranscriptScrollViewer_ScrollChanged;
        }

        private void UnsubscribeTranscriptScrollViewer()
        {
            if (_transcriptScrollViewer == null)
                return;

            _transcriptScrollViewer.ScrollChanged -= TranscriptScrollViewer_ScrollChanged;
            _transcriptScrollViewer = null;
        }

        private void TranscriptScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
                _isTranscriptScrolledToBottom = IsScrollViewerAtBottom(scrollViewer);
        }

        private static bool IsScrollViewerAtBottom(ScrollViewer scrollViewer)
        {
            return scrollViewer.ScrollableHeight <= 0d ||
                   scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - ScrollEndTolerance;
        }

        private void UnsubscribeViewModel()
        {
            if (SubscribedViewModel == null)
                return;

            SubscribedViewModel.Transcript.CollectionChanged -= Transcript_CollectionChanged;
            SubscribedViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            SubscribedViewModel = null;
        }
    }
}