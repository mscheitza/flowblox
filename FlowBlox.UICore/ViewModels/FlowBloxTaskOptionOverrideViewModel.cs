using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class FlowBloxTaskOptionOverrideViewModel : INotifyPropertyChanged
    {
        private string _optionKey;
        private string _stringValue;

        public ObservableCollection<string> AvailableOptionKeys { get; } = new();

        public string OptionKey
        {
            get => _optionKey;
            set
            {
                if (_optionKey == value)
                    return;

                _optionKey = value;
                OnPropertyChanged();
            }
        }

        public string StringValue
        {
            get => _stringValue;
            set
            {
                if (_stringValue == value)
                    return;

                _stringValue = value;
                OnPropertyChanged();
            }
        }

        internal void SetAvailableOptionKeys(IEnumerable<string> optionKeys)
        {
            var desiredKeys = optionKeys
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var index = AvailableOptionKeys.Count - 1; index >= 0; index--)
            {
                if (!desiredKeys.Contains(AvailableOptionKeys[index], StringComparer.OrdinalIgnoreCase))
                    AvailableOptionKeys.RemoveAt(index);
            }

            for (var targetIndex = 0; targetIndex < desiredKeys.Count; targetIndex++)
            {
                var optionKey = desiredKeys[targetIndex];
                var currentIndex = AvailableOptionKeys
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => string.Equals(x.value, optionKey, StringComparison.OrdinalIgnoreCase))
                    ?.index ?? -1;

                if (currentIndex < 0)
                    AvailableOptionKeys.Insert(targetIndex, optionKey);
                else if (currentIndex != targetIndex)
                    AvailableOptionKeys.Move(currentIndex, targetIndex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
