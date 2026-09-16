using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Notifications;
using FlowBlox.Core.Services.Communication;

namespace FlowBlox.Core.Services.Notifications
{
    public sealed class EmailRuntimeNotificationProvider : IRuntimeNotificationProvider
    {
        private readonly SmtpMailSender _sender = new();

        public string Name => "Email";

        public void Send(RuntimeNotificationConfiguration configuration, RuntimeNotificationContext context)
        {
            var attachments = CreateAttachments(configuration, context);
            _sender.Send(new SmtpMailSettings
            {
                Host = configuration.Host,
                Port = configuration.Port,
                UseSsl = configuration.UseSsl,
                AcceptInvalidCertificates = configuration.AcceptInvalidCertificates,
                UseAuthentication = configuration.UseAuthentication,
                UserName = configuration.UserName,
                Password = configuration.Password
            }, new SmtpMailRequest
            {
                FromAddress = configuration.FromAddress,
                ToAddresses = configuration.ToAddresses,
                CcAddresses = configuration.CcAddresses,
                BccAddresses = configuration.BccAddresses,
                Subject = RuntimeNotificationMailFormatter.CreateSubject(context),
                Body = RuntimeNotificationMailFormatter.CreateHtmlBody(context),
                IsBodyHtml = true,
                Attachments = attachments
            });
        }

        private static IReadOnlyCollection<SmtpMailAttachment> CreateAttachments(
            RuntimeNotificationConfiguration configuration,
            RuntimeNotificationContext context)
        {
            if (!configuration.AttachRuntimeLog ||
                context.Type is not (
                    RuntimeNotificationType.RuntimeError or
                    RuntimeNotificationType.RuntimeAborted or
                    RuntimeNotificationType.RuntimeStartFailed) ||
                string.IsNullOrWhiteSpace(context.RuntimeLogFilePath) ||
                !File.Exists(context.RuntimeLogFilePath))
            {
                return Array.Empty<SmtpMailAttachment>();
            }

            try
            {
                using var stream = new FileStream(
                    context.RuntimeLogFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);

                return
                [
                    new SmtpMailAttachment
                    {
                        FileName = Path.GetFileName(context.RuntimeLogFilePath),
                        MediaType = "text/plain",
                        Content = buffer.ToArray()
                    }
                ];
            }
            catch
            {
                // The notification itself remains useful when a log file is temporarily locked,
                // rotated, or otherwise unavailable.
                return Array.Empty<SmtpMailAttachment>();
            }
        }
    }
}
