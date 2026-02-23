using FlowBlox.Core.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.ViewModels.FieldSelection
{
    public sealed class ProjectPropertyRowViewModel
    {
        public FlowBloxProjectPropertyElement ProjectPropertyElement { get; }

        public string Key => ProjectPropertyElement?.Key ?? "";
        public string Name => ProjectPropertyElement?.DisplayName ?? "";
        public string Description => ProjectPropertyElement?.Description ?? "";
        public string Value => ProjectPropertyElement?.Value ?? "";

        public ProjectPropertyRowViewModel(FlowBloxProjectPropertyElement element)
        {
            ProjectPropertyElement = element;
        }
    }
}
