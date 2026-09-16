using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Services.Communication;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace FlowBlox.Core.Models.FlowBlocks.Communication
{
    [Display(Name = "SMTPFlowBlock_DisplayName", Description = "SMTPFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class SMTPFlowBlock : BaseSingleResultFlowBlock
    {
        public override FieldTypes DefaultResultFieldType => FieldTypes.Boolean;
        public override string DefaultResultFieldName => GlobalConstants.SuccessFieldName;

        [Required]
        [Display(Name = "SMTPFlowBlock_Host", Description = "SMTPFlowBlock_Host_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Host { get; set; }

        [Display(Name = "SMTPFlowBlock_Port", Description = "SMTPFlowBlock_Port_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public int Port { get; set; } = 25;
        public FieldElement Port_SelectedField { get; set; }

        [Display(Name = "SMTPFlowBlock_UseSsl", Description = "SMTPFlowBlock_UseSsl_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public bool UseSsl { get; set; }
        public FieldElement UseSsl_SelectedField { get; set; }

        [Display(Name = "SMTPFlowBlock_AcceptInvalidCertificates", Description = "SMTPFlowBlock_AcceptInvalidCertificates_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public bool AcceptInvalidCertificates { get; set; } = true;

        [Display(Name = "SMTPFlowBlock_UseAuthentication", Description = "SMTPFlowBlock_UseAuthentication_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public bool UseAuthentication { get; set; }

        [Display(Name = "SMTPFlowBlock_UserName", Description = "SMTPFlowBlock_UserName_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string UserName { get; set; }

        [Display(Name = "SMTPFlowBlock_Password", Description = "SMTPFlowBlock_Password_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox]
        public string Password { get; set; }

        [Required]
        [Display(Name = "SMTPFlowBlock_FromAddress", Description = "SMTPFlowBlock_FromAddress_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string FromAddress { get; set; }

        [Required]
        [Display(Name = "SMTPFlowBlock_ToAddresses", Description = "SMTPFlowBlock_ToAddresses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 7)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string ToAddresses { get; set; }

        [Display(Name = "SMTPFlowBlock_CcAddresses", Description = "SMTPFlowBlock_CcAddresses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 8)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string CcAddresses { get; set; }

        [Display(Name = "SMTPFlowBlock_BccAddresses", Description = "SMTPFlowBlock_BccAddresses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 9)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string BccAddresses { get; set; }

        [Display(Name = "SMTPFlowBlock_Subject", Description = "SMTPFlowBlock_Subject_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Subject { get; set; }

        [Display(Name = "SMTPFlowBlock_Body", Description = "SMTPFlowBlock_Body_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 11)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(MultiLine = true, IsCodingMode = true)]
        public string Body { get; set; }

        [Display(Name = "SMTPFlowBlock_IsBodyHtml", Description = "SMTPFlowBlock_IsBodyHtml_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 12)]
        public bool IsBodyHtml { get; set; }

        [Display(Name = "SMTPFlowBlock_Attachments", Description = "SMTPFlowBlock_Attachments_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 13)]
        [FlowBloxUI(Factory = UIFactory.GridView)]
        [FlowBloxDataGrid]
        public ObservableCollection<SmtpAttachmentMappingEntry> Attachments { get; set; } = new();

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.email_send_outline, 16, SKColors.CadetBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.email_send_outline, 32, SKColors.CadetBlue);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Communication;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Host));
            properties.Add(nameof(Port));
            properties.Add(nameof(UseSsl));
            properties.Add(nameof(AcceptInvalidCertificates));
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

                var port = FlowBloxFieldHelper.GetSimplePropertyOrFieldValue(this, x => x.Port);
                var useSsl = FlowBloxFieldHelper.GetSimplePropertyOrFieldValue(this, x => x.UseSsl);

                var resolvedHost = FlowBloxFieldHelper.ReplaceFieldsInString(Host ?? string.Empty)?.Trim();
                var resolvedFrom = FlowBloxFieldHelper.ReplaceFieldsInString(FromAddress ?? string.Empty)?.Trim();
                var resolvedTo = FlowBloxFieldHelper.ReplaceFieldsInString(ToAddresses ?? string.Empty);
                var resolvedCc = FlowBloxFieldHelper.ReplaceFieldsInString(CcAddresses ?? string.Empty);
                var resolvedBcc = FlowBloxFieldHelper.ReplaceFieldsInString(BccAddresses ?? string.Empty);
                var resolvedSubject = FlowBloxFieldHelper.ReplaceFieldsInString(Subject ?? string.Empty) ?? string.Empty;
                var resolvedBody = FlowBloxFieldHelper.ReplaceFieldsInString(Body ?? string.Empty) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(resolvedHost))
                {
                    CreateNotification(runtime, SMTPNotifications.HostIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                if (string.IsNullOrWhiteSpace(resolvedFrom))
                {
                    CreateNotification(runtime, SMTPNotifications.FromAddressIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                if (SmtpMailSender.ParseAddresses(resolvedTo).Count == 0)
                {
                    CreateNotification(runtime, SMTPNotifications.ToAddressesAreEmpty);
                    GenerateResult(runtime);
                    return;
                }
                var sendSucceeded = true;
                try
                {
                    var resolvedUser = FlowBloxFieldHelper.ReplaceFieldsInString(UserName ?? string.Empty);
                    var resolvedPassword = FlowBloxFieldHelper.ReplaceFieldsInString(Password ?? string.Empty);
                    var sender = new SmtpMailSender();
                    sender.Send(new SmtpMailSettings
                    {
                        Host = resolvedHost,
                        Port = port,
                        UseSsl = useSsl,
                        AcceptInvalidCertificates = AcceptInvalidCertificates,
                        UseAuthentication = UseAuthentication,
                        UserName = resolvedUser ?? string.Empty,
                        Password = resolvedPassword ?? string.Empty
                    }, new SmtpMailRequest
                    {
                        FromAddress = resolvedFrom,
                        ToAddresses = resolvedTo,
                        CcAddresses = resolvedCc,
                        BccAddresses = resolvedBcc,
                        Subject = resolvedSubject,
                        Body = resolvedBody,
                        IsBodyHtml = IsBodyHtml,
                        Attachments = BuildAttachments()
                    });
                    runtime.Report($"SMTP mail sent successfully via '{resolvedHost}:{port}'.");
                }
                catch (Exception ex)
                {
                    runtime.Report(ex.ToString());
                    CreateNotification(runtime, SMTPNotifications.MailSendFailure);
                    sendSucceeded = false;
                }

                if (sendSucceeded)
                    GenerateResult(runtime, bool.TrueString.ToLowerInvariant());
                else
                    GenerateResult(runtime);
            });
        }

        private List<SmtpMailAttachment> BuildAttachments()
        {
            var attachments = new List<SmtpMailAttachment>();
            foreach (var mapping in Attachments ?? Enumerable.Empty<SmtpAttachmentMappingEntry>())
            {
                if (mapping == null || mapping.Field == null)
                    continue;

                var fileName = FlowBloxFieldHelper.ReplaceFieldsInString(mapping.FileName ?? string.Empty)?.Trim();
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                var bytes = ConvertFieldToBytes(mapping);
                attachments.Add(new SmtpMailAttachment
                {
                    FileName = fileName,
                    MediaType = MediaTypeNames.Application.Octet,
                    Content = bytes
                });
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
            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "SMTP host is empty")]
            HostIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "From address is empty")]
            FromAddressIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "No To addresses configured")]
            ToAddressesAreEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "Mail send failure")]
            MailSendFailure
        }
    }
}
