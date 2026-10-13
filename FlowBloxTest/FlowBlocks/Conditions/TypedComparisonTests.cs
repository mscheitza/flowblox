using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Parsing;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FlowBloxTest.FlowBlocks.Conditions
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class TypedComparisonTests
    {
        public static IEnumerable<object[]> TypedValues()
        {
            yield return [42, "42", "100"];
            yield return [3000000000L, "3000000000", "10000000000"];
            yield return [2.5f, "2.5", "10.5"];
            yield return [2.5d, "2.5", "10.5"];
            yield return [false, "false", "true"];
            yield return [new DateTime(2026, 10, 9), "2026-10-09", "2026-10-10"];
        }

        private static IValueParsingStrategy GetParsingStrategy(object value)
        {
            if (value is float) return new FloatParsingStrategy(CultureInfo.InvariantCulture.NumberFormat);
            if (value is double) return new DoubleParsingStrategy(CultureInfo.InvariantCulture.NumberFormat);
            if (value is DateTime) return new DateTimeParsingStrategy("yyyy-MM-dd");
            return null;
        }

        [TestMethod]
        [DynamicData(nameof(TypedValues))]
        public void TypedValues_UseTypedEqualityAndOrdering(object left, string equal, string greater)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                var strategy = GetParsingStrategy(left);
                Assert.IsTrue(new ComparisonCondition { Operator = ComparisonOperator.Equals, Value = equal }.Compare(left, strategy));
                Assert.IsFalse(new ComparisonCondition { Operator = ComparisonOperator.NotEquals, Value = equal }.Compare(left, strategy));
                Assert.IsTrue(new ComparisonCondition { Operator = ComparisonOperator.LowerThan, Value = greater }.Compare(left, strategy));
                Assert.IsFalse(new ComparisonCondition { Operator = ComparisonOperator.GreaterThan, Value = greater }.Compare(left, strategy));
                Assert.IsTrue(new ComparisonCondition { Operator = ComparisonOperator.LowerThanOrEquals, Value = equal }.Compare(left, strategy));
                Assert.IsTrue(new ComparisonCondition { Operator = ComparisonOperator.GreaterThanOrEquals, Value = equal }.Compare(left, strategy));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [TestMethod]
        [DynamicData(nameof(TypedValues))]
        public void PresenceChecks_DoNotParseTheRightValue(object left, string equal, string greater)
        {
            Assert.IsTrue(new ComparisonCondition { Operator = ComparisonOperator.HasValue, Value = "invalid" }.Compare(left));
            Assert.IsFalse(new ComparisonCondition { Operator = ComparisonOperator.HasNoValue, Value = null }.Compare(left));
            Assert.IsFalse(new ComparisonCondition { Operator = ComparisonOperator.HasValue }.Compare((long?)null));
            Assert.IsTrue(new ComparisonCondition { Operator = ComparisonOperator.HasNoValue }.Compare((double?)null));
        }

        [TestMethod]
        [DynamicData(nameof(TypedValues))]
        public void InvalidTypedRightValue_ThrowsInsteadOfFallingBackToText(object left, string equal, string greater)
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                new ComparisonCondition { Operator = ComparisonOperator.Equals, Value = "invalid" }.Compare(left, GetParsingStrategy(left)));
            Assert.ThrowsExactly<NotSupportedException>(() =>
                new ComparisonCondition { Operator = ComparisonOperator.Contains, Value = equal }.Compare(left));
        }

        [TestMethod]
        public void TypedComparison_ResolvesRuntimeFieldsBeforeConversion()
        {
            var project = new FlowBloxProject();
            using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);
            var right = project.FlowBloxRegistry.CreateUserField(UserFieldTypes.Memory, fieldName: "Right");
            right.StringValue = "10000000000";
            var condition = new ComparisonCondition { Operator = ComparisonOperator.LowerThan, Value = right.FullyQualifiedName };
            Assert.IsTrue(condition.Compare((long?)3000000000L));
        }

        [TestMethod]
        public void ByteArrayComparison_IsRejectedAtRuntime()
        {
            foreach (var op in Enum.GetValues<ComparisonOperator>())
                Assert.ThrowsExactly<NotSupportedException>(() =>
                    new ComparisonCondition { Operator = op, Value = "AQI=" }.Compare(new byte[] { 1, 2 }));
        }

        [TestMethod]
        [DataRow("en")]
        [DataRow("de")]
        public void ByteArrayField_ProducesLocalizedCustomValidation(string language)
        {
            var previousCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            try
            {
                var field = new FieldElement(FieldTypes.ByteArray) { Name = "Bytes", StringValue = "AQI=" };
                var condition = new FieldComparisonCondition { FieldElement = field };
                var context = new ValidationContext(condition) { MemberName = nameof(FieldComparisonCondition.FieldElement) };
                var results = new List<ValidationResult>();
                Assert.IsFalse(Validator.TryValidateProperty(field, context, results));
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results[0].MemberNames.Contains(nameof(FieldComparisonCondition.FieldElement)));
                Assert.IsTrue(results[0].ErrorMessage?.StartsWith(language == "de" ? "Felder" : "Fields"));

                // Logical conditions share the same core and must reject the same field.
                var logical = new FieldLogicalComparisonCondition { FieldElement = field };
                var logicalContext = new ValidationContext(logical) { MemberName = nameof(FieldLogicalComparisonCondition.FieldElement) };
                Assert.IsFalse(Validator.TryValidateProperty(field, logicalContext, new List<ValidationResult>()));

                var supported = new FieldElement(FieldTypes.Long) { Name = "Number", StringValue = "3000000000" };
                Assert.IsTrue(Validator.TryValidateProperty(supported, context, new List<ValidationResult>()));
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousCulture;
            }
        }

        [TestMethod]
        [DataRow(ComparisonOperator.Contains)]
        [DataRow(ComparisonOperator.NotContains)]
        public void ContainsValidation_RejectsTypedFieldsEvenWhenEmpty_AndAllowsText(ComparisonOperator op)
        {
            var project = new FlowBloxProject();
            using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);
            foreach (var type in new[] { FieldTypes.Integer, FieldTypes.Long, FieldTypes.Float, FieldTypes.Double, FieldTypes.Boolean, FieldTypes.DateTime })
            {
                var field = project.FlowBloxRegistry.CreateUserField(UserFieldTypes.Memory, type);
                var condition = new FieldComparisonCondition { FieldElement = field, Operator = op };
                var context = new ValidationContext(condition) { MemberName = nameof(FieldComparisonCondition.FieldElement) };
                var results = new List<ValidationResult>();
                Assert.IsFalse(Validator.TryValidateProperty(field, context, results), $"Empty {type} must not support {op}.");
                Assert.IsTrue(results[0].MemberNames.Contains(nameof(FieldComparisonCondition.FieldElement)));

                var logical = new FieldLogicalComparisonCondition { FieldElement = field, Operator = op };
                var logicalContext = new ValidationContext(logical) { MemberName = nameof(FieldLogicalComparisonCondition.FieldElement) };
                Assert.IsFalse(Validator.TryValidateProperty(field, logicalContext, new List<ValidationResult>()));

                condition.Operator = ComparisonOperator.Equals;
                Assert.IsTrue(Validator.TryValidateProperty(field, context, new List<ValidationResult>()));
            }

            var text = project.FlowBloxRegistry.CreateUserField(UserFieldTypes.Memory, FieldTypes.Text);
            foreach (var value in new[] { null, "", "text" })
            {
                text.StringValue = value;
                var condition = new FieldComparisonCondition { FieldElement = text, Operator = op };
                var context = new ValidationContext(condition) { MemberName = nameof(FieldComparisonCondition.FieldElement) };
                Assert.IsTrue(Validator.TryValidateProperty(text, context, new List<ValidationResult>()));
            }
        }
    }
}
