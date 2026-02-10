namespace FlowBlox.Core.Util
{
    public class TextHelper
    {
        public static string ShortenString(string value, int maxLength, bool removeLineBreaks)
        {
            if (TryShortenString(value, maxLength, removeLineBreaks, out string shortValue))
                return shortValue;

            return value;
        }

        public static bool TryShortenString(string value, int maxLength, bool removeLineBreaks, out string shortenedValue)
        {
            if (maxLength < 3)
                throw new ArgumentException($"Parameter \"{nameof(maxLength)}\" must be 3 or greater.");

            if (value == null)
            {
                shortenedValue = null;
                return false;
            }

            shortenedValue = value;
            bool shortened = false;

            if (removeLineBreaks)
            {
                shortenedValue = shortenedValue
                    .Replace("\r\n", " ")
                    .Replace("\n", " ");

                shortened = true;
            }

            if (shortenedValue.Length > maxLength)
            {
                shortenedValue = shortenedValue.Substring(0, maxLength - 3) + "...";
                shortened = true;
            }

            return shortened;
        }
    }
}
