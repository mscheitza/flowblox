using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestExecutor
    {
        private TransientRuntime _localRuntime;
        private readonly FlowBloxRegistry _registry;

        private FlowBloxTestDefinition _testDefinition;
        private List<BaseFlowBlock> _capturedFlowBlocks;

        public event EventHandler<TestExpectationConditionFailedEventArgs>? ExpectationConditionFailed;

        public FlowBloxTestExecutor()
        {
            this._registry = FlowBloxRegistryProvider.GetRegistry();
        }

        private readonly FlowBloxTestDefinitionLatestFlowBlockResolver _latestResolver = new FlowBloxTestDefinitionLatestFlowBlockResolver();

        public void Initialize(
            FlowBloxTestDefinition testDefinition,
            BaseFlowBlock currentFlowBlock,
            IEnumerable<BaseFlowBlock>? includedFlowBlocks = null)
        {
            _testDefinition = testDefinition;

            FlowBloxTestCapture flowBloxCapture = new FlowBloxTestCapture();
            var targetFlowBlock = currentFlowBlock ?? _latestResolver.ResolveLatestFlowBlock(_testDefinition);
            flowBloxCapture.CreateCapture(_registry.GetStartFlowBlock(), targetFlowBlock);
            _capturedFlowBlocks = flowBloxCapture.GetCapturedFlowBlocks();

            InitializeTransientRuntime(includedFlowBlocks);
        }

        public void Shutdown()
        {
            _localRuntime.ShutdownRuntime(_capturedFlowBlocks);
        }

        private void InitializeTransientRuntime(IEnumerable<BaseFlowBlock>? includedFlowBlocks)
        {
            _localRuntime = new TransientRuntime(FlowBloxProjectManager.Instance.ActiveProject);
            _localRuntime.IncludedFlowBlocks = includedFlowBlocks?
                .Where(x => x != null)
                .Distinct()
                .ToList() ?? new List<BaseFlowBlock>();
            _localRuntime.InitializeRuntime(_capturedFlowBlocks);
        }

        public Task<FlowBloxTestResult> ExecuteTestAsync()
        {
            return Task.Run(ExecuteTest);
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
                        var FlowBloxFieldTestConfiguration = flowBloxTestConfigurations.SingleOrDefault(x => x.FieldElement == field);
                        if (FlowBloxFieldTestConfiguration == null)
                        {
                            _localRuntime.Report($"Test case: No field configuration was found for the field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Warning);
                            continue;
                        }

                        ReportExpectedValueConfiguredButNotExecuted(testDataset, FlowBloxFieldTestConfiguration);
                        ReportMissingExpectedValueForExecutedDataset(testDataset, FlowBloxFieldTestConfiguration);

                        if (testDataset.Execute)
                        {
                            var fieldValues = resultFlowBlock.GridElementResult.Results
                                .SelectMany(x => x.FieldValueMappings)
                                .Where(x => x.Field == field)
                                .Select(x => x.Value);

                            if (!EvaluateExpectationConditions(field, FlowBloxFieldTestConfiguration, fieldValues, out var failedCondition))
                            {
                                var eventArgs = new TestExpectationConditionFailedEventArgs(
                                    _testDefinition,
                                    capturedFlowBlock,
                                    field,
                                    FlowBloxFieldTestConfiguration,
                                    failedCondition!,
                                    _localRuntime,
                                    new FlowBloxTestResult(false, new Dictionary<string, string>(fieldValueAssignments)));

                                ExpectationConditionFailed?.Invoke(this, eventArgs);
                                if (!eventArgs.RepeatLatestExecution)
                                    return new FlowBloxTestResult(false, fieldValueAssignments);

                                _localRuntime.Report(
                                    $"Test case: Repeating latest execution for \"{capturedFlowBlock.Name}\" after regeneration hook.",
                                    FlowBloxLogLevel.Info);

                                if (testDataset.Execute)
                                {
                                    if (!capturedFlowBlock.ValidateRequirements(out List<string> repeatMessages))
                                        repeatMessages.ForEach(x => _localRuntime.Report($"Test case: {x}", FlowBloxLogLevel.Warning));

                                    if (!capturedFlowBlock.Execute(_localRuntime, null))
                                        _localRuntime.Report($"Test case: Re-execution of \"{capturedFlowBlock.Name}\" has failed.", FlowBloxLogLevel.Warning);
                                }

                                fieldValues = resultFlowBlock.GridElementResult.Results
                                    .SelectMany(x => x.FieldValueMappings)
                                    .Where(x => x.Field == field)
                                    .Select(x => x.Value);

                                if (!EvaluateExpectationConditions(field, FlowBloxFieldTestConfiguration, fieldValues, out _))
                                    return new FlowBloxTestResult(false, fieldValueAssignments);
                            }

                            if (FlowBloxFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.First)
                            {
                                var fieldValue = fieldValues.FirstOrDefault();
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }

                            if (FlowBloxFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.Index)
                            {
                                if (!FlowBloxFieldTestConfiguration.Index.HasValue)
                                {
                                    _localRuntime.Report($"Test case: No index was defined in the test data for field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Error);
                                    new FlowBloxTestResult(false, fieldValueAssignments);
                                }

                                var fieldValue = fieldValues.ElementAtOrDefault(new Index(FlowBloxFieldTestConfiguration.Index.Value));
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }


                            if (FlowBloxFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.Last)
                            {
                                var fieldValue = fieldValues.LastOrDefault();
                                field.SetValue(_localRuntime, fieldValue);
                                fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                            }   

                            if (FlowBloxFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue)
                            {
                                if (!string.IsNullOrWhiteSpace(FlowBloxFieldTestConfiguration.UserInput))
                                {
                                    if (!fieldValues.Any(x => x == FlowBloxFieldTestConfiguration.UserInput))
                                    {
                                        _localRuntime.Report($"Test case: There is no value \"{TextHelper.ShortenString(FlowBloxFieldTestConfiguration.UserInput, 100, true)}\" in field values from field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Error);
                                        return new FlowBloxTestResult(false, fieldValueAssignments);
                                    }

                                    var fieldValue = FlowBloxFieldTestConfiguration.UserInput;
                                    field.SetValue(_localRuntime, fieldValue);
                                    fieldValueAssignments[field.FullyQualifiedName] = fieldValue;
                                }
                            }
                        }

                        var userInput = FlowBloxFieldTestConfiguration.UserInput;
                        if (!string.IsNullOrEmpty(userInput) && 
                            FlowBloxFieldTestConfiguration.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput)
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

        private void ReportExpectedValueConfiguredButNotExecuted(
            FlowBlockTestDataset testDataset,
            FlowBloxFieldTestConfiguration fieldTestConfiguration)
        {
            if (testDataset.Execute || testDataset.FlowBlock == null)
                return;

            var selectionMode = fieldTestConfiguration.SelectionMode;
            var hasExpectedValueSelectionMode = selectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue;
            var hasExpectedValue = !string.IsNullOrWhiteSpace(fieldTestConfiguration.UserInput);

            if (!hasExpectedValueSelectionMode || !hasExpectedValue)
                return;

            var fieldName = fieldTestConfiguration.FieldElement?.FullyQualifiedName;
            var flowBlockName = testDataset.FlowBlock.Name;

            _localRuntime.Report(
                $"Test case: Warning - an expected value is configured for field \"{fieldName}\", but flow block \"{flowBlockName}\" is not executed.",
                FlowBloxLogLevel.Warning);
        }

        private void ReportMissingExpectedValueForExecutedDataset(
            FlowBlockTestDataset testDataset,
            FlowBloxFieldTestConfiguration fieldTestConfiguration)
        {
            if (!testDataset.Execute || testDataset.FlowBlock == null)
                return;

            if (fieldTestConfiguration.SelectionMode != FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue)
                return;

            if (!string.IsNullOrWhiteSpace(fieldTestConfiguration.UserInput))
                return;

            var hasTestExpectations = fieldTestConfiguration.ExpectationConditions?.Any() == true;
            if (hasTestExpectations)
                return;

            var fieldName = fieldTestConfiguration.FieldElement?.FullyQualifiedName ?? string.Empty;
            var flowBlockName = testDataset.FlowBlock.Name;

            _localRuntime.Report(
                $"Test case: Warning - flow block \"{flowBlockName}\" is executed with selection mode \"{FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue}\", but no expected value and no test expectations are configured for field \"{fieldName}\". No result is selected or fixed by expected value, and no expectation validation is defined.",
                FlowBloxLogLevel.Warning);
        }

        private bool EvaluateExpectationConditions(
            FieldElement field,
            FlowBloxFieldTestConfiguration flowBloxTestConfiguration,
            IEnumerable<string> fieldValues,
            out ExpectationCondition? failedCondition)
        {
            failedCondition = null;

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
                            conditionMet = expectationCondition.Compare(value);
                            break;
                        }
                    case ExpectationConditionTarget.AnyValue:
                        {
                            conditionMet = fieldValues.Any(value => expectationCondition.Compare(value));
                            break;
                        }
                    case ExpectationConditionTarget.LastValue:
                        {
                            string value = fieldValues.LastOrDefault();
                            conditionMet = expectationCondition.Compare(value);
                            break;
                        }
                    case ExpectationConditionTarget.NumberOfDatasets:
                        {
                            int count = fieldValues.Count();
                            conditionMet = expectationCondition.Compare(count);
                            break;
                        }
                }

                if (!conditionMet)
                {
                    _localRuntime.Report($"Test case: Expectation condition \"{expectationCondition.DisplayName}\" failed for the field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Error);
                    failedCondition = expectationCondition;
                    return false;
                }
            }

            _localRuntime.Report($"Test case: All expectation conditions met for the field \"{field.FullyQualifiedName}\".", FlowBloxLogLevel.Success);
            return true;
        }

        public BaseRuntime GetRuntime() => _localRuntime;
    }
}

