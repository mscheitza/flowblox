using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Compression
{
    public enum ZipArchiveIteratorDestinations
    {
        [Display(Name = "File")]
        File = 0,

        [Display(Name = "Content")]
        Content = 1
    }
}
