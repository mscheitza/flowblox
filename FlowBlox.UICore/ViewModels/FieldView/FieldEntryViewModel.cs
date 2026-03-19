using FlowBlox.Core;
using FlowBlox.Core.Models.Components;
using FlowBlox.UICore.Utilities;
using MahApps.Metro.IconPacks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FlowBlox.UICore.ViewModels.FieldView
{
    public sealed class FieldEntryViewModel : INotifyPropertyChanged
    {
        private string _sourceName;
        private string _fieldName;
        private string _fieldValue;
        private bool _isFlowBlockSelected;
        private bool _isAutoSelected;

        public FieldElement FieldElement { get; }

        public string SourceName
        {
            get => _sourceName;
            private set
            {
                if (_sourceName == value)
                    return;

                _sourceName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullyQualifiedName));
            }
        }

        public string FieldName
        {
            get => _fieldName;
            private set
            {
                if (_fieldName == value)
                    return;

                _fieldName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullyQualifiedName));
            }
        }

        public string FieldValue
        {
            get => _fieldValue;
            private set
            {
                if (_fieldValue == value)
                    return;

                _fieldValue = value;
                OnPropertyChanged();
            }
        }

        public string FullyQualifiedName => FieldElement?.FullyQualifiedName ?? string.Empty;

        public bool IsUserField => FieldElement?.UserField == true;

        public PackIconMaterialKind IconKind => IsUserField ? PackIconMaterialKind.Account : PackIconMaterialKind.LinkVariant;

        public ImageSource IconSource { get; }

        public bool IsFlowBlockSelected
        {
            get => _isFlowBlockSelected;
            set
            {
                if (_isFlowBlockSelected == value)
                    return;

                _isFlowBlockSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsAutoSelected
        {
            get => _isAutoSelected;
            set
            {
                if (_isAutoSelected == value)
                    return;

                _isAutoSelected = value;
                OnPropertyChanged();
            }
        }

        public FieldEntryViewModel(FieldElement fieldElement)
        {
            FieldElement = fieldElement;
            _sourceName = fieldElement?.FlowBlockName ?? string.Empty;
            _fieldName = fieldElement?.Name ?? string.Empty;
            _fieldValue = fieldElement?.Pending == true
                ? FlowBloxTexts.FieldElement_PendingValue
                : fieldElement?.StringValue ?? string.Empty;

            if (IsUserField)
            {
                IconSource = WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.Account, 14);
            }
            else
            {
                var icon16 = fieldElement?.Source?.Icon16;
                IconSource = icon16 != null
                    ? SkiaWpfImageHelper.ConvertToImageSource(icon16)
                    : WpfIconHelper.CreateMaterialIcon(PackIconMaterialKind.CubeOutline, 14);
            }
        }

        public void UpdateName()
        {
            SourceName = FieldElement?.FlowBlockName ?? string.Empty;
            FieldName = FieldElement?.Name ?? string.Empty;
        }

        public void UpdateValue(string value, bool pending)
        {
            FieldValue = pending ? FlowBloxTexts.FieldElement_PendingValue : value ?? string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
