using FlowBlox.Core;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Enums
{
    public enum FileOpenMode
    {
        [Display(ResourceType = typeof(FlowBloxTexts), Name = "FileOpenMode_WindowsDefaultApp")]
        WindowsDefaultApp,

        [Display(ResourceType = typeof(FlowBloxTexts), Name = "FileOpenMode_FlowBloxEditor")]
        FlowBloxEditor,
    }
}