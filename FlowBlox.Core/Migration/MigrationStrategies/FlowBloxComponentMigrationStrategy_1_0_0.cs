using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Migration.MigrationStrategies
{
    public class FlowBloxComponentMigrationStrategy_1_0_0 : FlowBloxMigrationStrategyBase, IFlowBloxMigrationStrategy
    {
        public Type ComponentType => typeof(FlowBloxComponent);

        public Version Version => new Version(1, 0, 0, 0);

        private static readonly Regex ListToObservableCollectionRegex = new Regex(
            @"System\.Collections\.Generic\.List`1\[\[(.*?)\]\], System\.Private\.CoreLib",
            RegexOptions.Compiled
        );

        public void Migrate(JObject json)
        {
            IterateThroughObjectsRecursive(json);
        }

        private const string ActivationConditionsPropertyName = "ActivationConditions";

        private const string FieldConditionTypeString =
            "FlowBlox.Core.Models.FlowBlocks.Additions.FieldCondition, FlowBlox.Core";

        private const string LogicalConditionTypeString =
            "FlowBlox.Core.Models.FlowBlocks.Additions.LogicalCondition, FlowBlox.Core";

        private static readonly Regex GenericArgRegex = new Regex(
            @"(System\.Collections\.(?:Generic\.List|ObjectModel\.ObservableCollection)`1\[\[)(.*?)(\]\].*?)$",
            RegexOptions.Compiled
        );

        private static string ReplaceFieldConditionWithLogicalCondition(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                return typeString;

            return GenericArgRegex.Replace(typeString, m =>
            {
                var prefix = m.Groups[1].Value;
                var arg = m.Groups[2].Value;
                var suffix = m.Groups[3].Value;

                // Only rewrite FieldCondition -> LogicalCondition.
                if (string.Equals(arg, FieldConditionTypeString, StringComparison.Ordinal))
                    arg = LogicalConditionTypeString;

                return prefix + arg + suffix;
            });

        }
        /// <summary>
        /// Property-level migration hook:
        /// - Rewrites List<T> type strings to ObservableCollection<T> when the target property is an ObservableCollection.
        /// </summary>
        protected override void OnVisitProperty(JProperty property, Type? parentType, string? parentPropertyName)
        {
            if (property.Name == "$type" && property.Value.Type == JTokenType.String)
            {
                string originalValue = property.Value.ToString();
                string updatedValue = originalValue;

                // Use the parent type and property name (if available) to decide whether a type replacement is required.
                if (parentType != null && parentPropertyName != null)
                {
                    if (ShouldReplaceType(parentType, parentPropertyName, originalValue))
                    {
                        updatedValue = ListToObservableCollectionRegex.Replace(
                            originalValue,
                            match => $"System.Collections.ObjectModel.ObservableCollection`1[[{match.Groups[1].Value}]], System.ObjectModel"
                        );
                    }

                    // ObservableCollection<FieldCondition> -> ObservableCollection<LogicalCondition>
                    // List<FieldCondition> (if present before list->obs) -> List<LogicalCondition> (safe)
                    if (string.Equals(parentPropertyName, ActivationConditionsPropertyName, StringComparison.Ordinal))
                    {
                        updatedValue = ReplaceFieldConditionWithLogicalCondition(updatedValue);
                    }
                }

                if (!string.Equals(originalValue, updatedValue, StringComparison.Ordinal))
                {
                    property.Value = updatedValue;
                }
            }
        }

        /// <summary>
        /// Object-level migration hook:
        /// - Migrates WebRequestFlowBlock.ResultField to WebRequestFlowBlock.ResultFields
        /// </summary>
        protected override void OnVisitObject(Type? currentType, JObject obj)
        {
            // WebRequestFlowBlock.ResultField -> ResultFields migration
            if (currentType != null &&
                WebRequestFlowBlockType != null &&
                WebRequestFlowBlockType.IsAssignableFrom(currentType))
            {
                MigrateWebRequestResultField(obj);
            }
        }

        private const string WebRequestFlowBlockTypeString =
            "FlowBlox.Core.Models.FlowBlocks.WebRequestFlowBlock, FlowBlox.Core";

        private const string ResultFieldByEnumValueTypeString =
            "FlowBlox.Core.Models.FlowBlocks.Base.ResultFieldByEnumValue`1[[FlowBlox.Core.Models.FlowBlocks.WebRequest.WebRequestDestinations, FlowBlox.Core]], FlowBlox.Core";

        private const string ResultFieldsCollectionTypeString =
            "System.Collections.ObjectModel.ObservableCollection`1[[FlowBlox.Core.Models.FlowBlocks.Base.ResultFieldByEnumValue`1[[FlowBlox.Core.Models.FlowBlocks.WebRequest.WebRequestDestinations, FlowBlox.Core]], FlowBlox.Core]], System.ObjectModel";

        private static readonly Type? WebRequestFlowBlockType = Type.GetType(WebRequestFlowBlockTypeString);

        /// <summary>
        /// Migration: WebRequest.ResultField -> ResultFields
        /// </summary>
        private void MigrateWebRequestResultField(JObject obj)
        {
            // If ResultFields already exist, do nothing (idempotent).
            if (obj.Property("ResultFields") != null)
                return;

            var legacyResultFieldProp = obj.Property("ResultField");
            if (legacyResultFieldProp == null)
                return;

            var legacyResultFieldToken = legacyResultFieldProp.Value;
            if (legacyResultFieldToken == null || legacyResultFieldToken.Type == JTokenType.Null)
            {
                // Legacy property is present but has no usable value -> remove and do not migrate.
                legacyResultFieldProp.Remove();
                return;
            }

            // Create the new ResultFields collection wrapper.
            var resultFieldsObj = new JObject
            {
                ["$type"] = ResultFieldsCollectionTypeString
            };

            var valuesArray = new JArray();
            resultFieldsObj["$values"] = valuesArray;

            // Single entry of type ResultFieldByEnumValue<WebRequestDestinations>.
            var entryObj = new JObject
            {
                ["$type"] = ResultFieldByEnumValueTypeString,
                ["EnumValue"] = "Content",
                ["ResultField"] = legacyResultFieldToken.DeepClone()
            };

            valuesArray.Add(entryObj);

            // Add the new ResultFields property.
            obj.Add("ResultFields", resultFieldsObj);

            // Remove the legacy ResultField property.
            legacyResultFieldProp.Remove();
        }

        /// <summary>
        /// Determines whether the given JSON $type value should be transformed from List&lt;T&gt; to ObservableCollection&lt;T&gt; for the specified CLR property.
        /// </summary>
        private bool ShouldReplaceType(Type parentType, string propertyName, string jsonTypeValue)
        {
            var prop = parentType.GetProperty(propertyName);
            if (prop == null)
                return false;

            var propType = prop.PropertyType;
            if (!propType.IsGenericType)
                return false;

            var isObservableCollection =
                propType.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ObservableCollection<>);

            if (!isObservableCollection)
                return false;

            return true;
        }
    }
}
