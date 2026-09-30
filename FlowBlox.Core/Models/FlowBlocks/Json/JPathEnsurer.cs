using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    /// <summary>
    /// Resolves a unique target through Newtonsoft JPath. Missing chains of
    /// simple object properties are created automatically.
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
                if (!isArray)
                    throw new InvalidOperationException("The target JPath resolves to an array. Enable array mode to append an object.");

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

            if (TryResolveMissingProperty(root, path, out var parent, out var propertyName))
                return CreateTarget(parent, propertyName, isArray);

            return CreateMissingObjectPath(root, path, isArray);
        }

        private static JObject CreateTarget(JObject parent, string propertyName, bool isArray)
        {
            var createdObject = new JObject();
            if (isArray)
                parent[propertyName] = new JArray(createdObject);
            else
                parent[propertyName] = createdObject;

            return createdObject;
        }

        private static JObject CreateMissingObjectPath(JToken root, string path, bool isArray)
        {
            if (root is not JObject rootObject)
                throw new InvalidOperationException("A missing JPath can only be created below a JSON object.");

            var propertyNames = ParseSimpleObjectPath(path);
            if (propertyNames.Count == 0)
                throw new InvalidOperationException("The target JPath cannot be created at this location.");

            var parent = rootObject;
            foreach (var propertyName in propertyNames.Take(propertyNames.Count - 1))
            {
                if (!parent.TryGetValue(propertyName, out var child))
                {
                    var createdParent = new JObject();
                    parent[propertyName] = createdParent;
                    parent = createdParent;
                    continue;
                }

                if (child is not JObject childObject)
                    throw new InvalidOperationException(
                        $"The JPath segment '{propertyName}' must resolve to an object before child properties can be created.");

                parent = childObject;
            }

            return CreateTarget(parent, propertyNames[^1], isArray);
        }

        private static IReadOnlyList<string> ParseSimpleObjectPath(string path)
        {
            var propertyNames = new List<string>();
            var value = path.Trim();
            var index = 0;

            if (value.StartsWith("$", StringComparison.Ordinal))
                index++;

            while (index < value.Length)
            {
                if (value[index] == '.')
                {
                    index++;
                    var propertyStart = index;
                    while (index < value.Length && value[index] != '.' && value[index] != '[')
                        index++;

                    var propertyName = value[propertyStart..index];
                    if (string.IsNullOrWhiteSpace(propertyName) || ContainsUnsupportedSelectorSyntax(propertyName))
                        throw CreateUnsupportedPathException();

                    propertyNames.Add(propertyName);
                    continue;
                }

                if (value[index] == '[')
                {
                    propertyNames.Add(ParseQuotedProperty(value, ref index));
                    continue;
                }

                if (index == 0)
                {
                    var propertyStart = index;
                    while (index < value.Length && value[index] != '.' && value[index] != '[')
                        index++;

                    var propertyName = value[propertyStart..index];
                    if (string.IsNullOrWhiteSpace(propertyName) || ContainsUnsupportedSelectorSyntax(propertyName))
                        throw CreateUnsupportedPathException();

                    propertyNames.Add(propertyName);
                    continue;
                }

                throw CreateUnsupportedPathException();
            }

            return propertyNames;
        }

        private static string ParseQuotedProperty(string path, ref int index)
        {
            var cursor = index + 1;
            while (cursor < path.Length && char.IsWhiteSpace(path[cursor]))
                cursor++;

            if (cursor >= path.Length || (path[cursor] != '\'' && path[cursor] != '"'))
                throw CreateUnsupportedPathException();

            var quote = path[cursor];
            var selectorStart = cursor;
            cursor++;
            var escaped = false;
            while (cursor < path.Length)
            {
                var character = path[cursor];
                if (!escaped && character == quote)
                    break;

                escaped = !escaped && character == '\\';
                if (character != '\\')
                    escaped = false;
                cursor++;
            }

            if (cursor >= path.Length)
                throw CreateUnsupportedPathException();

            var selector = path[selectorStart..(cursor + 1)];
            cursor++;
            while (cursor < path.Length && char.IsWhiteSpace(path[cursor]))
                cursor++;

            if (cursor >= path.Length || path[cursor] != ']')
                throw CreateUnsupportedPathException();

            index = cursor + 1;
            var propertyName = UnescapeQuotedPropertyName(selector);
            if (string.IsNullOrWhiteSpace(propertyName))
                throw CreateUnsupportedPathException();

            return propertyName;
        }

        private static bool ContainsUnsupportedSelectorSyntax(string propertyName)
        {
            return propertyName.IndexOfAny(['*', '?', '(', ')', '@', ']', ':']) >= 0;
        }

        private static InvalidOperationException CreateUnsupportedPathException()
        {
            return new InvalidOperationException(
                "The missing JPath cannot be created automatically because it contains an array selector, filter, wildcard, or recursive selector. Create that structure before executing the writer.");
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
