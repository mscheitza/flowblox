using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    public enum AiExecutionProviders
    {
        [Display(Name = "Default (CPU)")]
        Default,

        [Display(Name = "OpenVINO (Intel GPU/CPU)")]
        OpenVINO,

        [Display(Name = "DirectML (Any Windows GPU)")]
        DirectML,

        [Display(Name = "CUDA (NVIDIA GPU)")]
        CUDA
    }
}