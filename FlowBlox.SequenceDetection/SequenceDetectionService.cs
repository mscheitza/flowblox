using System.Text.RegularExpressions;
using FlowBlox.SequenceDetection.Data;
using FlowBlox.SequenceDetection.Model;
using FlowBlox.SequenceDetection.Util;

namespace FlowBlox.SequenceDetection
{
    public class SequenceDetectionService
    {
        private static SequenceDetectionService? _instance;

        private SequenceDetectionService() 
        {
            
        }

        public static SequenceDetectionService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SequenceDetectionService();
                return _instance;
            }
        }

        public SequenceDetectionPattern? Detect(SequenceDetectionInputData targetData)
        {
            if (targetData.Entries == null)
                throw new InvalidOperationException("The target data mus have entries.");

            SequenceDetectionPattern? result = default(SequenceDetectionPattern);
            List<Task<SequenceDetectionPattern?>> tasks = new List<Task<SequenceDetectionPattern?>>();
            var cancellationToken = new CancellationTokenSource();
            if (targetData.Timeout > 0)
                cancellationToken.CancelAfter(TimeSpan.FromSeconds(targetData.Timeout));
            foreach (var entry in targetData.Entries)
            {
                IEnumerable<SequenceDetectionInputEntry> otherEntries = targetData.Entries.Except(new[] { entry });

                if (string.IsNullOrEmpty(entry.Match))
                    throw new ArgumentException("Match must not be null or empty");
                if (string.IsNullOrEmpty(entry.Content))
                    throw new ArgumentException("Content must not be null or empty");
                if (entry.Count == 0)
                    throw new ArgumentException("Count must be greater than zero.");

                var regex = new Regex(RegexUtil.ExcapeRegexValue(entry.Match));
                var matchCollection = regex.Matches(entry.Content);
                foreach (Match match in matchCollection)
                {   
                    tasks.Add(Task.Run(() =>
                    {
                        var adjustableMatch = new AdjustableMatch(match.Value, match.Index, match.Length);
                        var scanner = new SequenceDetectionScanner(otherEntries, cancellationToken);
                        var pattern = scanner.FindPatternForMockup(entry.Content, adjustableMatch, entry.Count);
                        if (pattern == null)
                            return null;

                        if (otherEntries.Count() > 0 &&
                            !otherEntries.All(x => SequenceSearch.Instance.IsSuccessful(x.Content, pattern, x.Count)))
                            return null;

                        if (entry.Count > 1 && !pattern!.HasIterationTerminationSequence)
                            return null;

                        cancellationToken.Cancel();

                        return pattern;
                    }));
                }
            }
            Task.WaitAny(tasks.ToArray());
            result = tasks
                .Where(x => x.Status == TaskStatus.RanToCompletion)
                .Select(x => x.Result)
                .Where(x => x != null)
                .OrderByDescending(x => x!.TotalLength)
                .FirstOrDefault();
            return result;
        }
    }
}