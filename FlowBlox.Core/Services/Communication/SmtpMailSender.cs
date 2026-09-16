using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace FlowBlox.Core.Services.Communication
{
    public sealed class SmtpMailSender
    {
        private static readonly object CertificateValidationSync = new();

        public void Send(SmtpMailSettings settings, SmtpMailRequest request)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(settings.Host))
                throw new ArgumentException("SMTP host must not be empty.", nameof(settings));
            if (settings.Port is < 1 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(settings), "SMTP port must be between 1 and 65535.");
            if (string.IsNullOrWhiteSpace(request.FromAddress))
                throw new ArgumentException("From address must not be empty.", nameof(request));

            using var message = new MailMessage
            {
                From = new MailAddress(request.FromAddress.Trim()),
                Subject = request.Subject ?? string.Empty,
                Body = request.Body ?? string.Empty,
                IsBodyHtml = request.IsBodyHtml
            };

            if (!AddAddresses(message.To, request.ToAddresses, required: true))
                throw new ArgumentException("At least one recipient address is required.", nameof(request));

            AddAddresses(message.CC, request.CcAddresses, required: false);
            AddAddresses(message.Bcc, request.BccAddresses, required: false);

            foreach (var item in request.Attachments ?? Array.Empty<SmtpMailAttachment>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.FileName))
                    continue;

                var stream = new MemoryStream(item.Content ?? Array.Empty<byte>(), writable: false);
                message.Attachments.Add(new Attachment(
                    stream,
                    item.FileName,
                    string.IsNullOrWhiteSpace(item.MediaType) ? MediaTypeNames.Application.Octet : item.MediaType));
            }

            using var client = new SmtpClient(settings.Host.Trim(), settings.Port)
            {
                EnableSsl = settings.UseSsl
            };

            if (settings.UseAuthentication)
                client.Credentials = new NetworkCredential(settings.UserName ?? string.Empty, settings.Password ?? string.Empty);
            else
                client.UseDefaultCredentials = true;

            Send(client, message, settings.UseSsl, settings.AcceptInvalidCertificates);
        }

        public static IReadOnlyList<string> ParseAddresses(string addressesRaw)
        {
            return (addressesRaw ?? string.Empty)
                .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool AddAddresses(MailAddressCollection target, string addressesRaw, bool required)
        {
            var entries = ParseAddresses(addressesRaw);
            if (required && entries.Count == 0)
                return false;

            foreach (var address in entries)
                target.Add(new MailAddress(address));

            return true;
        }

        private static void Send(SmtpClient client, MailMessage message, bool useSsl, bool acceptInvalidCertificates)
        {
            if (!(useSsl && acceptInvalidCertificates))
            {
                client.Send(message);
                return;
            }

            lock (CertificateValidationSync)
            {
                var previousValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
                    client.Send(message);
                }
                finally
                {
                    ServicePointManager.ServerCertificateValidationCallback = previousValidationCallback;
                }
            }
        }
    }
}
