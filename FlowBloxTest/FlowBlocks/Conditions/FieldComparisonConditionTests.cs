using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBloxTest.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FlowBloxTest.FlowBlocks.Conditions
{
    [TestClass]
    public class FieldComparisonConditionTests
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            _project = project;
        }

        private static FieldComparisonCondition CreateCondition(FieldElement field, ComparisonOperator op, string compareValue)
        {
            return new FieldComparisonCondition
            {
                FieldElement = field,
                Operator = op,
                Value = compareValue
            };
        }

        private static void AssertCompareTrue(FieldElement field, ComparisonOperator op, string compareValue)
        {
            var c = CreateCondition(field, op, compareValue);
            Assert.IsTrue(c.Compare(), $"Expected TRUE for {field.Name} ({field.StringValue}) {op} '{compareValue}'");
        }

        private static void AssertCompareFalse(FieldElement field, ComparisonOperator op, string compareValue)
        {
            var c = CreateCondition(field, op, compareValue);
            Assert.IsFalse(c.Compare(), $"Expected FALSE for {field.Name} ({field.StringValue}) {op} '{compareValue}'");
        }

        [TestMethod]
        public void Text_Equals_And_NotEquals()
        {
            var f = FieldMockCreator.CreateStringField("Text", "Hello");

            AssertCompareTrue(f, ComparisonOperator.Equals, "Hello");
            AssertCompareFalse(f, ComparisonOperator.Equals, "hello");

            AssertCompareTrue(f, ComparisonOperator.NotEquals, "hello");
            AssertCompareFalse(f, ComparisonOperator.NotEquals, "Hello");
        }

        [TestMethod]
        public void Text_Contains_And_NotContains()
        {
            var f = FieldMockCreator.CreateStringField("Text", "Hello World");

            AssertCompareTrue(f, ComparisonOperator.Contains, "World");
            AssertCompareFalse(f, ComparisonOperator.Contains, "WORLD");

            AssertCompareTrue(f, ComparisonOperator.NotContains, "WORLD");
            AssertCompareFalse(f, ComparisonOperator.NotContains, "World");
        }

        [TestMethod]
        public void Text_HasValue_And_HasNoValue()
        {
            var f1 = FieldMockCreator.CreateStringField("Text1", "X");
            var f2 = FieldMockCreator.CreateStringField("Text2", "");
            var f3 = FieldMockCreator.CreateStringField("Text3", null);

            AssertCompareTrue(f1, ComparisonOperator.HasValue, "");
            AssertCompareFalse(f1, ComparisonOperator.HasNoValue, "");

            AssertCompareFalse(f2, ComparisonOperator.HasValue, "");
            AssertCompareTrue(f2, ComparisonOperator.HasNoValue, "");

            AssertCompareFalse(f3, ComparisonOperator.HasValue, "");
            AssertCompareTrue(f3, ComparisonOperator.HasNoValue, "");
        }

        [TestMethod]
        public void Text_RegexIsTrue_And_RegexIsFalse()
        {
            var f = FieldMockCreator.CreateStringField("Text", "ABC-123");

            AssertCompareTrue(f, ComparisonOperator.RegexIsTrue, @"^[A-Z]{3}-\d{3}$");
            AssertCompareFalse(f, ComparisonOperator.RegexIsTrue, @"^\d+$");

            AssertCompareTrue(f, ComparisonOperator.RegexIsFalse, @"^\d+$");
            AssertCompareFalse(f, ComparisonOperator.RegexIsFalse, @"^[A-Z]{3}-\d{3}$");
        }

        [TestMethod]
        public void Text_StringCompare_Operators()
        {
            var f = FieldMockCreator.CreateStringField("Text", "B");

            AssertCompareTrue(f, ComparisonOperator.GreaterThan, "A");
            AssertCompareFalse(f, ComparisonOperator.GreaterThan, "C");

            AssertCompareTrue(f, ComparisonOperator.LowerThan, "C");
            AssertCompareFalse(f, ComparisonOperator.LowerThan, "A");

            AssertCompareTrue(f, ComparisonOperator.GreaterThanOrEquals, "B");
            AssertCompareTrue(f, ComparisonOperator.GreaterThanOrEquals, "A");
            AssertCompareFalse(f, ComparisonOperator.GreaterThanOrEquals, "C");

            AssertCompareTrue(f, ComparisonOperator.LowerThanOrEquals, "B");
            AssertCompareTrue(f, ComparisonOperator.LowerThanOrEquals, "C");
            AssertCompareFalse(f, ComparisonOperator.LowerThanOrEquals, "A");
        }

        [TestMethod]
        public void DateTime_IsoStringCompare_GreaterLowerEquals()
        {
            var f = FieldMockCreator.CreateDateTimeField("Date", new DateTime(2025, 01, 15), format: "yyyy-MM-dd");

            AssertCompareTrue(f, ComparisonOperator.Equals, "2025-01-15");
            AssertCompareFalse(f, ComparisonOperator.Equals, "2025-01-16");

            AssertCompareTrue(f, ComparisonOperator.GreaterThan, "2025-01-01");
            AssertCompareFalse(f, ComparisonOperator.GreaterThan, "2025-02-01");

            AssertCompareTrue(f, ComparisonOperator.LowerThan, "2025-12-31");
            AssertCompareFalse(f, ComparisonOperator.LowerThan, "2024-12-31");
        }

        [TestMethod]
        public void Integer_FixedWidthStringCompare_GreaterLowerEquals()
        {
            var f = FieldMockCreator.CreateIntField("Int", 42);

            AssertCompareTrue(f, ComparisonOperator.Equals, "42");
            AssertCompareFalse(f, ComparisonOperator.Equals, "43");

            AssertCompareTrue(f, ComparisonOperator.GreaterThan, "41");
            AssertCompareFalse(f, ComparisonOperator.GreaterThan, "100");

            AssertCompareTrue(f, ComparisonOperator.LowerThan, "100");
            AssertCompareFalse(f, ComparisonOperator.LowerThan, "1");
        }
    }
}
