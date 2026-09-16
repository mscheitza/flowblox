namespace FlowBlox.Core.Services.Communication
{
    public sealed class SmtpMailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 25;
        public bool UseSsl { get; set; }
        public bool AcceptInvalidCertificates { get; set; }
        public bool UseAuthentication { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
