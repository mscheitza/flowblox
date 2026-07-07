namespace FlowBlox.AIAssistant.Constants
{
    public static class AssistantConfigurationLimits
    {
        public const int MinToolRounds = 1;
        public const int MaxToolRounds = 200;

        public const int MinLatestMessages = 1;
        public const int MaxLatestMessages = 50;

        public const int MinContextTokens = 0;
        public const int MinReservedResponseTokens = 0;

        public const int MinApproximateCharactersPerToken = 1;
        public const int MaxApproximateCharactersPerToken = 20;
    }
}
