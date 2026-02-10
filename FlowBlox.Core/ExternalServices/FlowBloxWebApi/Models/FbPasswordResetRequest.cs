namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbPasswordResetRequest
    {
        public string EmailOrUsername { get; set; }

        public string CaptchaId { get; set; }

        public string CaptchaCode { get; set; }
    }
}
