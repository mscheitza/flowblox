using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    /// <summary>
    /// Resolves a unique target through Newtonsoft JPath and creates its final
    /// object property when that property does not exist yet.
    /// </summary>
    public static class JPathEnsurer
    {
        public static JObject EnsureObject(JToken root, string path, bool isArray)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("JPath must not be empty.", nameof(path));

            var matches = root
                .SelectTokens(path, errorWhenNoMatch: false)
                .Take(2)
                .ToList();

            if (matches.Count > 1)
                throw new InvalidOperationException("The target JPath must resolve to at most one node.");

            var current = matches.SingleOrDefault();
            if (current is JArray targetArray)
            {
                var appendedObject = new JObject();
                targetArray.Add(appendedObject);
                return appendedObject;
            }

            if (current is JObject currentObject)
            {
                if (isArray)
                    throw new InvalidOperationException("The target JPath resolves to an object. For array mode, provide a path to an array property.");

                return currentObject;
            }

            if (current != null)
                throw new InvalidOperationException("The target node for the new object must be an object or array.");

            if (!TryResolveMissingProperty(root, path, out var parent, out var propertyName))
                throw new InvalidOperationException("The target JPath cannot be created at this location.");

            var createdObject = new JObject();
            if (isArray)
                parent[propertyName] = new JArray(createdObject);
            else
                parent[propertyName] = createdObject;

            return createdObject;
        }

        private static bool TryResolveMissingProperty(
            JToken root,
            string path,
            out JObject parent,
            out string propertyName)
        {
            parent = null;
            propertyName = null;

            if (!TrySplitFinalProperty(path, out var parentPath, out propertyName))
                return false;

            var parentMatches = root
                .SelectTokens(parentPath, errorWhenNoMatch: false)
                .Take(2)
                .ToList();

            if (parentMatches.Count != 1 || parentMatches[0] is not JObject parentObject)
                return false;

            parent = parentObject;
            return true;
        }

        private static bool TrySplitFinalProperty(
            string path,
            out string parentPath,
            out string propertyName)
        {
            parentPath = null;
            propertyName = null;

            if (string.IsNullOrWhiteSpace(path) || path == "$")
                return false;

            if (path.EndsWith("]", StringComparison.Ordinal))
            {
                var bracketIndex = path.LastIndexOf('[', path.Length - 1);
                if (bracketIndex >= 0)
                {
                    var selector = path[(bracketIndex + 1)..^1].Trim();
                    if (selector.Length >= 2 &&
                        (selector[0] == '\'' || selector[0] == '"') &&
                        selector[^1] == selector[0])
                    {
                        parentPath = bracketIndex == 0 ? "$" : path[..bracketIndex];
                        propertyName = UnescapeQuotedPropertyName(selector);
                        return !string.IsNullOrWhiteSpace(propertyName);
                    }
                }
            }

            var separatorIndex = path.LastIndexOf('.');
            if (separatorIndex >= 0)
            {
                parentPath = separatorIndex == 0 ? "$" : path[..separatorIndex];
                propertyName = path[(separatorIndex + 1)..];
                return !string.IsNullOrWhiteSpace(propertyName);
            }

            if (!path.StartsWith("$", StringComparison.Ordinal))
            {
                parentPath = "$";
                propertyName = path;
                return true;
            }

            return false;
        }

        private static string UnescapeQuotedPropertyName(string selector)
        {
            if (selector[0] == '"')
                return JsonConvert.DeserializeObject<string>(selector);

            return selector[1..^1]
                .Replace("\\'", "'", StringComparison.Ordinal)
                .Replace("\\\\", "\\", StringComparison.Ordinal);
        }
    }
}
