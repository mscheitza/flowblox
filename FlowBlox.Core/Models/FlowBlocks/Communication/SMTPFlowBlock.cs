using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace FlowBlox.Core.Models.FlowBlocks.Communication
{
    [Display(Name = "SMTPFlowBlock_DisplayName", Description = "SMTPFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class SMTPFlowBlock : BaseFlowBlock
    {
        [Required]
        [Display(Name = "SMTPFlowBlock_Host", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Host { get; set; }

        [Display(Name = "SMTPFlowBlock_Port", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public int Port { get; set; } = 25;

        [Display(Name = "SMTPFlowBlock_UseSsl", Description = "SMTPFlowBlock_UseSsl_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public bool UseSsl { get; set; }

        [Display(Name = "SMTPFlowBlock_UseAuthentication", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public bool UseAuthentication { get; set; }

        [Display(Name = "SMTPFlowBlock_UserName", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string UserName { get; set; }

        [Display(Name = "SMTPFlowBlock_Password", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox]
        public string Password { get; set; }

        [Required]
        [Display(Name = "SMTPFlowBlock_FromAddress", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string FromAddress { get; set; }

        [Required]
        [Display(Name = "SMTPFlowBlock_ToAddresses", Description = "SMTPFlowBlock_ToAddresses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 7)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string ToAddresses { get; set; }

        [Display(Name = "SMTPFlowBlock_CcAddresses", Description = "SMTPFlowBlock_CcAddresses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 8)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string CcAddresses { get; set; }

        [Display(Name = "SMTPFlowBlock_BccAddresses", Description = "SMTPFlowBlock_BccAddresses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 9)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string BccAddresses { get; set; }

        [Display(Name = "SMTPFlowBlock_Subject", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Subject { get; set; }

        [Display(Name = "SMTPFlowBlock_Body", ResourceType = typeof(FlowBloxTexts), Order = 11)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true, IsCodingMode = true)]
        public string Body { get; set; }

        [Display(Name = "SMTPFlowBlock_IsBodyHtml", ResourceType = typeof(FlowBloxTexts), Order = 12)]
        public bool IsBodyHtml { get; set; }

        [Display(Name = "SMTPFlowBlock_Attachments", ResourceType = typeof(FlowBloxTexts), Order = 13)]
        [FlowBlockUI(Factory = UIFactory.GridView)]
        [FlowBlockDataGrid]
        public ObservableCollection<SmtpAttachmentMappingEntry> Attachments { get; set; } = new();

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.cube_send, 16, SKColors.CadetBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.cube_send, 32, SKColors.CadetBlue);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Communication;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Host));
            properties.Add(nameof(Port));
            properties.Add(nameof(UseSsl));
            properties.Add(nameof(UseAuthentication));
            properties.Add(nameof(FromAddress));
            properties.Add(nameof(ToAddresses));
            properties.Add(nameof(Subject));
            properties.Add(nameof(IsBodyHtml));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var resolvedHost = FlowBloxFieldHelper.ReplaceFieldsInString(Host ?? string.Empty)?.Trim();
                var resolvedFrom = FlowBloxFieldHelper.ReplaceFieldsInString(FromAddress ?? string.Empty)?.Trim();
                var resolvedTo = FlowBloxFieldHelper.ReplaceFieldsInString(ToAddresses ?? string.Empty);
                var resolvedCc = FlowBloxFieldHelper.ReplaceFieldsInString(CcAddresses ?? string.Empty);
                var resolvedBcc = FlowBloxFieldHelper.ReplaceFieldsInString(BccAddresses ?? string.Empty);
                var resolvedSubject = FlowBloxFieldHelper.ReplaceFieldsInString(Subject ?? string.Empty) ?? string.Empty;
                var resolvedBody = FlowBloxFieldHelper.ReplaceFieldsInString(Body ?? string.Empty) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(resolvedHost))
                    throw new ValidationException("SMTP host is empty.");

                if (string.IsNullOrWhiteSpace(resolvedFrom))
                    throw new ValidationException("From address is empty.");

                using var message = new MailMessage
                {
                    From = new MailAddress(resolvedFrom),
                    Subject = resolvedSubject,
                    Body = resolvedBody,
                    IsBodyHtml = IsBodyHtml
                };

                AddAddresses(message.To, resolvedTo, true, "To");
                AddAddresses(message.CC, resolvedCc, false, "Cc");
                AddAddresses(message.Bcc, resolvedBcc, false, "Bcc");

                var disposableAttachments = BuildAttachments();
                try
                {
                    foreach (var attachment in disposableAttachments)
                        message.Attachments.Add(attachment);

                    using var client = new SmtpClient(resolvedHost, Port)
                    {
                        EnableSsl = UseSsl
                    };

                    if (UseAuthentication)
                    {
                        var resolvedUser = FlowBloxFieldHelper.ReplaceFieldsInString(UserName ?? string.Empty);
                        var resolvedPassword = FlowBloxFieldHelper.ReplaceFieldsInString(Password ?? string.Empty);
                        client.Credentials = new NetworkCredential(resolvedUser ?? string.Empty, resolvedPassword ?? string.Empty);
                    }
                    else
                    {
                        client.UseDefaultCredentials = true;
                    }

                    try
                    {
                        client.Send(message);
                        runtime.Report($"SMTP mail sent successfully via '{resolvedHost}:{Port}'.");
                    }
                    catch (Exception ex)
                    {
                        runtime.Report(ex.ToString());
                        CreateNotification(runtime, SMTPNotifications.MailSendFailure);
                    }
                }
                finally
                {
                    foreach (var attachment in disposableAttachments)
                        attachment.Dispose();
                }

                ExecuteNextFlowBlocks(runtime);
            });
        }

        private static void AddAddresses(MailAddressCollection target, string addressesRaw, bool isRequired, string label)
        {
            var entries = (addressesRaw ?? string.Empty)
                .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (isRequired && entries.Count == 0)
                throw new ValidationException($"No {label} addresses configured.");

            foreach (var address in entries)
                target.Add(new MailAddress(address));
        }

        private List<Attachment> BuildAttachments()
        {
            var attachments = new List<Attachment>();
            foreach (var mapping in Attachments ?? Enumerable.Empty<SmtpAttachmentMappingEntry>())
            {
                if (mapping == null || mapping.Field == null)
                    continue;

                var fileName = FlowBloxFieldHelper.ReplaceFieldsInString(mapping.FileName ?? string.Empty)?.Trim();
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                var bytes = ConvertFieldToBytes(mapping);
                var stream = new MemoryStream(bytes, writable: false);
                var attachment = new Attachment(stream, fileName, MediaTypeNames.Application.Octet);
                attachments.Add(attachment);
            }

            return attachments;
        }

        private static byte[] ConvertFieldToBytes(SmtpAttachmentMappingEntry mapping)
        {
            var configuredType = mapping.Field.GetConfiguredType();
            var value = mapping.Field.Value;

            if (configuredType == typeof(byte[]))
                return value as byte[] ?? [];

            if (configuredType == typeof(string))
            {
                var encoding = mapping.EncodingName.ToEncoding();
                return encoding.GetBytes(value?.ToString() ?? string.Empty);
            }

            var fallback = value?.ToString() ?? string.Empty;
            return mapping.EncodingName.ToEncoding().GetBytes(fallback);
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(SMTPNotifications));
                return notificationTypes;
            }
        }

        public enum SMTPNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "Mail send failure")]
            MailSendFailure
        }
    }
}
