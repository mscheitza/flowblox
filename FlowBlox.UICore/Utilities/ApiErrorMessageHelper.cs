using System.Globalization;
using System.Text;

namespace FlowBlox.UICore.Utilities
{
    public static class ApiErrorMessageHelper
    {
        public static string BuildErrorMessage(string apiErrorMessage) => BuildErrorMessage(null, apiErrorMessage);
        public static string BuildErrorMessage(string localizedErrorMessage = null, string apiErrorMessage = null)
        {
            var messageBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(localizedErrorMessage))
            {
                messageBuilder.Append(localizedErrorMessage);
            }
            else
            {
                messageBuilder.Append(GetDefaultLocalizedMessage());
            }

            if (!string.IsNullOrWhiteSpace(apiErrorMessage))
            {
                messageBuilder.AppendLine();
                messageBuilder.AppendLine();
                messageBuilder.Append(GetTechnicalDetailsLabel());
                messageBuilder.AppendLine(apiErrorMessage);
            }

            return messageBuilder.ToString();
        }

        private static string GetDefaultLocalizedMessage()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return culture switch
            {
                "de" => "Die angeforderte API-Aktion ist fehlgeschlagen.",
                _ => "The requested API action has failed."
            };
        }

        private static string GetTechnicalDetailsLabel()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return culture switch
            {
                "de" => "Technische Details:\n",
                _ => "Technical details:\n"
            };
        }
    }
}
