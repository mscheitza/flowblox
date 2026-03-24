using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Compression
{
    public enum ZipCompressionStrength
    {
        [Display(Name = "ZipCompressionStrength_None", ResourceType = typeof(FlowBloxTexts))]
        None = 0,

        [Display(Name = "ZipCompressionStrength_Low", ResourceType = typeof(FlowBloxTexts))]
        Low = 1,

        [Display(Name = "ZipCompressionStrength_Medium", ResourceType = typeof(FlowBloxTexts))]
        Medium = 2,

        [Display(Name = "ZipCompressionStrength_High", ResourceType = typeof(FlowBloxTexts))]
        High = 3,

        [Display(Name = "ZipCompressionStrength_VeryHigh", ResourceType = typeof(FlowBloxTexts))]
        VeryHigh = 4
    }
}
