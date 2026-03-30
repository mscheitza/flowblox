using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Grid.Elements.Util;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote
{
    [Display(Name = "AIPropertyValueGenerationStrategy_DisplayName", 
             Description = "AIPropertyValueGenerationStrategy_Description", ResourceType = typeof(FlowBloxTexts))]
    [FlowBloxSupportedTypes(typeof(BaseSingleResultFlowBlock))]
    public class AIPropertyValueGenerationStrategy : FlowBloxGenerationStrategyBase
    {
        public AIPropertyValueGenerationStrategy()
            : base()
        {
        }

        public AIPropertyValueGenerationStrategy(BaseFlowBlock flowBlock)
            : base(flowBlock)
        {
            if (flowBlock is not BaseSingleResultFlowBlock)
                throw new ArgumentException(nameof(flowBlock), $"The FlowBlock must be of type \"{typeof(BaseSingleResultFlowBlock).Name}\".");
        }

        [Required]
        [Display(Name = "AIPropertyValueGenerationStrategy_Provider", Description = "AIPropertyValueGenerationStrategy_Provider_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Association,
            SelectionFilterMethod = nameof(GetPossibleProviders),
            SelectionDisplayMember = nameof(ManagedObject.Name))]
        public AIProviderBase Provider { get; set; }

        [Required]
        [Display(Name = "AIPropertyValueGenerationStrategy_PromptTemplate", Description = "AIPropertyValueGenerationStrategy_PromptTemplate_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection, ToolboxCategory = nameof(FlowBloxToolboxCategory.AIPropertyValueGenerationPrompts))]
        [FlowBlockTextBox(MultiLine = true, IsCodingMode = true)]
        public string PromptTemplate { get; set; }

        [Display(Name = "AIPropertyValueGenerationStrategy_SystemInstruction", Description = "AIPropertyValueGenerationStrategy_SystemInstruction_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true)]
        public string SystemInstruction { get; set; }

        [Display(Name = "AIPropertyValueGenerationStrategy_ModelOverride", Description = "AIPropertyValueGenerationStrategy_ModelOverride_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string ModelOverride { get; set; }

        [Display(Name = "AIPropertyValueGenerationStrategy_Temperature", Description = "AIPropertyValueGenerationStrategy_Temperature_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        public double Temperature { get; set; }

        [Display(Name = "AIPropertyValueGenerationStrategy_MaxTokens", Description = "AIPropertyValueGenerationStrategy_MaxTokens_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 7)]
        public int? MaxTokens { get; set; }

        [Display(Name = "AIPropertyValueGenerationStrategy_TimeoutSecondsOverride", Description = "AIPropertyValueGenerationStrategy_TimeoutSecondsOverride_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 8)]
        public int? TimeoutSecondsOverride { get; set; }

        [Required]
        [Display(Name = "AIPropertyValueGenerationStrategy_TargetPropertyName", Description = "AIPropertyValueGenerationStrategy_TargetPropertyName_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 9)]
        [FlowBlockTextBox(Suggestions = true, SuggestionMember = nameof(GetPossibleTargetPropertyNames))]
        public string TargetPropertyName { get; set; }

        public List<AIProviderBase> GetPossibleProviders()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetManagedObjects<AIProviderBase>().ToList();
        }

        public IEnumerable<string> GetPossibleTargetPropertyNames()
        {
            if (Source == null)
                return Enumerable.Empty<string>();

            return Source.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsSupportedTargetProperty)
                .Select(x => x.Name)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public override bool CanExecute(out Dictionary<FlowBloxTestDefinition, List<string>> testDefinitionToMessages, out List<string> messages)
        {
            testDefinitionToMessages = new Dictionary<FlowBloxTestDefinition, List<string>>();
            messages = new List<string>();

            if (Source is not BaseSingleResultFlowBlock)
                messages.Add($"The source flow block must be of type \"{typeof(BaseSingleResultFlowBlock).Name}\".");

            if (Provider == null)
                messages.Add("No AI provider specified.");

            if (string.IsNullOrWhiteSpace(PromptTemplate))
                messages.Add("No prompt template specified.");

            if (string.IsNullOrWhiteSpace(TargetPropertyName))
                messages.Add("No target property name specified.");
            else
            {
                var targetProperty = GetTargetPropertyInfo();
                if (targetProperty == null)
                {
                    messages.Add($"Could not resolve target property \"{TargetPropertyName}\".");
                }
                else if (!IsSupportedTargetProperty(targetProperty))
                {
                    messages.Add($"Target property \"{TargetPropertyName}\" must be a writable string property.");
                }
            }

            if (Source?.TestDefinitions == null || !Source.TestDefinitions.Any())
                messages.Add("At least one test case is required.");

            return !messages.Any();
        }

        public override object Execute(BaseRuntime runtime, Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            if (Provider == null || string.IsNullOrWhiteSpace(PromptTemplate))
                return null;
            if (runtime == null)
                throw new InvalidOperationException("No runtime available for AI property generation.");

            var resolvedPrompt = ResolveTemplate(PromptTemplate, testResults);
            var resolvedSystemInstruction = ResolveTemplate(SystemInstruction, testResults);
            var resolvedModel = ResolveTemplate(ModelOverride, testResults);

            var request = new AIRequest
            {
                Prompt = resolvedPrompt,
                SystemInstruction = resolvedSystemInstruction,
                Model = string.IsNullOrWhiteSpace(resolvedModel) ? null : resolvedModel,
                Temperature = Temperature,
                MaxTokens = MaxTokens,
                TimeoutSecondsOverride = TimeoutSecondsOverride
            };

            request.Meta["FlowBlock"] = Source?.Name;
            request.Meta["FlowBlockType"] = Source?.GetType().Name;
            request.Meta["GenerationStrategy"] = Name;

            var response = Provider
                .ExecuteAsync(runtime, request, runtime.GetCancellationToken())
                .GetAwaiter()
                .GetResult();

            if (!response.Success)
            {
                runtime.Report(
                    $"AI property generation failed for \"{Source?.Name}\".{Environment.NewLine}" +
                    $"Provider: {Provider?.Name ?? "n/a"} ({Provider?.ProviderType ?? "n/a"}){Environment.NewLine}" +
                    $"Error: {response.Error ?? "n/a"}",
                    FlowBloxLogLevel.Error);
                return null;
            }

            return response.Text?.Trim();
        }

        public override void Assign(object value)
        {
            var targetProperty = GetTargetPropertyInfo();
            if (targetProperty == null)
                throw new InvalidOperationException($"Could not resolve target property \"{TargetPropertyName}\".");

            targetProperty.SetValue(Source, value?.ToString());
            FlowBloxComponentHelper.RaisePropertyChanged(Source, targetProperty.Name);
        }

        private string ResolveTemplate(string template, Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var resolved = template;

            resolved = resolved.Replace("$GenerationStrategy.Input", BuildGenerationInputText(testResults));
            resolved = resolved.Replace("$TestDefinition::TestExpectations", BuildTestExpectationsText(testResults));
            resolved = resolved.Replace("$TestDefinition::TestResults", BuildTestResultsText(testResults));

            resolved = ReplaceFieldTokensWithTestValues(resolved, testResults);
            resolved = FlowBloxFieldHelper.ReplaceFieldsInString(resolved);

            return resolved;
        }

        private string BuildGenerationInputText(Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            return InputField?.StringValue ?? string.Empty;
        }

        private string ReplaceFieldTokensWithTestValues(string input, Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var regex = new Regex(BaseFlowBlock.Regex_FullyQualifiedFieldNames);
            var tokens = regex.Matches(input)
                .Cast<Match>()
                .Select(x => x.Value)
                .Where(x => !x.StartsWith("$TestDefinition::", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            foreach (var token in tokens)
            {
                var values = new List<string>();
                foreach (var kvp in testResults.OrderBy(x => x.Key.Name))
                {
                    var testResult = kvp.Value;
                    if (testResult?.FieldValueAssignments == null)
                        continue;

                    if (testResult.FieldValueAssignments.TryGetValue(token, out var value))
                    {
                        values.Add($"[{kvp.Key.Name}] {value}");
                    }
                }

                if (values.Any())
                    input = input.Replace(token, string.Join(Environment.NewLine, values));
            }

            return input;
        }

        private string BuildTestResultsText(Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            if (testResults == null || testResults.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var kvp in testResults.OrderBy(x => x.Key.Name))
            {
                sb.AppendLine($"TestCase={kvp.Key.Name}");
                sb.AppendLine($"Success={kvp.Value?.Success}");
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }

        private string BuildTestExpectationsText(Dictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            if (Source == null || testResults == null || testResults.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var kvp in testResults.OrderBy(x => x.Key.Name))
            {
                var testDefinition = kvp.Key;

                if (testDefinition?.Entries == null)
                    throw new InvalidOperationException($"TestCase \"{testDefinition?.Name ?? "n/a"}\" has no entries.");

                var testEntry = testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == Source);
                if (testEntry == null)
                    throw new InvalidOperationException($"TestCase \"{testDefinition.Name}\" has no test entry for flow block \"{Source?.Name ?? "n/a"}\".");

                if (testEntry.FlowBloxTestConfigurations == null || !testEntry.FlowBloxTestConfigurations.Any())
                    throw new InvalidOperationException(
                        $"TestCase \"{testDefinition.Name}\" has no test configurations for flow block \"{Source?.Name ?? "n/a"}\". " +
                        "Cannot generate AI prompt without expectations.");

                sb.AppendLine($"TestCase={testDefinition.Name}");

                var counter = 1;

                var hasAnyExpectationAnchor = false;

                foreach (var config in testEntry.FlowBloxTestConfigurations)
                {
                    sb.AppendLine($"{counter}. Expectation");
                    sb.AppendLine($"Field={config.FieldElement?.FullyQualifiedName ?? "n/a"}");

                    if (!string.IsNullOrWhiteSpace(config.UserInput) &&
                        config.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue)
                    {
                        sb.AppendLine($"ExpectedValue={config.UserInput}");
                        hasAnyExpectationAnchor = true;
                    }

                    if (config.ExpectationConditions != null)
                    {
                        foreach (var condition in config.ExpectationConditions)
                        {
                            var targetText = condition.ExpectationConditionTarget.ToString();
                            if (condition.ExpectationConditionTarget == ExpectationConditionTarget.ValueAtIndex)
                            {
                                targetText = $"{targetText}({condition.Index})";
                                hasAnyExpectationAnchor = true;
                            }
                            else if (condition.ExpectationConditionTarget == ExpectationConditionTarget.AnyValue)
                            {
                                hasAnyExpectationAnchor = true;
                            }

                            sb.AppendLine($"{targetText} {condition.Operator} {condition.Value}");
                        }
                    }

                    sb.AppendLine();
                    counter++;
                }

                if (!hasAnyExpectationAnchor)
                {
                    throw new InvalidOperationException(
                        $"TestCase \"{testDefinition.Name}\" provides no usable expectation anchor for flow block \"{Source?.Name ?? "n/a"}\". " +
                        "Provide either SelectionMode=UserInput_ExpectedValue with a non-empty value, " +
                        "or an expectation condition targeting AnyValue or ValueAtIndex.");
                }
            }

            return sb.ToString().Trim();
        }

        private PropertyInfo GetTargetPropertyInfo()
        {
            if (Source == null || string.IsNullOrWhiteSpace(TargetPropertyName))
                return null;

            return Source.GetType().GetProperty(TargetPropertyName, BindingFlags.Instance | BindingFlags.Public);
        }

        private static bool IsSupportedTargetProperty(PropertyInfo property)
        {
            if (property == null)
                return false;

            return property.CanWrite &&
                   property.SetMethod != null &&
                   property.SetMethod.IsPublic &&
                   property.GetIndexParameters().Length == 0 &&
                   property.PropertyType == typeof(string);
        }
    }
}
