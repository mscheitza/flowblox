using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Notifications;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;

namespace FlowBlox.Core.Services.Notifications
{
    public static class RuntimeNotificationMailFormatter
    {
        public static string CreateSubject(RuntimeNotificationContext context)
        {
            var project = string.IsNullOrWhiteSpace(context.ProjectName) ? "FlowBlox" : context.ProjectName;
            return $"[FlowBlox] {GetEventName(context.Type)} - {project}";
        }

        public static string CreateHtmlBody(RuntimeNotificationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var accent = context.Type switch
            {
                RuntimeNotificationType.RuntimeCompletedSuccessfully => "#047857",
                RuntimeNotificationType.RuntimeStartedSuccessfully => "#2563eb",
                _ => "#b91c1c"
            };

            var rows = new StringBuilder();
            AddRow(rows, "Event", GetEventName(context.Type));
            AddRow(rows, "Occurred at", context.OccurredAt.ToString("F", CultureInfo.InvariantCulture));
            AddRow(rows, "Project", context.ProjectName);
            AddRow(rows, "Flow block", context.FlowBlockName);
            AddRow(rows, "Triggering error", context.TriggeringErrorMessage);
            AddRow(rows, "Host", context.HostName);
            AddRow(rows, "IP addresses", string.Join(", ", context.IpAddresses ?? Array.Empty<string>()));
            AddRow(rows, "Process", $"{context.ProcessName} ({context.ProcessId})");
            AddRow(rows, "Runtime log file", context.RuntimeLogFilePath);
            if (context.RuntimeStartedAt != default)
                AddRow(rows, "Runtime started at", context.RuntimeStartedAt.ToString("F", CultureInfo.InvariantCulture));
            if (context.RuntimeStartedAt != default && context.OccurredAt >= context.RuntimeStartedAt)
                AddRow(rows, "Duration", (context.OccurredAt - context.RuntimeStartedAt).ToString("g"));
            if (context.Cancellation != null)
            {
                AddRow(rows, "Abort type", context.Cancellation.CancellationKind.ToString());
                AddRow(rows, "Abort reason", context.Cancellation.Reason);
            }

            var message = Encode(context.Message);
            var exception = Encode(context.Exception?.ToString());
            var exceptionSection = string.IsNullOrWhiteSpace(exception)
                ? string.Empty
                : "<h3 style='margin:24px 0 8px;color:#1f2937;font-size:15px;'>Exception details</h3>" +
                  $"<pre style='white-space:pre-wrap;word-break:break-word;background:#111827;color:#f9fafb;padding:14px;border-radius:4px;font:12px Consolas,monospace;'>{exception}</pre>";

            return $"<!DOCTYPE html><html><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width,initial-scale=1.0'></head>" +
                "<body style='margin:0;padding:0;font-family:Segoe UI,Arial,sans-serif;background:#f5f7fa;'>" +
                "<table width='100%' cellpadding='0' cellspacing='0'><tr><td align='center' style='padding:30px 10px;'>" +
                "<table width='680' cellpadding='0' cellspacing='0' style='max-width:680px;background:#ffffff;border-radius:6px;box-shadow:0 2px 6px rgba(0,0,0,.05);'>" +
                $"<tr><td style='padding:24px;border-bottom:1px solid #e5e7eb;border-top:4px solid {accent};'>" +
                "<h1 style='margin:0;font-size:22px;color:#1f2937;'>FlowBlox</h1></td></tr>" +
                "<tr><td style='padding:24px;color:#374151;font-size:14px;line-height:1.6;'>" +
                $"<h2 style='margin:0 0 12px;font-size:18px;color:{accent};'>{Encode(GetEventName(context.Type))}</h2>" +
                (string.IsNullOrWhiteSpace(message) ? string.Empty : $"<p style='margin:0 0 20px;'>{message}</p>") +
                $"<table width='100%' cellpadding='7' cellspacing='0' style='border-collapse:collapse;'>{rows}</table>{exceptionSection}</td></tr>" +
                $"<tr><td style='padding:16px;background:#f9fafb;color:#9ca3af;font-size:12px;text-align:center;'>{Encode(GetApplicationCopyright())}</td></tr>" +
                "</table></td></tr></table></body></html>";
        }

        private static string GetApplicationCopyright()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            return string.IsNullOrWhiteSpace(copyright)
                ? $"© {DateTime.Now.Year} FlowBlox"
                : copyright;
        }

        private static string GetEventName(RuntimeNotificationType type) => type switch
        {
            RuntimeNotificationType.RuntimeError => "Runtime error",
            RuntimeNotificationType.RuntimeAborted => "Runtime aborted",
            RuntimeNotificationType.RuntimeCompletedSuccessfully => "Runtime completed successfully",
            RuntimeNotificationType.RuntimeStartedSuccessfully => "Runtime started successfully",
            RuntimeNotificationType.RuntimeStartFailed => "Runtime start failed",
            _ => type.ToString()
        };

        private static void AddRow(StringBuilder rows, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            rows.Append("<tr style='border-bottom:1px solid #e5e7eb;'><td style='width:185px;font-weight:600;vertical-align:top;'>")
                .Append(Encode(label))
                .Append("</td><td style='word-break:break-word;'>")
                .Append(Encode(value))
                .Append("</td></tr>");
        }

        private static string Encode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
