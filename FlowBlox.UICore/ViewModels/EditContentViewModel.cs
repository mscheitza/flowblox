using FlowBlox.UICore.Commands;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class EditContentViewModel : INotifyPropertyChanged
    {
        private const string DefaultTextHighlighting = "TEXT";

        private readonly TextDocument _document;
        private string _selectedHighlightingName;
        private ObservableCollection<string> _availableHighlightingNames;
        private readonly Window _window;
        private bool _isReadOnly;
        private bool _enableFieldSelection;

        public EditContentViewModel()
        {
            _document = new TextDocument();

            var highlightingDefinitionNames = HighlightingManager.Instance.HighlightingDefinitions
                .Select(h => h.Name)
                .OrderBy(n => n)
                .ToList();

            _availableHighlightingNames = [DefaultTextHighlighting, .. highlightingDefinitionNames];
            SelectedHighlightingName = DefaultTextHighlighting;

            ImportTextFromFileCommand = new RelayCommand(_ => ImportTextFromFile());
            ImportBytesFromFileCommand = new RelayCommand(_ => ImportBytesFromFile());
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public EditContentViewModel(Window window) : this()
        {
            _window = window;
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

        public ObservableCollection<string> AvailableHighlightingNames
        {
            get => _availableHighlightingNames;
            set
            {
                _availableHighlightingNames = value;
                OnPropertyChanged();
            }
        }

        public string SelectedHighlightingName
        {
            get => _selectedHighlightingName;
            set
            {
                if (_selectedHighlightingName == value)
                    return;

                _selectedHighlightingName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedHighlightingDefinition));
            }
        }

        public IHighlightingDefinition SelectedHighlightingDefinition
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedHighlightingName) ||
                    SelectedHighlightingName == DefaultTextHighlighting)
                    return null;

                return HighlightingManager.Instance.GetDefinition(SelectedHighlightingName);
            }
        }

        public TextDocument Document => _document;

        public string ContentText
        {
            get => Document?.Text;
            set
            {
                if (Document != null && Document.Text != value)
                {
                    Document.Text = value ?? string.Empty;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Document));
                }
            }
        }

        public ICommand ImportTextFromFileCommand { get; }
        public ICommand ImportBytesFromFileCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void ImportTextFromFile()
        {
            if (IsReadOnly)
                return;

            var ofd = new OpenFileDialog
            {
                Title = "Open File",
                Filter = "All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == true)
            {
                ContentText = File.ReadAllText(ofd.FileName);

                var ext = Path.GetExtension(ofd.FileName)?
                    .TrimStart('.')
                    .ToLowerInvariant();

                var guess = GuessHighlightingFromExtension(ext);
                if (!string.IsNullOrEmpty(guess) && AvailableHighlightingNames.Contains(guess))
                    SelectedHighlightingName = guess;
            }
        }

        private void ImportBytesFromFile()
        {
            if (IsReadOnly)
                return;

            var ofd = new OpenFileDialog
            {
                Title = "Open binary file",
                Filter = "All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == true)
            {
                var bytes = File.ReadAllBytes(ofd.FileName);
                ContentText = Convert.ToBase64String(bytes);
                SelectedHighlightingName = DefaultTextHighlighting;
            }
        }

        private static string GuessHighlightingFromExtension(string ext)
        {
            return ext switch
            {
                "json" => "JSON",
                "xml" => "XML",
                "xaml" => "XML",
                "cs" => "C#",
                "js" => "JavaScript",
                "ts" => "JavaScript",
                "sql" => "SQL",
                "html" => "HTML",
                "htm" => "HTML",
                "yaml" => "XML",
                "yml" => "XML",
                _ => DefaultTextHighlighting
            };
        }

        private void Save()
        {
            if (IsReadOnly || _window == null)
                return;

            _window.DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
            if (_window == null)
                return;

            _window.DialogResult = false;
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
