using FlowBlox.Core.Models.Components;
using System.Text;

namespace FlowBlox.Core.Util.Fields
{
    public static class FlowBloxBinaryContentHelper
    {
        public static byte[] Resolve(FieldElement field, bool allowPlainText = false, Encoding textEncoding = null)
        {
            ArgumentNullException.ThrowIfNull(field);

            if (field.GetConfiguredType() == typeof(byte[]))
                return field.Value as byte[] ?? [];

            var value = field.StringValue ?? string.Empty;
            var resolvedValue = FlowBloxFieldHelper.ReplaceFieldsInString(value) ?? string.Empty;
            var binaryCandidate = resolvedValue.Trim();

            if (File.Exists(binaryCandidate))
                return File.ReadAllBytes(binaryCandidate);

            try
            {
                return Convert.FromBase64String(binaryCandidate);
            }
            catch (FormatException) when (allowPlainText)
            {
                return (textEncoding ?? Encoding.UTF8).GetBytes(resolvedValue);
            }
            catch (FormatException ex)
            {
                throw new FormatException(
                    $"Field '{field.FullyQualifiedName}' must contain binary data, Base64 content, or an existing file path.",
                    ex);
            }
        }
    }
}
