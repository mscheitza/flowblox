using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.Base.DatasetSelection;
using FlowBloxTest.DatasetSelection;
using NSubstitute;
using System.Collections.ObjectModel;

namespace FlowBloxTest.Components
{
    [TestClass]
    public class FlowBlockRowWiseDatasetSelectorTests : FlowBlockDatasetSelectorTestBase
    {
        [TestMethod]
        public void RowWise_WithThreeInputsAndThreeRowsEach_ReturnsThreeCombinedRows()
        {
            // Arrange: 3 FlowBlocks, all RowWise, each with 3 records, each with exactly one field

            var fbA = Substitute.For<BaseResultFlowBlock>();
            fbA.Name.Returns("FB_A");

            var fbB = Substitute.For<BaseResultFlowBlock>();
            fbB.Name.Returns("FB_B");

            var fbC = Substitute.For<BaseResultFlowBlock>();
            fbC.Name.Returns("FB_C");

            var fbTarget = Substitute.For<BaseFlowBlock>();
            fbTarget.Name.Returns("Target");
            fbTarget.ReferencedFlowBlocks.Returns(new ObservableCollection<BaseFlowBlock> { fbA, fbB, fbC });

            var fbAField = Substitute.For<FieldElement>();
            fbAField.Name.Returns("FB_A_Field");
            fbAField.Source.Returns(fbA);

            var fbBField = Substitute.For<FieldElement>();
            fbBField.Name.Returns("FB_B_Field");
            fbBField.Source.Returns(fbB);

            var fbCField = Substitute.For<FieldElement>();
            fbCField.Name.Returns("FB_C_Field");
            fbCField.Source.Returns(fbC);

            var preceding = GetPrecedingFieldValues(fbAField, "Pre_A");

            var fbAOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A3", PrecedingFieldValues = preceding } } },
            };

            var fbBOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B3", PrecedingFieldValues = preceding } } },
            };

            var fbCOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C3", PrecedingFieldValues = preceding } } },
            };

            var passedResults = new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>
            {
                { fbA, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbAOuts } } },
                { fbB, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbBOuts } } },
                { fbC, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbCOuts } } },
            };

            var inputAssignments = new List<InputBehaviorAssignment>
            {
                new InputBehaviorAssignment { FlowBlock = fbA, Behavior = InputBehavior.RowWise },
                new InputBehaviorAssignment { FlowBlock = fbB, Behavior = InputBehavior.RowWise },
                new InputBehaviorAssignment { FlowBlock = fbC, Behavior = InputBehavior.RowWise },
            };

            var selector = new FlowBlockRowWiseDatasetSelector(passedResults, inputAssignments);

            // Act
            var results = selector.GetResults();

            // Assert
            Assert.AreEqual(3, results.Count, "Es sollten 3 zeilenweise kombinierte Datensätze entstehen.");

            // Row 0
            var row0Values = results[0].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            Assert.AreEqual("A1", row0Values[fbAField]);
            Assert.AreEqual("B1", row0Values[fbBField]);
            Assert.AreEqual("C1", row0Values[fbCField]);

            // Row 1
            var row1Values = results[1].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            Assert.AreEqual("A2", row1Values[fbAField]);
            Assert.AreEqual("B2", row1Values[fbBField]);
            Assert.AreEqual("C2", row1Values[fbCField]);

            // Row 2
            var row2Values = results[2].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            Assert.AreEqual("A3", row2Values[fbAField]);
            Assert.AreEqual("B3", row2Values[fbBField]);
            Assert.AreEqual("C3", row2Values[fbCField]);
        }

        [TestMethod]
        public void RowWise_WithShorterColumn_UsesMinimumRowCount()
        {
            // Arrange: 3 FlowBlocks, all RowWise, one with only 2 records

            var fbA = Substitute.For<BaseResultFlowBlock>();
            fbA.Name.Returns("FB_A");

            var fbB = Substitute.For<BaseResultFlowBlock>();
            fbB.Name.Returns("FB_B");

            var fbC = Substitute.For<BaseResultFlowBlock>();
            fbC.Name.Returns("FB_C");

            var fbTarget = Substitute.For<BaseFlowBlock>();
            fbTarget.Name.Returns("Target");
            fbTarget.ReferencedFlowBlocks.Returns(new ObservableCollection<BaseFlowBlock> { fbA, fbB, fbC });

            var fbAField = Substitute.For<FieldElement>();
            fbAField.Name.Returns("FB_A_Field");
            fbAField.Source.Returns(fbA);

            var fbBField = Substitute.For<FieldElement>();
            fbBField.Name.Returns("FB_B_Field");
            fbBField.Source.Returns(fbB);

            var fbCField = Substitute.For<FieldElement>();
            fbCField.Name.Returns("FB_C_Field");
            fbCField.Source.Returns(fbC);

            var preceding = GetPrecedingFieldValues(fbAField, "Pre_A");

            var fbAOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A3", PrecedingFieldValues = preceding } } },
            };

            var fbBOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B3", PrecedingFieldValues = preceding } } },
            };

            // fbC only has 2 records
            var fbCOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C2", PrecedingFieldValues = preceding } } },
            };

            var passedResults = new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>
            {
                { fbA, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbAOuts } } },
                { fbB, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbBOuts } } },
                { fbC, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbCOuts } } },
            };

            var inputAssignments = new List<InputBehaviorAssignment>
            {
                new InputBehaviorAssignment { FlowBlock = fbA, Behavior = InputBehavior.RowWise },
                new InputBehaviorAssignment { FlowBlock = fbB, Behavior = InputBehavior.RowWise },
                new InputBehaviorAssignment { FlowBlock = fbC, Behavior = InputBehavior.RowWise },
            };

            var selector = new FlowBlockRowWiseDatasetSelector(passedResults, inputAssignments);

            // Act
            var results = selector.GetResults();

            // Assert
            Assert.AreEqual(2, results.Count, "The number of combined records should be determined by the shortest RowWise column (2).");

            var row0Values = results[0].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            Assert.AreEqual("A1", row0Values[fbAField]);
            Assert.AreEqual("B1", row0Values[fbBField]);
            Assert.AreEqual("C1", row0Values[fbCField]);

            var row1Values = results[1].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            Assert.AreEqual("A2", row1Values[fbAField]);
            Assert.AreEqual("B2", row1Values[fbBField]);
            Assert.AreEqual("C2", row1Values[fbCField]);
        }

        [TestMethod]
        public void RowWise_WithMixedRowWiseAndFirst_BroadcastsFirstDataset()
        {
            // Arrange: 3 inputs, 2× RowWise, 1× First

            var fbA = Substitute.For<BaseResultFlowBlock>();
            fbA.Name.Returns("FB_A");

            var fbB = Substitute.For<BaseResultFlowBlock>();
            fbB.Name.Returns("FB_B");

            var fbC = Substitute.For<BaseResultFlowBlock>();
            fbC.Name.Returns("FB_C");

            var fbTarget = Substitute.For<BaseFlowBlock>();
            fbTarget.Name.Returns("Target");
            fbTarget.ReferencedFlowBlocks.Returns(new ObservableCollection<BaseFlowBlock> { fbA, fbB, fbC });

            var fbAField = Substitute.For<FieldElement>();
            fbAField.Name.Returns("FB_A_Field");
            fbAField.Source.Returns(fbA);

            var fbBField = Substitute.For<FieldElement>();
            fbBField.Name.Returns("FB_B_Field");
            fbBField.Source.Returns(fbB);

            var fbCField = Substitute.For<FieldElement>();
            fbCField.Name.Returns("FB_C_Field");
            fbCField.Source.Returns(fbC);

            var preceding = GetPrecedingFieldValues(fbAField, "Pre_A");

            var fbAOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbAField, Value = "A3", PrecedingFieldValues = preceding } } },
            };

            var fbBOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B1", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbBField, Value = "B3", PrecedingFieldValues = preceding } } },
            };

            // fbC: First => only the first data record is used and broadcast across all rows
            var fbCOuts = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C_FIRST", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C_IGNORED_2", PrecedingFieldValues = preceding } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping>
                    { new FlowBlockOutDatasetFieldValueMapping { Field = fbCField, Value = "C_IGNORED_3", PrecedingFieldValues = preceding } } },
            };

            var passedResults = new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>
            {
                { fbA, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbAOuts } } },
                { fbB, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbBOuts } } },
                { fbC, new HashSet<FlowBlockOut> { new FlowBlockOut { Results = fbCOuts } } },
            };

            var inputAssignments = new List<InputBehaviorAssignment>
            {
                new InputBehaviorAssignment { FlowBlock = fbA, Behavior = InputBehavior.RowWise },
                new InputBehaviorAssignment { FlowBlock = fbB, Behavior = InputBehavior.RowWise },
                new InputBehaviorAssignment { FlowBlock = fbC, Behavior = InputBehavior.First },
            };

            var selector = new FlowBlockRowWiseDatasetSelector(passedResults, inputAssignments);

            // Act
            var results = selector.GetResults();

            // Assert
            Assert.AreEqual(3, results.Count, "Using 2×RowWise (3 datasets each) and 1×First should result in 3 combined rows.");

            foreach (var row in results)
            {
                var values = row.FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
                Assert.AreEqual("C_FIRST", values[fbCField], "The first dataset of FB_C should be broadcast across all rows.");
            }

            // Check for varying RowWise values
            var r0 = results[0].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            var r1 = results[1].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);
            var r2 = results[2].FieldValueMappings.ToDictionary(m => m.Field, m => m.Value);

            Assert.AreEqual("A1", r0[fbAField]);
            Assert.AreEqual("B1", r0[fbBField]);

            Assert.AreEqual("A2", r1[fbAField]);
            Assert.AreEqual("B2", r1[fbBField]);

            Assert.AreEqual("A3", r2[fbAField]);
            Assert.AreEqual("B3", r2[fbBField]);
        }
    }
}