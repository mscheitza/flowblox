using Newtonsoft.Json.Serialization;
using System.Collections;

namespace FlowBlox.Core.Util.Json.ValueProvider
{
    internal sealed class EmptyEnumerableKeywordValueProvider : IValueProvider
    {
        public const string EmptyCollectionKeyword = "EmptyCollection";

        private readonly IValueProvider _inner;

        public EmptyEnumerableKeywordValueProvider(IValueProvider inner)
        {
            _inner = inner;
        }

        public object GetValue(object target)
        {
            var value = _inner.GetValue(target);
            if (value is not IEnumerable enumerable || value is string)
                return value;

            var enumerator = enumerable.GetEnumerator();
            try
            {
                return enumerator.MoveNext()
                    ? value
                    : EmptyCollectionKeyword;
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public void SetValue(object target, object value)
        {
            _inner.SetValue(target, value);
        }
    }
}