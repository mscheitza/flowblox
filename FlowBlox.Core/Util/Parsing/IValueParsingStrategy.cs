namespace FlowBlox.Core.Util.Parsing
{
    public interface IValueParsingStrategy
    {
        Type ValueType { get; }
        object Parse(string value);
    }

    public abstract class ValueParsingStrategy<T> : IValueParsingStrategy
    {
        public Type ValueType => typeof(T);
        public abstract T Parse(string value);
        object IValueParsingStrategy.Parse(string value) => Parse(value);
    }
}
