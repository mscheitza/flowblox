using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Components.IO
{
    public enum SftpAuthenticationMethod
    {
        [Display(Name = "SftpAuthenticationMethod_Password", ResourceType = typeof(FlowBloxTexts))]
        Password,

        [Display(Name = "SftpAuthenticationMethod_PrivateKey", ResourceType = typeof(FlowBloxTexts))]
        PrivateKey,

        [Display(Name = "SftpAuthenticationMethod_PasswordAndPrivateKey", ResourceType = typeof(FlowBloxTexts))]
        PasswordAndPrivateKey
    }
}
