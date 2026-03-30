using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Models.Project
{
    public enum FlowBloxInputTemplateSyncMode
    {
        [Display(Name = "FlowBloxInputTemplateSyncMode_CreateIfNotExists", ResourceType = typeof(FlowBloxTexts))]
        CreateIfNotExists = 0,

        [Display(Name = "FlowBloxInputTemplateSyncMode_Overwrite", ResourceType = typeof(FlowBloxTexts))]
        AlwaysOverwrite = 1
    }
}
