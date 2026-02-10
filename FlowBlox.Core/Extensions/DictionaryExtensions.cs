namespace FlowBlox.Core.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Attempts to create a reversed dictionary (value -> key). Returns false on duplicate values.
        /// </summary>
        public static bool TryReverseDictionary<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            out IDictionary<TValue, TKey> reversed)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            var result = new Dictionary<TValue, TKey>(source.Count);
            foreach (var entry in source)
            {
                if (!result.TryAdd(entry.Value, entry.Key))
                {
                    reversed = null;
                    return false;
                }
            }

            reversed = result;
            return true;
        }

        /// <summary>
        /// Creates a reversed dictionary (key -> value). Throws if duplicate values are present.
        /// </summary>
        public static IDictionary<TValue, TKey> ReverseDictionary<TKey, TValue>(
            this IDictionary<TKey, TValue> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (source.TryReverseDictionary(out var reversed))
                return reversed;

            throw new ArgumentException("Cannot reverse dictionary. Duplicate values detected.");
        }
    }
}
