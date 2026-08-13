using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.Fields;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class FlowBloxTaskInputParameterViewModel : INotifyPropertyChanged
    {
        private readonly FieldElement _field;
        private string _textValue;
        private int? _integerValue;
        private DateTime? _dateTimeValue;
        private bool _booleanValue;

        public FlowBloxTaskInputParameterViewModel(FieldElement field, string value)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            FieldName = field.Name;
            DisplayName = string.IsNullOrWhiteSpace(field.Name) ? field.FullyQualifiedName : field.Name;

            ApplyValue(value ?? field.StringValue);
        }

        public string FieldName { get; }
        public string DisplayName { get; }
        public FieldTypes FieldType => _field.FieldType?.FieldType ?? FieldTypes.Text;
        public bool IsText => FieldType == FieldTypes.Text || IsUnsupported;
        public bool IsInteger => FieldType == FieldTypes.Integer;
        public bool IsDateTime => FieldType == FieldTypes.DateTime;
        public bool IsBoolean => FieldType == FieldTypes.Boolean;
        public bool IsUnsupported => FieldType is not FieldTypes.Text and not FieldTypes.Integer and not FieldTypes.DateTime and not FieldTypes.Boolean;

        public string TextValue
        {
            get => _textValue;
            set
            {
                if (_textValue == value)
                    return;

                _textValue = value;
                OnPropertyChanged();
            }
        }

        public int? IntegerValue
        {
            get => _integerValue;
            set
            {
                if (_integerValue == value)
                    return;

                _integerValue = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DateTimeValue
        {
            get => _dateTimeValue;
            set
            {
                if (_dateTimeValue == value)
                    return;

                _dateTimeValue = value;
                OnPropertyChanged();
            }
        }

        public bool BooleanValue
        {
            get => _booleanValue;
            set
            {
                if (_booleanValue == value)
                    return;

                _booleanValue = value;
                OnPropertyChanged();
            }
        }

        public string ToRunnerValue()
        {
            return FieldType switch
            {
                FieldTypes.Integer => FieldResultFormatter.FormatResult(_field, IntegerValue),
                FieldTypes.DateTime => FieldResultFormatter.FormatResult(_field, DateTimeValue),
                FieldTypes.Boolean => FieldResultFormatter.FormatResult(_field, BooleanValue),
                _ => TextValue ?? string.Empty
            };
        }

        private void ApplyValue(string value)
        {
            TextValue = value ?? string.Empty;

            if (FieldType == FieldTypes.Integer &&
                int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
            {
                IntegerValue = intValue;
            }

            if (FieldType == FieldTypes.DateTime)
            {
                var format = _field.FieldType?.DateFormat;
                if (!string.IsNullOrWhiteSpace(format) &&
                    DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactValue))
                {
                    DateTimeValue = exactValue;
                }
                else if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var cultureValue))
                {
                    DateTimeValue = cultureValue;
                }
            }

            if (FieldType == FieldTypes.Boolean &&
                bool.TryParse(value, out var boolValue))
            {
                BooleanValue = boolValue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
