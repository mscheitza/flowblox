using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlowBlox.Core.Migration.MigrationStrategies
{
    public class FlowBloxComponentMigrationStrategy_1_0_0 : IFlowBloxMigrationStrategy
    {
        public Type ComponentType => typeof(FlowBloxComponent);

        public Version Version => new Version(1, 0, 0, 0);

        private static readonly Regex ListToObservableCollectionRegex = new Regex(
            @"System\.Collections\.Generic\.List`1\[\[(.*?)\]\], System\.Private\.CoreLib",
            RegexOptions.Compiled
        );

        private static readonly Regex TypeAndAssemblyFromTypeStringRegex = new Regex(@"^(?<type>[^,]+),\s*(?<assembly>.+)$", RegexOptions.Compiled);

        public void Migrate(JObject json)
        {
            ReplaceListWithObservableCollection(json);
        }

        private void ReplaceListWithObservableCollection(JToken token, Type? parentType = null, string? propertyName = null)
        {
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;

                Type currentType = null;

                // Aktuellen Typ am Objekt ermitteln
                if (obj.TryGetValue("$type", out var typeToken) && typeToken.Type == JTokenType.String)
                {
                    var typeName = typeToken.ToString();
                    var typeMatch = TypeAndAssemblyFromTypeStringRegex.Match(typeName);
                    if (typeMatch.Success)
                    {
                        var typeStr = typeMatch.Groups["type"].Value;
                        var assemblyStr = typeMatch.Groups["assembly"].Value;
                        currentType = Type.GetType($"{typeStr}, {assemblyStr}");
                    }
                }

                foreach (var property in obj.Properties())
                {
                    if (property.Name == "$type" && property.Value.Type == JTokenType.String)
                    {
                        string originalValue = property.Value.ToString();

                        // Hole den aktuellen Property-Namen vom übergeordneten Objekt (falls bekannt)
                        if (parentType != null && propertyName != null)
                        {
                            if (ShouldReplaceType(parentType, propertyName, originalValue))
                            {
                                string updatedValue = ListToObservableCollectionRegex.Replace(
                                    originalValue,
                                    match => $"System.Collections.ObjectModel.ObservableCollection`1[[{match.Groups[1].Value}]], System.ObjectModel"
                                );

                                if (originalValue != updatedValue)
                                {
                                    property.Value = updatedValue;
                                }
                            }
                        }
                    }

                    if (obj.TryGetValue("$values", out var valuesToken) && valuesToken.Type == JTokenType.Array)
                    {
                        foreach (var item in (JArray)valuesToken)
                        {
                            ReplaceListWithObservableCollection(item);
                        }
                    }
                    else
                    {
                        // Rekursiv weitergehen
                        ReplaceListWithObservableCollection(property.Value, currentType, property.Name);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var item in (JArray)token)
                {
                    ReplaceListWithObservableCollection(item);
                }
            }
        }

        private bool ShouldReplaceType(Type parentType, string propertyName, string jsonTypeValue)
        {
            var prop = parentType.GetProperty(propertyName);
            if (prop == null) 
                return false;

            var propType = prop.PropertyType;
            if (!propType.IsGenericType) 
                return false;

            var isObservableCollection = propType.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ObservableCollection<>);

            if (!isObservableCollection) 
                return false;

            return true;
        }
    }
}
