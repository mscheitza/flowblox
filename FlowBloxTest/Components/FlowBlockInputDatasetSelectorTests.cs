using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace FlowBloxTest.Components
{
    [TestClass]
    public class FlowBlockInputDatasetSelectorTests
    {
        private Dictionary<FieldElement, string> GetPrecedingFieldValues(FieldElement fieldElement, string value) => GetPrecedingFieldValues(new Tuple<FieldElement, string>(fieldElement, value));

        private Dictionary<FieldElement, string> GetPrecedingFieldValues(params Tuple<FieldElement, string>[] fieldValues)
        {
            var result = new Dictionary<FieldElement, string>();
            foreach (var fieldValue in fieldValues)
            {
                result[fieldValue.Item1] = fieldValue.Item2;
            }
            return result;
        }

        [TestMethod]
        public void Test_GetResults_WithSpecificSetup_ReturnsFilteredResults()
        {
            // Erstelle Substitutes für die FlowBlocks
            var fb1 = Substitute.For<BaseResultFlowBlock>();
            fb1.Name.Returns("FB1");

            var fb2 = Substitute.For<BaseResultFlowBlock>();
            fb2.Name.Returns("FB2");
            var fb2_ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock>{ fb1 };
            fb2.ReferencedFlowBlocks.Returns(fb2_ReferencedFlowBlocks);

            var fb3 = Substitute.For<BaseResultFlowBlock>();
            fb3.Name.Returns("FB3");
            var fb3_ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock> { fb1 };
            fb3.ReferencedFlowBlocks.Returns(fb3_ReferencedFlowBlocks);

            var fb4 = Substitute.For<BaseFlowBlock>();
            fb4.Name.Returns("FB4");
            var fb4_ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock> { fb2, fb3 };
            fb4.ReferencedFlowBlocks.Returns(fb4_ReferencedFlowBlocks);

            // Erstelle Substitutes für FieldElements
            var fb1Field = Substitute.For<FieldElement>();
            fb1Field.Name.Returns("FB1-Feld");
            fb1Field.Source.Returns(fb2);

            var fb2Field = Substitute.For<FieldElement>();
            fb2Field.Name.Returns("FB2-Feld");
            fb2Field.Source.Returns(fb2);

            var fb3Field = Substitute.For<FieldElement>();
            fb3Field.Name.Returns("FB3-Feld");
            fb3Field.Source.Returns(fb3);

            var precedingFieldValues = GetPrecedingFieldValues(fb1Field, "FB1-Value1");

            // Erstelle Mock-Daten für FlowBlockOut und FlowBlockOutDataset
            var fb2Outs = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "Value1", PrecedingFieldValues = precedingFieldValues } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "Value2", PrecedingFieldValues = precedingFieldValues } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "Value3", PrecedingFieldValues = precedingFieldValues } } }
            };

            var fb3Outs = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "Value4", PrecedingFieldValues = precedingFieldValues } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "Value5", PrecedingFieldValues = precedingFieldValues } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "Value6", PrecedingFieldValues = precedingFieldValues } } }
            };

            var passedResults = new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>
            {
                { fb2, new HashSet<FlowBlockOut>() { new FlowBlockOut() { Results = fb2Outs } } },
                { fb3, new HashSet<FlowBlockOut>() { new FlowBlockOut() { Results = fb3Outs } } }
            };

            var inputAssignments = new List<InputBehaviorAssignment>()
            {
                new InputBehaviorAssignment() { FlowBlock = fb2, Behavior = InputBehavior.Cross },
                new InputBehaviorAssignment() { FlowBlock = fb3, Behavior = InputBehavior.Cross }
            };

            FlowBlockInputDatasetSelector flowBlockInputDatasetSelector = new FlowBlockInputDatasetSelector(passedResults, inputAssignments);
            var results = flowBlockInputDatasetSelector.GetResults();

            // Ergebnisabgleich
            Assert.AreEqual(9, results.Count, "Die Anzahl der zurückgegebenen Kombinationen sollte 9 sein.");
        }

        [TestMethod]
        public void Test_GetResults_WithIntegrityCheck_ReturnsEighteenCombinations()
        {
            // Erstelle Substitutes für die FlowBlocks
            var fb1 = Substitute.For<BaseResultFlowBlock>();
            fb1.Name.Returns("FB1");

            var fb2 = Substitute.For<BaseResultFlowBlock>();
            fb2.Name.Returns("FB2");
            var fb2_ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock> { fb1 };
            fb2.ReferencedFlowBlocks.Returns(fb2_ReferencedFlowBlocks);

            var fb3 = Substitute.For<BaseResultFlowBlock>();
            fb3.Name.Returns("FB3");
            var fb3_ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock> { fb1 };
            fb3.ReferencedFlowBlocks.Returns(fb3_ReferencedFlowBlocks);

            var fb4 = Substitute.For<BaseFlowBlock>();
            fb4.Name.Returns("FB4");
            var fb4_ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock> { fb2, fb3 };
            fb4.ReferencedFlowBlocks.Returns(fb4_ReferencedFlowBlocks);

            // Erstelle Substitutes für FieldElements
            var fb1Field = Substitute.For<FieldElement>();
            fb1Field.Name.Returns("FB1-Feld");
            fb1Field.Source.Returns(fb2);

            var fb2Field = Substitute.For<FieldElement>();
            fb2Field.Name.Returns("FB2-Feld");
            fb2Field.Source.Returns(fb2);

            var fb3Field = Substitute.For<FieldElement>();
            fb3Field.Name.Returns("FB3-Feld");
            fb3Field.Source.Returns(fb3);

            var precedingFieldValuesForFB1Value1 = GetPrecedingFieldValues(fb1Field, "FB1-Value1");
            var precedingFieldValuesForFB1Value2 = GetPrecedingFieldValues(fb1Field, "FB1-Value2");


            // Erstelle Mock-Daten für FlowBlockOut und FlowBlockOutDataset
            var fb2Outs = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "FB1_Value1_FB2_Value1", PrecedingFieldValues = precedingFieldValuesForFB1Value1 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "FB1_Value1_FB2_Value2", PrecedingFieldValues = precedingFieldValuesForFB1Value1 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "FB1_Value1_FB2_Value3", PrecedingFieldValues = precedingFieldValuesForFB1Value1 } } }
            };

            var fb2OutsExtended = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "FB2_Value2_FB2_Value1", PrecedingFieldValues = precedingFieldValuesForFB1Value2 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "FB2_Value2_FB2_Value2", PrecedingFieldValues = precedingFieldValuesForFB1Value2 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb2Field, Value = "FB2_Value2_FB2_Value3", PrecedingFieldValues = precedingFieldValuesForFB1Value2 } } },
            };

            var fb3Outs = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "FB1_Value1_FB3_Value1", PrecedingFieldValues = precedingFieldValuesForFB1Value1 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "FB1_Value1_FB3_Value1", PrecedingFieldValues = precedingFieldValuesForFB1Value1 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "FB1_Value1_FB3_Value1", PrecedingFieldValues = precedingFieldValuesForFB1Value1 } } }
            };

            var fb3OutsExtended = new List<FlowBlockOutDataset>
            {
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "FB2_Value2_FB3_Value1", PrecedingFieldValues = precedingFieldValuesForFB1Value2 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "FB2_Value2_FB3_Value2", PrecedingFieldValues = precedingFieldValuesForFB1Value2 } } },
                new FlowBlockOutDataset { FieldValueMappings = new List<FlowBlockOutDatasetFieldValueMapping> { new FlowBlockOutDatasetFieldValueMapping { Field = fb3Field, Value = "FB2_Value2_FB3_Value3", PrecedingFieldValues = precedingFieldValuesForFB1Value2 } } },
            };

            // Adjust passedResults to include extended datasets
            var passedResultsExtended = new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>
            {
                { fb2, new HashSet<FlowBlockOut>() { new FlowBlockOut() { Results = fb2Outs }, new FlowBlockOut() { Results = fb2OutsExtended } } },
                { fb3, new HashSet<FlowBlockOut>() { new FlowBlockOut() { Results = fb3Outs }, new FlowBlockOut() { Results = fb3OutsExtended } } }
            };

            var inputAssignments = new List<InputBehaviorAssignment>()
            {
                new InputBehaviorAssignment() { FlowBlock = fb2, Behavior = InputBehavior.Cross },
                new InputBehaviorAssignment() { FlowBlock = fb3, Behavior = InputBehavior.Cross }
            };

            // Verwende die erweiterten Datasets und InputAssignments in deinem FlowBlockInputDatasetSelector
            FlowBlockInputDatasetSelector flowBlockInputDatasetSelector = new FlowBlockInputDatasetSelector(passedResultsExtended, inputAssignments);
            var results = flowBlockInputDatasetSelector.GetResults();

            // Ergebnisabgleich
            Assert.AreEqual(18, results.Count, "Die Anzahl der zurückgegebenen Kombinationen sollte 18 sein.");
        }
    }
}