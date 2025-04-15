using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TValue, TKey> ReverseDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            var dictionary = new Dictionary<TValue, TKey>();
            foreach (var entry in source)
            {
                if (!dictionary.ContainsKey(entry.Value))
                {
                    dictionary.Add(entry.Value, entry.Key);
                }
                else
                {
                    throw new ArgumentException($"Cannot reverse dictionary. The value '{entry.Value}' is not unique.");
                }
            }
            return dictionary;
        }
    }
}
