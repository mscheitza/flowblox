using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    public static class JsonPathSelector
    {
        /// <summary>
        /// Navigates through a JObject/JArray using a slash-delimited path.
        /// </summary>
        /// <param name="root">The starting token (must be a JObject or JArray).</param>
        /// <param name="path">Path in the format "participants/addresses" or "participants/addresses/0".</param>
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
                    if (!int.TryParse(part, out var index))
                        throw new InvalidOperationException($"Expected array index at '{part}', but got non-numeric value.");

                    if (index < 0 || index >= arr.Count)
                        throw new InvalidOperationException($"Array index '{index}' is out of range.");

                    var child = arr[index];
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
    }
}