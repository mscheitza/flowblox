namespace FlowBlox.Core.Services.Communication
{
    public sealed class SmtpMailRequest
    {
        public string FromAddress { get; set; } = string.Empty;
        public string ToAddresses { get; set; } = string.Empty;
        public string CcAddresses { get; set; } = string.Empty;
        public string BccAddresses { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; }
        public IReadOnlyCollection<SmtpMailAttachment> Attachments { get; set; } = Array.Empty<SmtpMailAttachment>();
    }

    public sealed class SmtpMailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string MediaType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
}
