namespace FlowBlox.Core.Extensions
{
    public static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            if (hashSet == null) throw new ArgumentNullException(nameof(hashSet));
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                hashSet.Add(item);
            }
        }
    }
}
