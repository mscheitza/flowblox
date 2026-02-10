using System.Reflection;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Util
{
    public static class TypeNameHelper
    {
        public static string GetSimpleTypeName(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return fullTypeName;

            var lastDot = fullTypeName.LastIndexOf('.');
            if (lastDot < 0 || lastDot == fullTypeName.Length - 1)
                return fullTypeName;

            return fullTypeName.Substring(lastDot + 1);
        }

        public static Type? FindTypeBySimpleName(Assembly assembly, string simpleTypeName)
        {
            if (string.IsNullOrWhiteSpace(simpleTypeName))
                return null;

            return assembly
                .GetTypes()
                .FirstOrDefault(t =>
                    string.Equals(t.Name, simpleTypeName, StringComparison.Ordinal));
        }

        private static readonly Regex TypeAndAssemblyFromTypeStringRegex = new Regex(@"^(?<type>[^,]+),\s*(?<assembly>.+)$", RegexOptions.Compiled);
        public static Type FindTypeByFullOrSimpleName(string typeName)
        {
            var typeMatch = TypeAndAssemblyFromTypeStringRegex.Match(typeName);
            if (!typeMatch.Success)
                return default;

            Type resolvedType = null;

            var typeStr = typeMatch.Groups["type"].Value;
            var assemblyStr = typeMatch.Groups["assembly"].Value;
            resolvedType = Type.GetType($"{typeStr}, {assemblyStr}");

            if (resolvedType == null)
            {
                var assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyStr);

                if (assembly != null)
                {
                    var bySimpleName = FindTypeBySimpleName(assembly, GetSimpleTypeName(typeStr));
                    if (bySimpleName != null)
                        resolvedType = bySimpleName;
                }
            }

            return resolvedType;
        }
    }
}
