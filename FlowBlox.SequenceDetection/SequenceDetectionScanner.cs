using FlowBlox.SequenceDetection.Data;
using FlowBlox.SequenceDetection.Model;
using FlowBlox.SequenceDetection.Util;
using System.Text.RegularExpressions;

namespace FlowBlox.SequenceDetection
{
    internal class SequenceDetectionScanner
    {
        static readonly Regex RegexFindSequences = new Regex(@"[a-zA-Z0-9äöüÄÖÜß]+|[^a-zA-Z0-9äöüÄÖÜß]+", RegexOptions.Compiled | RegexOptions.Multiline);

        const int SequenceLimit = 500;
        const double AverageLengthTolerance = 0.75;
        const int CombinationRange = 2;
            
        private readonly IEnumerable<SequenceDetectionInputEntry> _relatedInputEntries;
        private readonly IEnumerable<string> _relatedContents;
        private readonly CancellationTokenSource? _cancellationToken;

        public SequenceDetectionScanner(IEnumerable<SequenceDetectionInputEntry> relatedInputEntries, CancellationTokenSource? cancellationToken = null)
        {
            this._relatedInputEntries = relatedInputEntries;    
            this._relatedContents = relatedInputEntries.Select(x => x.Content);
            this._cancellationToken = cancellationToken;
        }

        public SequenceDetectionPattern? FindPatternForMockup(string content, AdjustableMatch match, long count, SequenceDetectionPattern? parent = null, int layer = 0)
        {
            string contentUntilMatch = content.Substring(0, match.Index);
            string contentAfterMatch = content.Substring(match.Index + match.Length);
            var listOfAllPhrases = CreateListOfAllPhrases(contentUntilMatch, contentAfterMatch);
            var orderedByDesc = listOfAllPhrases.OrderByDescending(x => x.Score);
            foreach (var phraseEntry in orderedByDesc)
            {
                if (_cancellationToken != null &&
                    _cancellationToken.IsCancellationRequested)
                    return null;

                string contentUntilEntry = content.Substring(0, phraseEntry.Index);
                    
                if (!contentUntilEntry.Contains(phraseEntry.Phrase!))
                {
                    var sequenceDetection = new SequenceDetectionPattern(parent);
                    sequenceDetection.Sequence = phraseEntry.Phrase;
                    if (phraseEntry.Distance > 0)
                    {
                        var newIndex = phraseEntry.Index + phraseEntry.Length;
                        string newContent = content.Substring(newIndex);
                        var subSequence = FindPatternForMockup(newContent, match.CopyAndAdjust(-newIndex), count, sequenceDetection, layer + 1);
                        if (subSequence == null)
                            return null;
                    }
                    else
                    {
                        if (!AddTerminationSequence(sequenceDetection, contentAfterMatch, match.Value, TerminationSequenceMode.ValueTerminationSequence))
                            return null;
                    }
                    if (!sequenceDetection.HasIterationTerminationSequence && count > 1)
                    {
                        if (phraseEntry.CouldMatchRequiredCount(count))
                            AddIterationTerminationSequence(sequenceDetection, match, phraseEntry.Phrase!, contentAfterMatch, count);

                        if (layer == 0 && !sequenceDetection.HasIterationTerminationSequence)
                            continue;
                    }
                    
                    return sequenceDetection;
                }
            }
            return default(SequenceDetectionPattern);
        }

        private bool AddIterationTerminationSequence(SequenceDetectionPattern pattern, AdjustableMatch match, string phrase, string contentAfterMatch, long count)
        {
            Regex regex = new Regex(RegexUtil.ExcapeRegexValue(phrase), RegexOptions.Multiline);
            var matchesAfterMatch = regex.Matches(contentAfterMatch);
            long lastMatchIndex = count - 2;
            if (matchesAfterMatch.Count > lastMatchIndex)
            {
                var lastMatch = matchesAfterMatch.ElementAt((int)lastMatchIndex);
                List<string> _result = new List<string>();
                int lastResultEnds = -1;
                string _content = contentAfterMatch.Substring(lastMatch.Index + lastMatch.Length);

                if (pattern.Next == null)
                {
                    if (string.IsNullOrEmpty(pattern.ValueTerminationSequence))
                        throw new InvalidOperationException("The pattern must have a value termination sequence or a next detection pattern");

                    var indexOfTerminationSequence = _content.IndexOf(pattern.ValueTerminationSequence);
                    if (indexOfTerminationSequence >= 0)
                    {
                        _result.Add(_content.Substring(0, indexOfTerminationSequence));
                        lastResultEnds = indexOfTerminationSequence + pattern.ValueTerminationSequence.Length;
                    }
                }
                else
                {
                    SequenceSearch.Instance.SearchFor(_content, pattern.Next, ref _result, ref lastResultEnds);
                }
                if (lastResultEnds >= 0)
                {
                    var lastResultValue = _result.First();
                    if (IsAverageLengthMatch(lastResultValue, match.Value, AverageLengthTolerance))
                    {
                        var contentBetweenEndOfMatchAndTerminationSequence = contentAfterMatch.Substring(0, lastMatch.Index + lastMatch.Length + lastResultValue.Length);
                        AddTerminationSequence(pattern, _content.Substring(lastResultEnds), contentBetweenEndOfMatchAndTerminationSequence, TerminationSequenceMode.IterationTerminationSequence);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAverageLengthMatch(string newString, string baseString, double tolerance)
        {
            if (string.IsNullOrEmpty(baseString))
                return false;

            double averageLength = baseString.Length;
            double deviation = Math.Abs(newString.Length - averageLength) / averageLength;
            return (deviation <= tolerance);
        }

        private enum TerminationSequenceMode
        {
            ValueTerminationSequence,
            IterationTerminationSequence
        }

        private bool AddTerminationSequence(SequenceDetectionPattern pattern, string contentAfterMatch, string value, TerminationSequenceMode mode)
        {
            var listOfAfterwardsPhrases = CreateListOfAllPhrases(contentAfterMatch, rightMode: true);
            var orderByDesc = listOfAfterwardsPhrases.OrderByDescending(x => x.Score);
            foreach (var afterwardsPhraseEntry in orderByDesc.Where(x =>
                mode != TerminationSequenceMode.ValueTerminationSequence ||
                x.Distance == 0))
            {
                if (_cancellationToken != null &&
                    _cancellationToken.IsCancellationRequested)
                    return false;

                if (!value.Contains(afterwardsPhraseEntry.Phrase!))
                {
                    if (mode == TerminationSequenceMode.ValueTerminationSequence)
                        pattern.ValueTerminationSequence = afterwardsPhraseEntry.Phrase;
                    else
                    {
                        pattern.IterationTerminationSequence = afterwardsPhraseEntry.Phrase;

                        if (_relatedInputEntries.Any() &&
                            !_relatedInputEntries.All(x => SequenceSearch.Instance.IsSuccessful(x.Content, pattern.Root, x.Count)))
                        {
                            string phrases = string.Join("\n", orderByDesc.Select(x => x.Phrase));
                            pattern.IterationTerminationSequence = string.Empty;
                            continue;
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        private void AddCombinations(string content, string contentAfterMatch, bool rightMode, MatchCollection matchCollection, int index, int range, Dictionary<string, int> phraseToCount, List<PhraseEntry> phrases)
        {
            var relevantMatches = matchCollection.Skip(index - range).Take(range * 2 + 1);
            for (int length = relevantMatches.Count(); length > 0; length--)
            {
                for (int i = 0; i < length; i++)
                {
                    var matchesToCombine = relevantMatches.Skip(i).Take(length);
                    if (matchesToCombine.Count() < length)
                        break;

                    int mIndex = matchesToCombine.First().Index;
                    int mLength = matchesToCombine.Sum(x => x.Length);
                    int mEnds = mIndex + mLength;

                    string phrase = string.Concat(matchesToCombine.Select(x => x.Value)).Trim();

                    if (string.IsNullOrEmpty(phrase))
                        continue;

                    if (phrase.Contains('\r') || phrase.Contains('\n'))
                        continue;

                    if (phrases.Any(x => x.Index == mIndex && x.Length == mLength))
                        continue;

                    if (!_relatedContents.Any() ||
                        _relatedContents.Any(x => x.Contains(phrase)))
                    {
                        phrases.Add(new PhraseEntry(phraseToCount)
                        {
                            Phrase = phrase,
                            Distance = !rightMode ? content.Length - mEnds : mIndex,
                            Index = mIndex,
                            Length = mLength,
                            NumberOfPhrasesInAfterwards = RegexUtil.CountOccurences(contentAfterMatch, phrase),
                        });
                            
                        if (!phraseToCount.ContainsKey(phrase))
                            phraseToCount[phrase] = 1;
                        else
                            phraseToCount[phrase]++;
                    }
                }
            }
        }

        private List<PhraseEntry> CreateListOfAllPhrases(string content, string contentAfterMatch = "", long requiredCount = 0, bool rightMode = false)
        {
            var matchCollection = RegexFindSequences.Matches(content);
            var result = new List<PhraseEntry>();
            Dictionary<string, int> phraseToCount = new Dictionary<string, int>();
            if (rightMode)
            {
                for (int index = 0; index < Math.Min(matchCollection.Count, SequenceLimit); index++)
                {
                    AddCombinations(content, contentAfterMatch, rightMode, matchCollection, index, CombinationRange, phraseToCount, result);
                }
            }
            else
            {
                for (int index = matchCollection.Count - 1; index >= Math.Max(matchCollection.Count - SequenceLimit, 0); index--)
                {
                    AddCombinations(content, contentAfterMatch, rightMode, matchCollection, index, CombinationRange, phraseToCount, result);
                }
            }
            result.ForEach(x => x.CalculateScore(requiredCount, rightMode));
            return result;
        }
    }   
}
