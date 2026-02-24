using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.Core.Models.Project
{
    [Serializable]
    public class FlowBloxInputFileTemplate : INotifyPropertyChanged
    {
        private string _relativePath;
        private string _contentBase64;

        /// <summary>
        /// Relative path inside the project's input directory.
        /// </summary>
        public string RelativePath
        {
            get => _relativePath;
            set
            {
                if (string.Equals(_relativePath, value, StringComparison.Ordinal))
                    return;

                _relativePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FileName));
            }
        }

        /// <summary>
        /// File content as Base64 string.
        /// </summary>
        public string ContentBase64
        {
            get => _contentBase64;
            set
            {
                if (string.Equals(_contentBase64, value, StringComparison.Ordinal))
                    return;

                _contentBase64 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ContentBytes));
                OnPropertyChanged(nameof(SizeBytes));
            }
        }

        /// <summary>
        /// Cached decoded bytes - not persisted.
        /// Note: The getter decodes on every access to keep the model simple and stateless.
        /// </summary>
        [JsonIgnore]
        public byte[] ContentBytes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ContentBase64))
                    return Array.Empty<byte>();

                try
                {
                    return Convert.FromBase64String(ContentBase64);
                }
                catch
                {
                    return Array.Empty<byte>();
                }
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    ContentBase64 = "";
                    return;
                }

                ContentBase64 = Convert.ToBase64String(value);
            }
        }

        [JsonIgnore]
        public string FileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RelativePath))
                    return "";

                var trimmed = RelativePath.Replace('\\', '/').Trim('/');
                var idx = trimmed.LastIndexOf('/');
                return idx >= 0 ? trimmed[(idx + 1)..] : trimmed;
            }
        }

        [JsonIgnore]
        public long SizeBytes => ContentBytes?.LongLength ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}