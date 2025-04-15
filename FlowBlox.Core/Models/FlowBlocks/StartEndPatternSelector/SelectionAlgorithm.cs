namespace FlowBlox.Core.Models.FlowBlocks.StartEndPatternSelector
{
    public class SelectionAlgorithm : ISelectionAlgorithm
    {
        private const int InvalidIndex = -1;

        public List<string> Select(string content, string startPattern, string endPattern, bool enableMultiline)
        {
            List<string> matchList = new List<string>();

            List<string> startPatterns = new List<string>() { startPattern };
            List<string> endPatterns = new List<string>() { endPattern };

            if (startPattern != null && startPattern.Contains("\r\n"))
                startPatterns.Add(startPattern.Replace("\r\n", "\n"));

            if (endPattern != null && endPattern.Contains("\r\n"))
                endPatterns.Add(endPattern.Replace("\r\n", "\n"));

            foreach (string startDelimiter in startPatterns)
            {
                foreach (string endDelimiter in endPatterns)
                {
                    int indexStartOfMatch = 0;
                    int indexRecentMatch = InvalidIndex;

                    while (true)
                    {
                        indexStartOfMatch = content.IndexOf(startDelimiter, indexStartOfMatch);
                        if ((indexStartOfMatch == InvalidIndex) || (indexStartOfMatch == indexRecentMatch))
                        {
                            break;
                        }

                        if (indexRecentMatch == InvalidIndex)
                        {
                            indexRecentMatch = indexStartOfMatch;
                        }

                        indexStartOfMatch += startDelimiter.Length;

                        int indexEndOfMatch;
                        if (endDelimiter == null)
                            indexEndOfMatch = content.Length;
                        else
                            indexEndOfMatch = content.IndexOf(endDelimiter, indexStartOfMatch);

                        if (indexEndOfMatch == InvalidIndex)
                            break;

                        bool noFurtherMatch = false;
                        while (!noFurtherMatch)
                        {
                            int indexNextStartOfMatch = content.IndexOf(startDelimiter, indexStartOfMatch);
                            if ((indexNextStartOfMatch != InvalidIndex) &&
                                ((indexNextStartOfMatch + startDelimiter.Length) < indexEndOfMatch))
                            {
                                indexStartOfMatch = indexNextStartOfMatch + startDelimiter.Length;
                            }
                            else noFurtherMatch = true;
                        }

                        int matchLength = indexEndOfMatch - indexStartOfMatch;
                        string matched = content.Substring(indexStartOfMatch, matchLength);
                        if (enableMultiline || !matched.Contains("\n"))
                        {
                            matchList.Add(matched);
                            indexStartOfMatch = indexEndOfMatch;
                        }
                    }
                }
            }
            return matchList;
        }
    }
}
