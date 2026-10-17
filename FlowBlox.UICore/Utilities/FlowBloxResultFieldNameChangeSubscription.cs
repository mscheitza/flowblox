using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.ComponentModel;

namespace FlowBlox.UICore.Utilities
{
    internal sealed class FlowBloxResultFieldNameChangeSubscription : IDisposable
    {
        private readonly BaseResultFlowBlock _resultFlowBlock;
        private readonly Action<FieldElement> _fieldNameChanged;
        private readonly HashSet<FieldElement> _subscribedFields = new();
        private bool _disposed;

        public FlowBloxResultFieldNameChangeSubscription(
            BaseResultFlowBlock resultFlowBlock,
            Action<FieldElement> fieldNameChanged)
        {
            _resultFlowBlock = resultFlowBlock ?? throw new ArgumentNullException(nameof(resultFlowBlock));
            _fieldNameChanged = fieldNameChanged ?? throw new ArgumentNullException(nameof(fieldNameChanged));

            Rebind();
        }

        public void Rebind()
        {
            if (_disposed)
                return;

            var currentFields = _resultFlowBlock.Fields
                .Where(field => field != null)
                .ToHashSet();

            foreach (var field in _subscribedFields.Except(currentFields).ToList())
                Unsubscribe(field);

            foreach (var field in currentFields)
                Subscribe(field);
        }

        private void Subscribe(FieldElement field)
        {
            if (!_subscribedFields.Add(field))
                return;

            field.PropertyChanged += Field_PropertyChanged;
        }

        private void Unsubscribe(FieldElement field)
        {
            field.PropertyChanged -= Field_PropertyChanged;
            _subscribedFields.Remove(field);
        }

        private void Field_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_disposed &&
                e.PropertyName == nameof(FieldElement.Name) &&
                sender is FieldElement field)
            {
                _fieldNameChanged(field);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var field in _subscribedFields.ToList())
                Unsubscribe(field);
        }
    }
}
