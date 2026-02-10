namespace FlowBlox.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T> source) where T : class
        {
            if (source == null) 
                throw new ArgumentNullException(nameof(source));
            
            return source.Where(item => item != null);
        }

        public static IEnumerable<string> ExceptNullOrEmpty(this IEnumerable<string> source)
        {
            if (source == null) 
                throw new ArgumentNullException(nameof(source));
            
            return source.Where(item => !string.IsNullOrEmpty(item));
        }
    }
}
