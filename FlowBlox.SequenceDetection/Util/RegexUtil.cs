using System.Text.RegularExpressions;

namespace FlowBlox.SequenceDetection.Util
{
    internal class RegexUtil
    {
        static readonly List<string> regexChars = new List<string>()
        {
            "/","[", "\\", "-", "]", ".", "?", "(", ")", "{", "}", "*", "|", "$", "^", "=", "#", "+"
        };

        internal static string ExcapeRegexValue(string value)
        {
            regexChars.ForEach(x => value = value.Replace(x, "\\" + x));
            regexChars.ForEach(x => value = value.Replace("\\\\" + x, "\\" + x));
            return value;
        }

        internal static int CountOccurences(string content, string value)
        {
            var excapedValue = ExcapeRegexValue(value);
            var regex = new Regex(excapedValue, RegexOptions.Multiline);
            return regex.Matches(content).Count();  
        }
    }
}