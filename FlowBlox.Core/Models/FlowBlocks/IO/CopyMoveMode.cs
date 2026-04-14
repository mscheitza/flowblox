using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.IO
{
    public enum CopyMoveMode
    {
        [Display(Name = "CopyMoveMode_CopyFile", ResourceType = typeof(FlowBloxTexts))]
        CopyFile,

        [Display(Name = "CopyMoveMode_MoveFile", ResourceType = typeof(FlowBloxTexts))]
        MoveFile,

        [Display(Name = "CopyMoveMode_CopyDirectory", ResourceType = typeof(FlowBloxTexts))]
        CopyDirectory,

        [Display(Name = "CopyMoveMode_MoveDirectory", ResourceType = typeof(FlowBloxTexts))]
        MoveDirectory
    }
}

