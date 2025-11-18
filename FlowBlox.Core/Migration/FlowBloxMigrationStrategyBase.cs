using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Migration
{
    /// <summary>
    /// Base migration strategy that provides a recursive traversal over a JSON object tree
    /// and exposes hooks for per-object and per-property migration logic.
    /// </summary>
    public abstract class FlowBloxMigrationStrategyBase
    {
        /// <summary>
        /// Recursively iterates through all objects and arrays in the JSON token tree,
        /// resolving the current CLR type (if possible) and invoking the hook methods.
        /// </summary>
        /// <param name="token">Current JSON token to process.</param>
        /// <param name="parentType">CLR type of the parent object, if known.</param>
        /// <param name="parentPropertyName">Name of the property on the parent that holds the current token, if known.</param>
        protected void IterateThroughObjectsRecursive(JToken token, Type? parentType = null, string? parentPropertyName = null)
        {
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;

                Type? currentType = null;

                // Determine the current CLR type of the object (if available via $type).
                if (obj.TryGetValue("$type", out var typeToken) && typeToken.Type == JTokenType.String)
                {
                    var typeName = typeToken.ToString();
                    currentType = TypeNameHelper.FindTypeByFullOrSimpleName(typeName);
                }

                // Allow subclasses to apply object-level migration logic.
                OnVisitObject(currentType, obj);

                foreach (var property in obj.Properties())
                {
                    // Allow subclasses to apply property-level migration logic.
                    OnVisitProperty(property, parentType, parentPropertyName);

                    if (obj.TryGetValue("$values", out var valuesToken) && valuesToken.Type == JTokenType.Array)
                    {
                        foreach (var item in (JArray)valuesToken)
                        {
                            IterateThroughObjectsRecursive(item);
                        }
                    }
                    else
                    {
                        // Recurse into child tokens.
                        IterateThroughObjectsRecursive(property.Value, currentType, property.Name);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var item in (JArray)token)
                {
                    IterateThroughObjectsRecursive(item);
                }
            }
        }

        /// <summary>
        /// Hook that is called for each JSON object, after its CLR type has been resolved (if possible).
        /// </summary>
        /// <param name="currentType">The resolved CLR type for the current object, or null if unknown.</param>
        /// <param name="obj">The JSON object.</param>
        protected virtual void OnVisitObject(Type? currentType, JObject obj)
        {
        }

        /// <summary>
        /// Hook that is called for each JSON property before recursing into its value.
        /// </summary>
        /// <param name="property">The current JSON property.</param>
        /// <param name="parentType">The CLR type of the parent object containing this property, if known.</param>
        /// <param name="parentPropertyName">The name of the parent property for the current object, if known.</param>
        protected virtual void OnVisitProperty(JProperty property, Type? parentType, string? parentPropertyName)
        {
        }
    }
}
