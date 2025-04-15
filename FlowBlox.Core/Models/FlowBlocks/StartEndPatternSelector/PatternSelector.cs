using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Models.FlowBlocks.StartEndPatternSelector
{
    public class PatternSelector
    {
        private static ISelectionAlgorithm selectionAlgorithm = new SelectionAlgorithm();

        /// <summary>
        /// Selects all matches from a list <paramref name="contents"/> based on a start pattern/end pattern expression and returns them.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="startPattern"></param>
        /// <param name="endPattern"></param>
        /// <param name="enableMultiline"></param>
        /// <returns></returns>
        public static List<string> SelectPatternFromContext(List<string> contents, string startPattern, string endPattern, bool enableMultiline)
        {
            return contents.SelectMany(x => SelectPatternFromContext(x, startPattern, endPattern, enableMultiline)).ToList();
        }

        /// <summary>
        /// Selects all matches from a <c>content</c> based on a start pattern/end pattern expression and returns them.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startPattern"></param>
        /// <param name="endPattern"></param>
        /// <param name="enableMultiline"></param>
        /// <returns></returns>
        public static List<string> SelectPatternFromContext(string content, string startPattern, string endPattern, bool enableMultiline)
        {
            if (content == null)
                return new List<string>();

            return selectionAlgorithm.Select(content, startPattern, endPattern, enableMultiline);
        }

        private static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, System.Globalization.NumberStyles.HexNumber)).ToString();
                });
        }

        /// <summary>
        /// Gibt aus einem PatternSelection Match einen gültigen Wert zurück. Es können Html-Codes aufgelöst und
        /// Html-/Xml-Tags ausgeblendet werden. Wird ein NullValue angegeben, wird im Falle eines leeren Wertes dieser
        /// NullValue zurückgegeben.
        /// </summary>
        /// <param name="selectionMatch">Der aus der <c>SelectPatternFromContext</c> Methode zurückgegebene Match.</param>
        /// <param name="nullValue">Geben Sie hier einen <c>string</c> an oder <c>null</c>.</param>
        /// <param name="hideHtmlXml">Sollen HTML/XML Tags ausgeblendet werden?</param>
        /// <param name="replaceHtmlCodes">Sollen HTML/UTF8 Codes aufgelöst werden?</param>
        /// <returns></returns>
        public static string GetValidMatch
            (
                string selectionMatch,
                bool hideHtmlXml,
                bool replaceHtmlCodes
            )
        {
            if (replaceHtmlCodes)
            {
                selectionMatch = System.Web.HttpUtility.HtmlDecode(selectionMatch);

                if (selectionMatch.Contains("\\u"))
                {
                    selectionMatch = DecodeEncodedNonAsciiCharacters(selectionMatch);
                }

                string CellSeparator = FlowBloxOptions.GetOptionInstance().OptionCollection["General.CellSeparator"].Value;
                string ReplaceCellSeparatorBy = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ReplaceCellSeparatorBy"].Value;

                if (!CellSeparator.Equals(ReplaceCellSeparatorBy))
                {
                    selectionMatch = selectionMatch.Replace(CellSeparator, ReplaceCellSeparatorBy);
                }
            }

            selectionMatch = selectionMatch.Replace("\r", string.Empty);
            selectionMatch = selectionMatch.Replace("\n", string.Empty);
            selectionMatch = selectionMatch.Replace("\t", string.Empty);

            selectionMatch = selectionMatch.Trim();
            return selectionMatch;
        }
    }
}