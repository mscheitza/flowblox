using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBloxTest.FlowBlocks.Base
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBlockOutputConfigurationTests : FlowBloxTestsBase
    {
        private sealed class DatasetSourceFlowBlock : BaseResultFlowBlock
        {
            public List<FieldElement> ResultFields { get; } = [];

            public List<string> Values { get; } = [];

            public override List<FieldElement> Fields => ResultFields;

            public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

            public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

            public override bool Execute(BaseRuntime runtime, object data)
            {
                return Invoke(runtime, data, () => GenerateResult(runtime, Values));
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
        }

        [TestMethod]
        public void RangeWithStartValue_ReturnsRecordsStartingAtMatchingValue()
        {
            var source = CreateSource("alpha", "beta", "gamma");
            source.OutputBehavior = OutputBehavior.Range;
            source.StartValue = "beta";

            CreateRuntimeAndExecute(FlowBloxProjectManager.Instance.ActiveProject);

            CollectionAssert.AreEqual(
                new[] { "beta", "gamma" },
                GetGeneratedValues(source));
        }

        [TestMethod]
        public void RangeWithStartIndex_ReturnsRecordsStartingAtIndex()
        {
            var source = CreateSource("alpha", "beta", "gamma");
            source.StartIndex = 1;

            CreateRuntimeAndExecute(FlowBloxProjectManager.Instance.ActiveProject);

            CollectionAssert.AreEqual(new[] { "beta", "gamma" }, GetGeneratedValues(source));
        }

        [TestMethod]
        public void RangeWithEndIndex_ReturnsRecordsThroughIndex()
        {
            var source = CreateSource("alpha", "beta", "gamma");
            source.EndIndex = 1;

            CreateRuntimeAndExecute(FlowBloxProjectManager.Instance.ActiveProject);

            CollectionAssert.AreEqual(new[] { "alpha", "beta" }, GetGeneratedValues(source));
        }

        [TestMethod]
        public void RangeWithStartAndEndIndex_ReturnsConfiguredRange()
        {
            var source = CreateSource("alpha", "beta", "gamma", "delta");
            source.StartIndex = 1;
            source.EndIndex = 2;

            CreateRuntimeAndExecute(FlowBloxProjectManager.Instance.ActiveProject);

            CollectionAssert.AreEqual(new[] { "beta", "gamma" }, GetGeneratedValues(source));
        }

        [TestMethod]
        public void RangeWithStartValue_DoesNotReturnAllRecordsWhenValueIsMissing()
        {
            var source = CreateSource("alpha", "beta", "gamma");
            source.OutputBehavior = OutputBehavior.Range;
            source.StartValue = "missing";
            string warning = null;
            source.OnWarn += (_, message) => warning = message;

            CreateRuntimeAndExecute(FlowBloxProjectManager.Instance.ActiveProject);

            CollectionAssert.AreEqual(
                new string[] { null },
                GetGeneratedValues(source));
            Assert.AreEqual(FlowBloxTexts.BaseResultFlowBlockNotifications_StartValueNotFound, warning);
            Assert.AreEqual(
                NotificationType.Warning,
                source.GetCurrentNotificationType(BaseResultFlowBlock.BaseResultFlowBlockNotifications.StartValueNotFound));
        }

        [TestMethod]
        [DataRow(3, null)]
        [DataRow(null, 3)]
        [DataRow(2, 1)]
        [DataRow(1, 1)]
        public void RangeWithInvalidIndices_CreatesWarningAndReturnsNoRecords(int? startIndex, int? endIndex)
        {
            var source = CreateSource("alpha", "beta", "gamma");
            source.OutputBehavior = OutputBehavior.Range;
            source.StartIndex = startIndex;
            source.EndIndex = endIndex;
            string warning = null;
            source.OnWarn += (_, message) => warning = message;

            var warningLogs = ExecuteAndCaptureWarnings(source);

            CollectionAssert.AreEqual(new string[] { null }, GetGeneratedValues(source));
            Assert.AreEqual(FlowBloxTexts.BaseResultFlowBlockNotifications_IndexOutputUnavailable, warning);
            Assert.AreEqual(
                NotificationType.Warning,
                source.GetCurrentNotificationType(BaseResultFlowBlock.BaseResultFlowBlockNotifications.IndexOutputUnavailable));

            if (startIndex is < 0 or >= 3)
            {
                CollectionAssert.Contains(
                    warningLogs,
                    string.Format(FlowBloxTexts.BaseResultFlowBlock_Warning_StartIndexOutsideResultRange, startIndex, 3));
            }

            if (endIndex is < 0 or >= 3)
            {
                CollectionAssert.Contains(
                    warningLogs,
                    string.Format(FlowBloxTexts.BaseResultFlowBlock_Warning_EndIndexOutsideResultRange, endIndex, 3));
            }

            if (startIndex.HasValue && endIndex.HasValue && startIndex.Value >= endIndex.Value)
            {
                CollectionAssert.Contains(
                    warningLogs,
                    string.Format(FlowBloxTexts.BaseResultFlowBlock_Warning_IndexOrderInvalid, endIndex, startIndex));
            }
        }

        [TestMethod]
        public void ValidateOutputBehavior_RejectsRangeWithoutCriterion()
        {
            var source = CreateSource("alpha");

            var result = ValidateRange(source);

            Assert.AreNotEqual(ValidationResult.Success, result);
        }

        [TestMethod]
        public void ValidateOutputBehavior_RejectsMixedStartValueAndIndexModes()
        {
            var source = CreateSource("alpha");
            source.StartValue = "alpha";
            source.StartIndex = 0;

            var result = ValidateRange(source);

            Assert.AreNotEqual(ValidationResult.Success, result);
        }

        [TestMethod]
        [DataRow(2, 1)]
        [DataRow(1, 1)]
        public void ValidateOutputBehavior_RejectsNonIncreasingIndexRange(int startIndex, int endIndex)
        {
            var source = CreateSource("alpha", "beta", "gamma");
            source.StartIndex = startIndex;
            source.EndIndex = endIndex;

            var result = ValidateRange(source);

            Assert.AreNotEqual(ValidationResult.Success, result);
        }

        [TestMethod]
        [DataRow(nameof(BaseResultFlowBlock.StartIndex))]
        [DataRow(nameof(BaseResultFlowBlock.EndIndex))]
        public void RangeIndices_RejectNegativeValues(string propertyName)
        {
            var source = CreateSource("alpha");
            var property = typeof(BaseResultFlowBlock).GetProperty(propertyName);
            Assert.IsNotNull(property);
            property.SetValue(source, -1);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateProperty(
                property.GetValue(source),
                new ValidationContext(source) { MemberName = propertyName },
                results);

            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public void RangeInputs_ExposeOnlyTheSelectedConfigurationMode()
        {
            var source = CreateSource("alpha");
            source.OutputBehavior = OutputBehavior.Range;

            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.StartValue)));
            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.StartIndex)));
            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.EndIndex)));

            source.StartValue = "alpha";

            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.StartValue)));
            Assert.IsFalse(IsActive(source, nameof(BaseResultFlowBlock.StartIndex)));
            Assert.IsFalse(IsActive(source, nameof(BaseResultFlowBlock.EndIndex)));

            source.StartValue = null;
            source.StartIndex = 0;

            Assert.IsFalse(IsActive(source, nameof(BaseResultFlowBlock.StartValue)));
            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.StartIndex)));
            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.EndIndex)));
        }

        [TestMethod]
        public void RangeInputs_KeepBothModesVisibleForConflictingLegacyConfiguration()
        {
            var source = CreateSource("alpha");
            source.StartValue = "alpha";
            source.StartIndex = 0;

            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.StartValue)));
            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.StartIndex)));
            Assert.IsTrue(IsActive(source, nameof(BaseResultFlowBlock.EndIndex)));
        }

        private DatasetSourceFlowBlock CreateSource(params string[] values)
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var source = CreateFlowBlock<DatasetSourceFlowBlock>(start);
            source.ResultFields.Add(Registry.CreateField(source));
            source.Values.AddRange(values);
            return source;
        }

        private static string[] GetGeneratedValues(DatasetSourceFlowBlock source)
        {
            return source.GridElementResult.Results
                .Select(x => x.FieldValueMappings.Single().Value)
                .ToArray();
        }

        private static List<string> ExecuteAndCaptureWarnings(DatasetSourceFlowBlock source)
        {
            var warningLogs = new List<string>();
            var runtime = new FlowBloxUnitTestRuntime(FlowBloxProjectManager.Instance.ActiveProject);
            runtime.LogMessageCreated += (_, message, level) =>
            {
                if (level == FlowBloxLogLevel.Warning)
                    warningLogs.Add(message);
            };
            source.Execute(runtime, null);
            return warningLogs;
        }

        private static bool IsActive(BaseResultFlowBlock source, string propertyName)
        {
            var property = typeof(BaseResultFlowBlock).GetProperty(propertyName);
            Assert.IsNotNull(property);
            var activationCondition = property.GetCustomAttribute<ActivationConditionAttribute>();
            Assert.IsNotNull(activationCondition);
            return activationCondition.IsActive(source);
        }

        private static ValidationResult ValidateRange(DatasetSourceFlowBlock source)
        {
            source.OutputBehavior = OutputBehavior.Range;
            return BaseResultFlowBlock.ValidateOutputBehavior(
                source.OutputBehavior,
                new ValidationContext(source)
                {
                    MemberName = nameof(BaseResultFlowBlock.OutputBehavior)
                });
        }
    }
}
