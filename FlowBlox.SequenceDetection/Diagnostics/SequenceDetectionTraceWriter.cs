namespace FlowBlox.SequenceDetection.Diagnostics
{
    internal static class SequenceDetectionTraceWriter
    {
        public static IDisposable Begin(Action<string> writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            var previousContext = SequenceDetectionTraceContext.CurrentContext.Value;
            var context = new SequenceDetectionTraceContext(writer);
            SequenceDetectionTraceContext.CurrentContext.Value = context;
            return new SequenceDetectionTraceScope(context, previousContext);
        }

        public static void Write(string message)
        {
            SequenceDetectionTraceContext.CurrentContext.Value?.Write(message);
        }
    }
}
