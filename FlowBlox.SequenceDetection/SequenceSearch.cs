using FlowBlox.SequenceDetection.Util;
using System.Text.RegularExpressions;

namespace FlowBlox.SequenceDetection
{
    public class SequenceSearch
    {
        private static SequenceSearch? _instance;

        public static SequenceSearch Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SequenceSearch();
                return _instance;
            }
        }

        internal bool IsSuccessful(string content, SequenceDetectionPattern patternDescription, int count)
        {
            List<string> result = new List<string>();
            SearchFor(content, patternDescription, ref result);
            return result.Count == count;
        }

        public void SearchFor(string content, SequenceDetectionPattern patternDescription, ref List<string> result)
        {
            int _index = 0;
            SearchFor(content, patternDescription, ref result, ref _index);
        }

        internal void SearchFor(string content, SequenceDetectionPattern patternDescription, ref List<string> result, ref int index)
        {   
            if (patternDescription == null)
                throw new ArgumentNullException(nameof(patternDescription));

            Regex regexFindSequence = new Regex(RegexUtil.ExcapeRegexValue(patternDescription.Sequence!), RegexOptions.Multiline);

            string adjustedContent = content;
            if (!string.IsNullOrEmpty(patternDescription.IterationTerminationSequence))
            {
                var limitToIndex = content.IndexOf(patternDescription.IterationTerminationSequence);
                if (limitToIndex < 0)
                    return;

                adjustedContent = content.Substring(0, limitToIndex);
            }

            var matches = regexFindSequence.Matches(adjustedContent);
            foreach (Match match in matches)
            {
                int position = match.Index + match.Length;

                if (patternDescription.Next != null)
                    SearchFor(adjustedContent.Substring(position), patternDescription.Next!, ref result, ref index);
                else
                {
                    if (patternDescription.ValueTerminationSequence == null)
                        throw new InvalidOperationException("No termination sequence found.");

                    var terminationIndex = adjustedContent
                        .Substring(position)
                        .IndexOf(patternDescription.ValueTerminationSequence);

                    if (terminationIndex >= 0)
                    {
                        terminationIndex += position;
                        int length = terminationIndex - position;
                        var value = adjustedContent.Substring(position, length);
                        index = position + length;
                        result.Add(value);
                    }
                }

                if (string.IsNullOrEmpty(patternDescription.IterationTerminationSequence))
                    break;
            }
        }
    }
}
