using FlowBlox.Grid.Elements.Util;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public class MultilineTextBoxFactory
    {
        private readonly PropertyInfo _property;
        private readonly object _target;
        private readonly bool _readOnly;

        public MultilineTextBoxFactory(PropertyInfo property, object target, bool readOnly)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _readOnly = readOnly;
        }

        public FrameworkElement Create()
        {
            var binding = new Binding(_property.Name)
            {
                Source = _target,
                Mode = _property.CanWrite ? BindingMode.TwoWay : BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true,
                NotifyOnValidationError = true
            };

            var textBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsReadOnly = _readOnly
            };
            textBox.TextChanged += (s, e) => FlowBloxComponentHelper.RaisePropertyChanged(_target, _property.Name);
            textBox.SetBinding(TextBox.TextProperty, binding);

            return ResizableControlContainer.Create(textBox);
        }
    }
}
