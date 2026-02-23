using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using System;

namespace FlowBloxTest.Helper
{
    public static class FieldMockCreator
    {
        private static FlowBloxRegistry GetRegistry()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();

            if (registry == null)
                throw new InvalidOperationException("No active FlowBloxRegistry available. Ensure a project is initialized before creating field mocks.");

            return registry;
        }

        public static FieldElement CreateStringField(string name, string value)
        {
            var registry = GetRegistry();

            var field = registry.CreateUserField(UserFieldTypes.Memory, fieldName: name);
            field.StringValue = value;

            field.FieldType ??= new TypeElement();
            field.FieldType.FieldType = FieldTypes.Text;

            return field;
        }

        public static FieldElement CreateIntField(string name, int? value)
        {
            var registry = GetRegistry();

            var field = registry.CreateUserField(UserFieldTypes.Memory, fieldName: name);
            field.StringValue = value?.ToString() ?? string.Empty;

            field.FieldType ??= new TypeElement();
            field.FieldType.FieldType = FieldTypes.Integer;

            return field;
        }

        public static FieldElement CreateDateTimeField(string name, DateTime value, string format = "yyyy-MM-dd")
        {
            var registry = GetRegistry();

            var field = registry.CreateUserField(UserFieldTypes.Memory, fieldName: name);
            field.StringValue = value.ToString(format);

            field.FieldType ??= new TypeElement();
            field.FieldType.FieldType = FieldTypes.DateTime;
            field.FieldType.DateFormat = format;

            return field;
        }
    }
}
