using FlowBlox.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public sealed class FieldSelectionDialogContext
    {
        public FlowBlockUIAttribute UiAttribute { get; init; }

        public FieldSelectionAttribute FieldSelectionAttribute { get; init; }
    }
}
