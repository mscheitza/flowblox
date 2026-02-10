using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Enums
{
    public enum OnnxNotifications
    {
        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Model path is empty or file does not exist.")]
        ModelFileMissing,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Tokenizer instance is null.")]
        TokenizerMissing,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Failed to execute prompt using ONNX model.")]
        PromptExecutionFailed,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "General execution error in the ONNX AI flow block.")]
        ExecutionFailed,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Could not initialize AI tokenizer.")]
        AITokenizerInitializationFailed
    }
}