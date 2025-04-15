namespace FlowBlox.SequenceDetection.Data
{
    internal class PhraseEntry
    {
        const float CouldMatchRequiredCountIncrement = 50;

        private readonly Dictionary<string, int> _phraseToCount;

        public PhraseEntry(Dictionary<string, int> phraseToCount)
        {
            this._phraseToCount = phraseToCount;
        }

        public string? Phrase { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public int Distance { get; set; }
        public int NumHits => _phraseToCount[Phrase!];
        public int NumberOfPhrasesInAfterwards { get; set; }
        public double Score { get; private set; }


        public bool CouldMatchRequiredCount(long requiredCount)
        {
            if (requiredCount > 1 &&
                NumberOfPhrasesInAfterwards + 1 >= requiredCount)
            {
                return true;
            }
            return false;
        }

        private bool _calculated = false;
        public void CalculateScore(long requiredCount, bool rightMode)
        {
            if (!_calculated)
            {
                bool couldMatchRequiredCount = CouldMatchRequiredCount(requiredCount);
                this.Score = CalculateScore(this.Distance, this.NumHits, this.Length, couldMatchRequiredCount, 1, !rightMode ? 0.5f : 0, 0.25f);
                _calculated = true;
            }
        }

        double CalculateScore(int distance, int numHits, int length, bool couldMatchRequiredCount, float weightDistance, float weightHits, float weightLength)
        {
            float score = 1 - (weightDistance * distance + weightHits * numHits + weightLength * length);
            if (couldMatchRequiredCount)
                score += CouldMatchRequiredCountIncrement;
            return 1 / (1 + Math.Exp(-score));
        }
    }
}
