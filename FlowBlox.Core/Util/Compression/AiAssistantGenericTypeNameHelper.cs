namespace FlowBlox.Core.Util.Compression
{
    public static class AiAssistantGenericTypeNameHelper
    {
        public static string GetCompactTypeName(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return GetCompactTypeName(nullableType) + "?";

            if (type.IsArray)
                return GetCompactTypeName(type.GetElementType()!) + "[]";

            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                var genericName = genericType.FullName ?? genericType.Name;
                var aritySeparatorIndex = genericName.IndexOf('`');
                if (aritySeparatorIndex >= 0)
                    genericName = genericName[..aritySeparatorIndex];

                genericName = AiAssistantTypeAliasHelper.CompressTypeName(genericName);
                var genericArguments = string.Join(",", type.GetGenericArguments().Select(GetCompactTypeName));
                return $"{genericName}<{genericArguments}>";
            }

            return AiAssistantTypeAliasHelper.CompressTypeName(type.FullName ?? type.Name);
        }
    }
}
