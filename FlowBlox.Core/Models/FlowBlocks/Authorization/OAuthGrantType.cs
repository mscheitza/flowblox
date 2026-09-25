using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Authorization
{
    public enum OAuthGrantType
    {
        [Display(Name = "OAuthGrantType_ClientCredentials", ResourceType = typeof(FlowBloxTexts))]
        ClientCredentials,

        [Display(Name = "OAuthGrantType_Password", ResourceType = typeof(FlowBloxTexts))]
        Password
    }
}
