using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using OfficeOpenXml.ConditionalFormatting.Contracts;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestExecutor
    {
        private TransientRuntime _localRuntime;
        private readonly FlowBloxRegistry _registry;

        private FlowBloxTestDefinition _testDefinition;
        private List<BaseFlowBlock> _capturedFlowBlocks;

        public FlowBloxTestExecutor()
        {
            this._registry = FlowBloxRegistryProvider.GetRegistry();
        }

        public void Initialize(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock)
        {
            _testDefinition = testDefinition;

            FlowBloxTestCapture flowBloxCapture = new FlowBloxTestCapture();
            flowBloxCapture.CreateCapture(_registry.GetStartFlowBlock(), currentFlowBlock);
            _capturedFlowBlocks = flowBloxCapture.GetCapturedFlowBlocks();

            InitializeTransientRuntime();
        }

        public void Shutdown()
        {
            _localRuntime.ShutdownRuntime(_capturedFlowBlocks);
        }

        private void InitializeTransientRuntime()
        {
            _localRuntime = new TransientRuntime(FlowBloxProjectManager.Instance.ActiveProject);
            _localRuntime.InitializeRuntime(_capturedFlowBlocks);
        }

        public FlowBloxTestResult ExecuteTest()
        {
            if (_localRuntime == null ||
                _testDefinition == null ||
                _capturedFlowBlocks == null)
            {
                throw new InvalidOperationException($"Please call \"{nameof(Initialize)}\" before calling \"{nameof(ExecuteTest)}.");
            }

            var fieldValueAssignments = new Dictionary<string, string>();

            var entryOfUserFields = _testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == null);
            if (entryOfUserFields != null)
            {
                foreach (var userFieldTestConfiguration in entryOfUserFields.FlowBloxTestConfigurations)
                {
                    var field = userFieldTestConfiguration.FieldElement;
                    if (userFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput)
                    {
                        var fieldValue = userFieldTestConfiguration.UserInput;
                        field.SetValue(_localRuntime, fieldValue, true);
                        fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                    }
                    else if (userFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.Keep)
                    {
                        fieldValueAssignments[field.FullyQualifiedName] = userFieldTestConfiguration.FieldElement.StringValue;
                    }
                    else
                    {
                        fieldValueAssignments[field.FullyQualifiedName] = string.Empty;
                    }
                }
            }

            foreach (var capturedFlowBlock in _capturedFlowBlocks)
            {
                var testDataset = _testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == capturedFlowBlock);
                if (testDataset == null)
                {
                    _localRuntime.Report($"Test case: No test configuration was found for the FlowBlock \"{capturedFlowBlock.Name}\".");
                    continue;
                }

                var flowBloxTestConfigurations = testDataset.FlowBloxTestConfigurations;

                if (testDataset.Execute)
                {
                    if (!capturedFlowBlock.ValidateRequirements(out List<string> messages))
                    {
                        messages.ForEach(x => _localRuntime.Report($"Test case: {x}", FlowBloxLogLevel.Warning));
                    }

                    if (!capturedFlowBlock.Execute(_localRuntime, null))
                    {
                        _localRuntime.Report($"Test case: Execution of \"{capturedFlowBlock.Name}\" has failed.", FlowBloxLogLevel.Warning);
                    }
                }

                if (capturedFlowBlock is BaseResultFlowBlock)
                {
                    var resultFlowBlock = ((BaseResultFlowBlock)capturedFlowBlock);
                    foreach (var field in resultFlowBlock.Fields)
                    {
                        var FlowBloxTestConfiguration = flowBloxTestConfigurations.SingleOrDefault(x => x.FieldElement == field);
                        if (FlowBloxTestConfiguration == null)
                        {
                            _localRuntime.Report($"Test case: No field configuration was found for the field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Warning);
                            continue;
                        }

                        if (testDataset.Execute)
                        {
                            var fieldValues = resultFlowBlock.GridElementResult.Results
                                .SelectMany(x => x.FieldValueMappings)
                                .Where(x => x.Field == field)
                                .Select(x => x.Value);

                            if (!EvaluateExpectationConditions(field, FlowBloxTestConfiguration, fieldValues))
                                return new FlowBloxTestResult(false, fieldValueAssignments);

                            if (FlowBloxTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.First)
                            {
                                var fieldValue = fieldValues.FirstOrDefault();
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }

                            if (FlowBloxTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.Index)
                            {
                                if (!FlowBloxTestConfiguration.Index.HasValue)
                                {
                                    _localRuntime.Report($"Test case: No index was defined in the test data for field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Error);
                                    new FlowBloxTestResult(false, fieldValueAssignments);
                                }

                                var fieldValue = fieldValues.ElementAtOrDefault(new Index(FlowBloxTestConfiguration.Index.Value));
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }


                            if (FlowBloxTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.Last)
                            {
                                var fieldValue = fieldValues.LastOrDefault();
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }   

                            if (FlowBloxTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue)
                            {
                                if (!fieldValues.Any(x => x == FlowBloxTestConfiguration.UserInput))
                                {
                                    _localRuntime.Report($"Test case: There is no value \"{TextHelper.ShortenString(FlowBloxTestConfiguration.UserInput, 100, true)}\" in field values from field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Error);
                                    return new FlowBloxTestResult(false, fieldValueAssignments);
                                }

                                var fieldValue = FlowBloxTestConfiguration.UserInput;
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }
                        }

                        var userInput = FlowBloxTestConfiguration.UserInput;
                        if (!string.IsNullOrEmpty(userInput) && 
                            FlowBloxTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput)
                        {
                            var fieldValue = userInput;
                            field.SetValue(_localRuntime, fieldValue);
                            fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                        }
                    }
                }
            }

            _localRuntime.Report($"The test \"{_testDefinition.Name}\" was completed successfully.");

            return new FlowBloxTestResult(true, fieldValueAssignments);
        }

        private bool EvaluateExpectationConditions(FieldElement field, FlowBloxTestConfiguration flowBloxTestConfiguration, IEnumerable<string> fieldValues)
        {
            if (flowBloxTestConfiguration.ExpectationConditions == null)
                return true;

            if (!flowBloxTestConfiguration.ExpectationConditions.Any())
                return true;

            foreach (var expectationCondition in flowBloxTestConfiguration.ExpectationConditions)
            {
                bool conditionMet = false;

                switch (expectationCondition.ExpectationConditionTarget)
                {
                    case ExpectationConditionTarget.FirstValue:
                        {
                            string value = fieldValues.FirstOrDefault();
                            conditionMet = expectationCondition.Check(value);
                            break;
                        }
                    case ExpectationConditionTarget.AnyValue:
                        {
                            conditionMet = fieldValues.Any(value => expectationCondition.Check(value));
                            break;
                        }
                    case ExpectationConditionTarget.LastValue:
                        {
                            string value = fieldValues.LastOrDefault();
                            conditionMet = expectationCondition.Check(value);
                            break;
                        }
                    case ExpectationConditionTarget.NumberOfDatasets:
                        {
                            int count = fieldValues.Count();
                            conditionMet = expectationCondition.Check(count);
                            break;
                        }
                }

                if (!conditionMet)
                {
                    _localRuntime.Report($"Test case: Expectation condition \"{expectationCondition.DisplayName}\" failed for the field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Error);
                    return false;
                }
            }

            _localRuntime.Report($"Test case: All expectation conditions met for the field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Success);
            return true;
        }

        public BaseRuntime GetRuntime() => _localRuntime;
    }
}
