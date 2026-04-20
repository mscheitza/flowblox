using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Models.Project
{
    public enum FlowBloxInputFileSyncMode
    {
        [Display(Name = "FlowBloxInputFileSyncMode_CreateIfNotExists", ResourceType = typeof(FlowBloxTexts))]
        CreateIfNotExists = 0,

        [Display(Name = "FlowBloxInputFileSyncMode_Overwrite", ResourceType = typeof(FlowBloxTexts))]
        AlwaysOverwrite = 1
    }
}
