using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Exceptions
{
    [Serializable]
    public class FieldNullException : Exception
    {
        public FieldElement FieldElement { get; private set; }

        public FieldNullException(string message, FieldElement fieldElement) : base(message)
        {
            this.FieldElement = fieldElement;
        }
    }
}
