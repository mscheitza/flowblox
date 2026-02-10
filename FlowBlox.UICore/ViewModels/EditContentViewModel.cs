using FlowBlox.UICore.Commands;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class EditContentViewModel : INotifyPropertyChanged
    {
        private const string DefaultTextHighlighting = "TEXT";

        private TextDocument _document;
        private string _selectedHighlightingName;
        private ObservableCollection<string> _availableHighlightingNames;
        private readonly Window _window;

        public EditContentViewModel()
        {
            _document = new TextDocument();

            // Determine all highlights dynamically
            var highlightingDefinitionNames = HighlightingManager.Instance.HighlightingDefinitions
                .Select(h => h.Name)
                .OrderBy(n => n)
                .ToList();

            _availableHighlightingNames = [DefaultTextHighlighting, .. highlightingDefinitionNames];

            SelectedHighlightingName = DefaultTextHighlighting;

            ImportTextFromFileCommand = new RelayCommand(_ => ImportTextFromFile());
            ImportBytesFromFileCommand = new RelayCommand(_ => ImportBytesFromFile());
            ApplyCommand = new RelayCommand(_ => Apply());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public EditContentViewModel(Window window) : this()
        {
            _window = window;
        }

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
                if (_selectedHighlightingName != value)
                {
                    _selectedHighlightingName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedHighlightingDefinition));
                }
            }
        }

        public IHighlightingDefinition SelectedHighlightingDefinition
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedHighlightingName))
                    return null;

                if (SelectedHighlightingName == DefaultTextHighlighting)
                    return null;

                return HighlightingManager.Instance.GetDefinition(SelectedHighlightingName);
            }
        }

        public TextDocument Document
        {
            get
            {
                return _document;
            }
        }

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
        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        private void ImportTextFromFile()
        {
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

        private void Apply()
        {
            _window.DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
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
