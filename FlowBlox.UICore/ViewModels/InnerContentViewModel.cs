using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Formatting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;

namespace FlowBlox.UICore.ViewModels
{

    public class InnerContentViewModel : INotifyPropertyChanged
    {
        public const string DefaultContentType = "TEXT";
        private const string JsonContentType = "JSON";
        private static readonly Regex Base64CharactersRegex = new(
            @"\A[\r\n\t ]*[A-Za-z0-9+/][A-Za-z0-9+/\r\n\t ]*={0,2}[\r\n\t ]*\z",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

        private readonly TextDocument _document = new();
        private string _selectedContentType = DefaultContentType;
        private bool _isReadOnly;
        private bool _enableFieldSelection;

        public InnerContentViewModel()
        {
            _document.TextChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(ContentText));
                OnPropertyChanged(nameof(IsPotentialBase64Content));
                CommandManager.InvalidateRequerySuggested();
            };
            var highlightingNames = HighlightingManager.Instance.HighlightingDefinitions
                .Select(definition => definition.Name)
                .OrderBy(name => name)
                .ToList();
            AvailableContentTypes = [DefaultContentType, .. highlightingNames];

            CopyCommand = new RelayCommand(Copy);
            ImportTextFromFileCommand = new RelayCommand(ImportTextFromFile, () => CanEdit);
            ImportBytesFromFileCommand = new RelayCommand(ImportBytesFromFile, () => CanEdit);
            ExportTextToFileCommand = new RelayCommand(ExportTextToFile);
            ExportBase64ToFileCommand = new RelayCommand(ExportBase64ToFile, () => IsPotentialBase64Content);
            FormatDocumentCommand = new RelayCommand(FormatDocument);
            FormattingProvider = GetFormattingProvider(SelectedContentType);
        }

        public TextDocument Document => _document;
        public ObservableCollection<string> AvailableContentTypes { get; }

        public string ContentText
        {
            get => Document.Text;
            set
            {
                var normalized = value ?? string.Empty;
                if (Document.Text == normalized)
                    return;

                Document.Text = normalized;
                OnPropertyChanged(nameof(Document));
            }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (_isReadOnly == value)
                    return;

                _isReadOnly = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(ShowFieldPlaceholderButton));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool EnableFieldSelection
        {
            get => _enableFieldSelection;
            set
            {
                if (_enableFieldSelection == value)
                    return;

                _enableFieldSelection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowFieldPlaceholderButton));
            }
        }

        public bool CanEdit => !IsReadOnly;
        public bool ShowFieldPlaceholderButton => EnableFieldSelection && CanEdit;
        public bool IsPotentialBase64Content => Base64CharactersRegex.IsMatch(ContentText);

        public string SelectedContentType
        {
            get => _selectedContentType;
            set
            {
                var normalized = string.IsNullOrWhiteSpace(value) ? DefaultContentType : value;
                if (string.Equals(_selectedContentType, normalized, StringComparison.Ordinal))
                    return;

                _selectedContentType = normalized;
                FormattingProvider = GetFormattingProvider(normalized);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedHighlightingDefinition));
                OnPropertyChanged(nameof(FormattingProvider));
                OnPropertyChanged(nameof(CanFormatDocument));
            }
        }

        public IHighlightingDefinition? SelectedHighlightingDefinition =>
            string.Equals(SelectedContentType, DefaultContentType, StringComparison.Ordinal)
                ? null
                : HighlightingManager.Instance.GetDefinition(SelectedContentType);

        public IContentFormattingProvider? FormattingProvider { get; private set; }
        public bool CanFormatDocument => FormattingProvider != null;

        public ICommand CopyCommand { get; }
        public ICommand ImportTextFromFileCommand { get; }
        public ICommand ImportBytesFromFileCommand { get; }
        public ICommand ExportTextToFileCommand { get; }
        public ICommand ExportBase64ToFileCommand { get; }
        public ICommand FormatDocumentCommand { get; }

        private void Copy() => Clipboard.SetText(ContentText);

        private void ImportTextFromFile()
        {
            if (!CanEdit)
                return;

            var dialog = new OpenFileDialog { Title = "Open File", Filter = "All Files (*.*)|*.*" };
            if (dialog.ShowDialog() != true)
                return;

            ContentText = File.ReadAllText(dialog.FileName);
            SelectedContentType = GuessContentTypeFromExtension(Path.GetExtension(dialog.FileName));
        }

        private void ImportBytesFromFile()
        {
            if (!CanEdit)
                return;

            var dialog = new OpenFileDialog { Title = "Open binary file", Filter = "All files (*.*)|*.*" };
            if (dialog.ShowDialog() != true)
                return;

            ContentText = Convert.ToBase64String(File.ReadAllBytes(dialog.FileName));
            SelectedContentType = DefaultContentType;
        }

        private void ExportTextToFile()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save text",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                File.WriteAllText(dialog.FileName, ContentText);
        }

        private void ExportBase64ToFile()
        {
            var buffer = new byte[ContentText.Length];
            if (!Convert.TryFromBase64String(ContentText, buffer, out var bytesWritten))
            {
                ShowInvalidBase64Message();
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save decoded Base64 content",
                Filter = "All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                File.WriteAllBytes(dialog.FileName, buffer.AsSpan(0, bytesWritten));
        }

        private static void ShowInvalidBase64Message() => MessageBox.Show(
            Resources.EditContentView.Error_InvalidBase64,
            Resources.EditContentView.Error_InvalidContent_Title,
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        private void FormatDocument()
        {
            var provider = FormattingProvider;
            if (provider == null)
                return;

            try
            {
                ContentText = provider.Format(ContentText);
            }
            catch (FormatException)
            {
                MessageBox.Show(
                    Resources.EditContentView.Error_DocumentFormatting,
                    Resources.EditContentView.Error_InvalidContent_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private static IContentFormattingProvider? GetFormattingProvider(string? contentType) =>
            ContentFormattingProviderFactory.GetFormattingProvider(contentType);

        private string GuessContentTypeFromExtension(string? extension)
        {
            var contentType = extension?.TrimStart('.').ToLowerInvariant() switch
            {
                "json" => JsonContentType,
                "xml" or "xaml" => "XML",
                "cs" => "C#",
                "js" or "ts" => "JavaScript",
                "sql" => "SQL",
                "html" or "htm" => "HTML",
                "yaml" or "yml" => "XML",
                _ => DefaultContentType
            };
            return AvailableContentTypes.Contains(contentType) 
                ? contentType 
                : DefaultContentType;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
