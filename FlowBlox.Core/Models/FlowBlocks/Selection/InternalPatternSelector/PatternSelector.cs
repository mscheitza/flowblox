using System.Text.RegularExpressions;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Models.FlowBlocks.Selection.StartEndPatternSelector
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
    }
}