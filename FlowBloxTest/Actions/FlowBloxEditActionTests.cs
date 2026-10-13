using FlowBlox.Core.Actions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Logic;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.DeepCopier;

namespace FlowBloxTest.Actions
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FlowBloxEditActionTests
    {
        [TestMethod]
        public void UndoRedo_FormatEdit_PreservesDecisionFieldAndSourceAcrossRepeatedRestores()
        {
            var project = new FlowBloxProject();
            using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);
            var format = new FormatFlowBlock { Name = "Format_NoJobs", FormatExpression = "before" };
            var field = new FieldElement(format) { Name = "NoJobs" };
            format.ResultField = field;
            project.FlowBloxRegistry.RegisterFlowBlock(format);
            var decision = new DecisionFlowBlock { Name = "Decision_0" };
            decision.Decisions.Add(new FieldComparisonCondition { FieldElement = field });
            var action = new FlowBloxEditAction { Target = format };
            action.CaptureRestoreState();

            // Exercise the same working-copy / recopy path used by Save and Apply.
            var copier = new DynamicDeepCopier(FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(format));
            var workingCopy = (FormatFlowBlock)copier.Copy(format);
            workingCopy.FormatExpression = "after";
            workingCopy.ResultField.Name = "Changed";
            copier.PropertyActions = FlowBloxDeepCopyStrategy.Instance.GetDeepCopyActions(workingCopy);
            copier.SpecialMode = DeepCopierSpecialMode.Recopy;
            copier.Copy(workingCopy, format);
            action.CaptureRepetitionState();

            for (var cycle = 0; cycle < 3; cycle++)
            {
                action.Undo();
                Assert.AreEqual("before", format.FormatExpression);
                Assert.AreEqual("NoJobs", field.Name);
                Assert.AreSame(field, format.ResultField);
                Assert.AreSame(format, field.Source);
                Assert.AreSame(format.ResultField, decision.Decisions[0].FieldElement);
                Assert.IsTrue(project.FlowBloxRegistry.GetFieldElements().Contains(format.ResultField));

                action.Invoke();
                Assert.AreEqual("after", format.FormatExpression);
                Assert.AreEqual("Changed", field.Name);
                Assert.AreSame(field, format.ResultField);
                Assert.AreSame(format, field.Source);
                Assert.AreSame(format.ResultField, decision.Decisions[0].FieldElement);
            }
        }

        [TestMethod]
        public void UndoRedo_DecisionEdit_RestoresRulesWithoutCopyingExternalFields()
        {
            var project = new FlowBloxProject();
            using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);
            var format = new FormatFlowBlock { Name = "Format_NoJobs" };
            format.ResultField = new FieldElement(format) { Name = "NoJobs" };
            var decision = new DecisionFlowBlock { Name = "Decision_0" };
            decision.ResultField = new FieldElement(decision) { Name = "Field0" };
            var ownField = decision.ResultField;
            decision.ReferencedFlowBlocks.Add(format);
            decision.Decisions.Add(new FieldComparisonCondition { FieldElement = format.ResultField, OutputValue = "before" });
            var action = new FlowBloxEditAction { Target = decision };
            action.CaptureRestoreState();
            decision.Decisions[0].OutputValue = "after";
            action.CaptureRepetitionState();

            for (var cycle = 0; cycle < 3; cycle++)
            {
                action.Undo();
                Assert.AreEqual("before", decision.Decisions[0].OutputValue);
                Assert.AreSame(format.ResultField, decision.Decisions[0].FieldElement);
                Assert.AreSame(format, decision.ReferencedFlowBlocks.Single());
                Assert.AreSame(ownField, decision.ResultField);
                Assert.AreSame(decision, ownField.Source);
                action.Invoke();
                Assert.AreEqual("after", decision.Decisions[0].OutputValue);
                Assert.AreSame(format.ResultField, decision.Decisions[0].FieldElement);
                Assert.AreSame(ownField, decision.ResultField);
                Assert.AreSame(decision, ownField.Source);
            }
        }
    }
}
