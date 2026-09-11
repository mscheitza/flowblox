namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    /// <summary>
    /// Provider-neutral transfer object for metadata that belongs to an assistant chat message.
    /// Providers can map these raw values to their concrete metadata keys in <see cref="AIProviderBase.BuildChatMessageMetadata"/>.
    /// Store this object with assistant history messages when metadata must survive history persistence or provider changes.
    /// </summary>
    public sealed class AIChatMessageMetadata
    {
        public Dictionary<string, string> Values { get; set; } = new(StringComparer.Ordinal);

        public bool HasValues => Values.Count > 0;

        public AIChatMessageMetadata Clone()
        {
            return new AIChatMessageMetadata
            {
                Values = new Dictionary<string, string>(Values, StringComparer.Ordinal)
            };
        }
    }
}
