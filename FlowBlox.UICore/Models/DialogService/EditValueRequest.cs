using FlowBlox.UICore.Enums;

namespace FlowBlox.UICore.Models.DialogService
{
    public class EditValueRequest
    {
        public string Value { get; set; }

        public EditMode EditMode { get; set; }

        public string ParameterName { get; set; }

        public bool IsRegex { get; set; }

        public bool IsMultiline { get; set; }
    }
}
