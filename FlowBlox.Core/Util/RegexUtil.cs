namespace FlowBlox.Core.Utilities
{
    public static class RegexUtil
    {
        public static string EscapeRegexValue(string value)
        {
            List<string> regexChars = new List<string>()
                {
                    "/", "[", "\\", "-", "]", ".", "?", "(", ")", "{", "}", "*", "|", "$", "^", "=", "#", "+"
                };

            foreach (string regexChar in regexChars)
            {
                value = value.Replace(regexChar, "\\" + regexChar);
            }

            foreach (string regexChar in regexChars)
            {
                value = value.Replace("\\\\" + regexChar, "\\" + regexChar);
            }

            return value;
        }
    }
}
