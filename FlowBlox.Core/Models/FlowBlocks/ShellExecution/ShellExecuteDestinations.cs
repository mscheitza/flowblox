using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.ShellExecution
{
    public enum ShellExecuteDestinations
    {
        [Display(Name = "Command")]
        Command = 0,

        [Display(Name = "Success")]
        Success = 1,

        [Display(Name = "ExitCode")]
        ExitCode = 2,

        [Display(Name = "StdOut")]
        StandardOutput = 3,

        [Display(Name = "StdErr")]
        StandardError = 4
    }
}
