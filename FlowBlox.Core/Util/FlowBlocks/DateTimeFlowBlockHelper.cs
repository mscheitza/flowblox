using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public static class DateTimeFlowBlockHelper
    {
        public static List<FieldElement> GetDateTimeFieldElements()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFieldElements(true)
                .Where(field => field.FieldType?.FieldType == Enums.FieldTypes.DateTime)
                .ToList();
        }

        public static DateTime ResolveDateTime(FieldElement field, string fieldNameForError)
        {
            if (field == null)
                throw new InvalidOperationException($"Field '{fieldNameForError}' is not set.");

            if (field.FieldType?.FieldType != Enums.FieldTypes.DateTime)
            {
                throw new InvalidOperationException(
                    $"Field '{field.FullyQualifiedName}' must be configured as DateTime.");
            }

            if (field.Value is DateTime dateTime)
                return dateTime;

            throw new InvalidOperationException(
                $"Field '{field.FullyQualifiedName}' does not contain a valid DateTime value.");
        }
    }
}
