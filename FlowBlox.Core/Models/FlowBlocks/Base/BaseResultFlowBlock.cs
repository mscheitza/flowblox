using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Fields;
using Newtonsoft.Json;
using FlowBlox.Core.Interfaces;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    [Serializable()]
    [FlowBloxUIGroup("BaseResultFlowBlock_Groups_Output", 25)]
    public abstract class BaseResultFlowBlock : BaseFlowBlock
    {
        [Display(Name = "BaseResultFlowBlock_OutputBehavior", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseResultFlowBlock_Groups_Output", Order = 0)]
        [CustomValidation(typeof(BaseResultFlowBlock), nameof(ValidateOutputBehavior))]
        public OutputBehavior OutputBehavior { get; set; }

        private string _startValue;

        [Display(Name = "BaseResultFlowBlock_StartValue", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseResultFlowBlock_Groups_Output", Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxFieldSelection(DefaultRequiredValue = false)]
        public string StartValue
        {
            get => _startValue;
            set
            {
                _startValue = value;

                if (!string.IsNullOrEmpty(_startValue))
                {
                    OutputBehavior = OutputBehavior.Range;
                    OnPropertyChanged(nameof(OutputBehavior));
                }
            }
        }

        private int? _startIndex;

        [Display(Name = "BaseResultFlowBlock_StartIndex", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseResultFlowBlock_Groups_Output", Order = 1)]
        public int? StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;
                
                if (_startIndex != null)
                {
                    OutputBehavior = OutputBehavior.Range;
                    OnPropertyChanged(nameof(OutputBehavior));
                }
            }
        }

        private int? _endIndex;

        [Display(Name = "BaseResultFlowBlock_EndIndex", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseResultFlowBlock_Groups_Output", Order = 2)]
        public int? EndIndex
        {
            get => _endIndex;
            set
            {
                _endIndex = value;

                if (_endIndex != null)
                {
                    OutputBehavior = OutputBehavior.Range;
                    OnPropertyChanged(nameof(OutputBehavior));
                }
            }
        }

        public static ValidationResult ValidateOutputBehavior(OutputBehavior outputBehavior, ValidationContext context)
        {
            var instance = context.ObjectInstance as BaseResultFlowBlock;
            if (outputBehavior == OutputBehavior.Range &&
                instance != null &&
                string.IsNullOrWhiteSpace(instance.StartValue) &&
                !instance.StartIndex.HasValue &&
                !instance.EndIndex.HasValue)
            {
                return new ValidationResult("When OutputBehavior is Range, either StartValue, StartIndex or EndIndex must be specified.", [context.MemberName]);
            }
            return ValidationResult.Success;
        }

        public abstract List<FieldElement> Fields { get; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public FlowBlockOut GridElementResult { get; protected set; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public FlowBlockOutDataset OutputDataset_CurrentlyProcessing { get; internal set; }

        public override List<FieldElement> GetPossibleFieldElements()
        {
            return FlowBloxFieldsResolver.GetFieldsOrderedByReferencedFlowBlocksExcluding(this);
        }

        public override List<IManagedObject> DefinedManagedObjects
        {
            get
            {
                var definedManagedObjects = base.DefinedManagedObjects;
                definedManagedObjects.AddRange(this.Fields);
                return definedManagedObjects;
            }
        }

        protected void GenerateResult(BaseRuntime runtime, string content)
        {
            var field = this.Fields.Single();
            GenerateResult(runtime, new List<Dictionary<FieldElement, string>>()
            {
                new Dictionary<FieldElement, string>()
                {
                    { field, content }
                }
            });
        }

        protected void GenerateResult(BaseRuntime runtime, IEnumerable<string> contents)
        {
            var field = this.Fields.Single();
            var resultMap = new List<Dictionary<FieldElement, string>>();
            contents.ToList().ForEach(x => resultMap.Add(new Dictionary<FieldElement, string>() { { field, x } }));
            GenerateResult(runtime, resultMap);
        }

        private void AdjustedResultMap(BaseRuntime runtime, IEnumerable<Dictionary<FieldElement, string>> resultMap)
        {
            if (resultMap == null)
                return;

            foreach (var record in resultMap)
            {
                foreach (var fieldElement in record.Keys.ToList())
                {
                    var fieldValue = record[fieldElement];
                    if (fieldValue == null)
                        continue;

                    // Process modifiers
                    foreach (var modifierElement in fieldElement.Modifiers)
                    {
                        var modifiedValue = modifierElement.Modify(runtime, fieldValue);
                        if (modifiedValue != fieldValue)
                        {
                            var shortenedFieldValue = FieldElement.GetShortStringValue(fieldValue);
                            var shortenedModifiedValue = FieldElement.GetShortStringValue(modifiedValue);
                            runtime.Report($"Modified value from \"{shortenedFieldValue}\" to \"{shortenedModifiedValue}\" (at \"{fieldElement.FullyQualifiedName}\").");
                            record[fieldElement] = modifiedValue;
                            fieldValue = modifiedValue;
                        }
                    }

                    // Process conditions
                    var invalidDueToCondition = fieldElement.Conditions.FirstOrDefault(condition => !condition.Compare(fieldValue));
                    if (invalidDueToCondition != null)
                    {
                        runtime.Report($"Validation failed for \"{fieldElement.ShortStringValue}\": does not meet \"{invalidDueToCondition.DisplayName}\" (at \"{fieldElement.FullyQualifiedName}\").");
                        record[fieldElement] = null;
                        continue;
                    }
                }
            }
        }

        private IEnumerable<Dictionary<FieldElement, string>> GetFilteredResultMap(IEnumerable<Dictionary<FieldElement, string>> resultMap)
        {
            if (!InputIgnoreDuplicates || resultMap == null)
                return resultMap;

            var uniqueResultMaps = new List<Dictionary<FieldElement, string>>();
            var alreadyProcessedValues = new HashSet<string>();

            foreach (var record in resultMap)
            {
                var recordValues = string.Join("|", record.OrderBy(x => x.Key).Select(x => x.Value));
                if (alreadyProcessedValues.Contains(recordValues))
                    continue;

                alreadyProcessedValues.Add(recordValues);
                uniqueResultMaps.Add(record);
            }

            return uniqueResultMaps;
        }

        private Dictionary<FieldElement, string> GetPrecedingFieldValues(BaseFlowBlock baseFlowBlock)
        {
            var result = new Dictionary<FieldElement, string>();
            foreach (var previousFlowBlock in baseFlowBlock.ReferencedFlowBlocks.OfType<BaseResultFlowBlock>())
            {
                foreach (var kv in GetPrecedingFieldValues(previousFlowBlock))
                    result.TryAdd(kv.Key, kv.Value);

                foreach (var field in previousFlowBlock.Fields)
                    result.TryAdd(field, field.StringValue);
            }
            return result;
        }

        protected void GenerateResult(BaseRuntime runtime, IEnumerable<Dictionary<FieldElement, string>> resultMap = null)
        {
            ApplyOutputBehavior(runtime, ref resultMap);
            AdjustedResultMap(runtime, resultMap);
            resultMap = GetFilteredResultMap(resultMap);
            this.GridElementResult = new FlowBlockOut();
            Dictionary<FieldElement, string> precedingFieldValues = null;
            precedingFieldValues = GetPrecedingFieldValues(this);
            this.GridElementResult.Results = resultMap?.Any() == true ? resultMap.Select(x => new FlowBlockOutDataset()
            {
                FieldValueMappings = x.Select(y => new FlowBlockOutDatasetFieldValueMapping()
                {
                    Field = y.Key,
                    Value = y.Value,
                    PrecedingFieldValues = precedingFieldValues
                }).ToList()
            }).ToList() : CreateEmptyResults();
            runtime.NotifyResultDatasetGenerated(this, this.GridElementResult.Results?.Count ?? 0);

            if (!runtime.ExecutionFlowEnabled)
                return;

            var nextElementList = this.GetNextFlowBlocks();
            if (!nextElementList.Any())
            {
                foreach (var result in this.GridElementResult.Results)
                {
                    foreach (var fieldValueMapping in result.FieldValueMappings)
                    {
                        fieldValueMapping.Field.SetValue(runtime, fieldValueMapping.Value);
                    }
                }
            }
            else if (nextElementList.All(x => x.HasInputReference))
            {
                this.Fields.ForEach(x => x.Pending = true);
                nextElementList.ForEach(x => x.Execute(runtime, this));
            }
            else
            {
                var items = new List<IRuntimeWorkItem>(this.GridElementResult.Results.Count);
                foreach (var result in this.GridElementResult.Results)
                {
                    items.Add(new Runtime.WorkItems.ApplyOutputDatasetAndScheduleNextWorkItem(this, result));
                }
                runtime.TaskRunner.EnqueueBatchInExecutionOrder(items);
            }
        }

        private void ApplyOutputBehavior(BaseRuntime runtime, ref IEnumerable<Dictionary<FieldElement, string>> resultMap)
        {
            if (resultMap == null)
                return;

            if (this.OutputBehavior == OutputBehavior.First)
            {
                resultMap = new List<Dictionary<FieldElement, string>>()
                {
                    resultMap.First()
                };
            }
            else if (this.OutputBehavior == OutputBehavior.Last)
            {
                resultMap = new List<Dictionary<FieldElement, string>>()
                {
                    resultMap.Last()
                };
            }
            else if (this.OutputBehavior == OutputBehavior.Range)
            {
                int startIndex = this.StartIndex ?? 0;
                int endIndex = this.EndIndex ?? resultMap.Count() - 1;

                string startValue = this.StartValue;
                startValue = FlowBloxFieldHelper.ReplaceFieldsInString(startValue);

                runtime.Report("Use StartIndex=" + startIndex.ToString() + " EndIndex=" + endIndex.ToString() + " StartValue=" + startValue);

                bool startValueFound = false;
                var resultMapList = resultMap.ToList();
                List<Dictionary<FieldElement, string>> filteredResults = new List<Dictionary<FieldElement, string>>();
                if (!string.IsNullOrEmpty(startValue))
                {
                    for (int i = 0; i < resultMapList.Count; i++)
                    {
                        if (resultMapList[i].Values.Contains(startValue))
                        {
                            startIndex = i;
                            startValueFound = true;
                            break;
                        }
                    }

                    if (startValueFound)
                        filteredResults = resultMapList.Skip(startIndex).ToList();
                }

                if (!startValueFound)
                    filteredResults = resultMapList.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();

                resultMap = filteredResults;
            }
        }

        private List<FlowBlockOutDataset> CreateEmptyResults()
        {
            var results = new List<FlowBlockOutDataset>();
            var precedingFieldValues = GetPrecedingFieldValues(this);

            var dataset = new FlowBlockOutDataset()
            {
                FieldValueMappings = this.Fields
                    .Select(field => new FlowBlockOutDatasetFieldValueMapping()
                    {
                        Field = field,
                        Value = null,
                        PrecedingFieldValues = precedingFieldValues
                    })
                    .ToList()
            };

            results.Add(dataset);
            return results;
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();

            if (this.OutputBehavior != OutputBehavior.All)
                properties.Add(nameof(OutputBehavior));

            properties.Add(nameof(StartValue));
            properties.Add(nameof(StartIndex));
            properties.Add(nameof(EndIndex));
            return properties;
        }

        protected sealed override bool InvokeExecutor(BaseRuntime runtime, Action executor)
        {
            var success = base.InvokeExecutor(runtime, executor);
            if (!success)
                GenerateResult(runtime);
            return success;
        }
    }
}
