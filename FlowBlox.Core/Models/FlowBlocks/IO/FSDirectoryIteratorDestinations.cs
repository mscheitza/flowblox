using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.IO
{
    public enum FSDirectoryIteratorDestinations
    {
        [Display(Name = "FSDirectoryIteratorDestinations_FullPath", ResourceType = typeof(FlowBloxTexts))]
        FullPath = 0,

        [Display(Name = "FSDirectoryIteratorDestinations_RelativePath", ResourceType = typeof(FlowBloxTexts))]
        RelativePath = 1,

        [Display(Name = "FSDirectoryIteratorDestinations_FileName", ResourceType = typeof(FlowBloxTexts))]
        FileName = 2,

        [Display(Name = "FSDirectoryIteratorDestinations_Size", ResourceType = typeof(FlowBloxTexts))]
        Size = 3,

        [Display(Name = "FSDirectoryIteratorDestinations_LastModified", ResourceType = typeof(FlowBloxTexts))]
        LastModified = 4,

        [Display(Name = "FSDirectoryIteratorDestinations_CreationDate", ResourceType = typeof(FlowBloxTexts))]
        CreationDate = 5
    }
}
