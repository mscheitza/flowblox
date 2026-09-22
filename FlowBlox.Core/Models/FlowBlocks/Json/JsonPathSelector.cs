using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    public static class JsonPathSelector
    {
        /// <summary>
        /// Navigates through a JObject/JArray using a slash-delimited path.
        /// </summary>
        /// <param name="root">The starting token (must be a JObject or JArray).</param>
        /// <param name="path">Path in the format "participants/addresses", "participants/addresses/0" or "addresses/@Country=Germany/Street".</param>
        /// <param name="parent">The parent token of the target path (JObject or JArray).</param>
        /// <param name="propertyName">The property name or array index (as a string) of the target path.</param>
        /// <returns>The token at the specified path, or null if it does not exist.</returns>
        public static JToken GetJToken(JToken root, string path, out JToken parent, out string propertyName)
        {
            parent = null;
            propertyName = null;

            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (string.IsNullOrWhiteSpace(path))
            {
                parent = null;
                propertyName = null;
                return root;
            }

            var parts = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
            JToken current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                bool isLast = (i == parts.Length - 1);
                parent = current;
                propertyName = part;

                if (JsonPathComparison.TryParse(part, out var comparison))
                {
                    var filtered = ApplyFilter(current, comparison);
                    if (isLast)
                        return filtered;

                    current = filtered;
                    continue;
                }

                if (current is JObject obj)
                {
                    if (!obj.TryGetValue(part, out var child))
                        child = null;

                    if (isLast)
                        return child;

                    current = child ?? throw new InvalidOperationException($"Property '{part}' not found in JSON object.");
                }
                else if (current is JArray arr)
                {
                    var child = ResolveArraySegment(arr, part);
                    if (isLast)
                        return child;

                    current = child;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot navigate through token of type {current.Type}.");
                }
            }

            return current;
        }

        private static JToken ResolveArraySegment(JArray arr, string part)
        {
            if (int.TryParse(part, out var index))
            {
                if (index < 0 || index >= arr.Count)
                    throw new InvalidOperationException($"Array index '{index}' is out of range.");

                return arr[index];
            }

            var projected = new JArray();
            foreach (var item in arr)
            {
                if (item is JObject obj && obj.TryGetValue(part, out var child))
                    AddProjectedToken(projected, child);
            }

            return projected;
        }

        private static JArray ApplyFilter(JToken current, JsonPathComparison comparison)
        {
            var source = current switch
            {
                JArray arr => arr,
                JObject obj => new JArray(obj),
                _ => throw new InvalidOperationException($"Cannot apply JSON path filter to token of type {current.Type}.")
            };

            var result = new JArray();
            foreach (var item in source)
            {
                if (comparison.IsMatch(item))
                    result.Add(item);
            }

            return result;
        }

        private static void AddProjectedToken(JArray target, JToken token)
        {
            if (token is JArray arr)
            {
                foreach (var item in arr)
                    target.Add(item);

                return;
            }

            target.Add(token);
        }
    }
}
