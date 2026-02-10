using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.ViewModels.FieldSelection
{ 
    public sealed class FieldRowViewModel
    {
        public FieldElement FieldElement { get; }

        public string SourceName { get; }
        public string FieldName { get; }
        public bool IsUserField { get; }
        public bool IsConnected { get; }
        public string IconKey { get; }

        public FieldRowViewModel(FieldElement element, bool isConnected)
        {
            FieldElement = element;

            SourceName = element?.Source?.Name ?? "";
            FieldName = element?.Name ?? "";
            IsUserField = element?.UserField == true;
            IsConnected = isConnected;

            if (IsUserField)
                IconKey = "user";
            else if (IsConnected)
                IconKey = "connected";
            else
                IconKey = "disconnected";
        }
    }
}