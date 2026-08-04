using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.ControlFlow
{
    [Display(Name = "InvocationFieldTransferConfig", ResourceType = typeof(FlowBloxTexts))]
    public class InvocationFieldTransferConfig : FlowBloxReactiveObject
    {
        [Required()]
        [Display(Name = "InvocationFieldTransferConfig_TransferFrom", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.ComboBox,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(RecursiveCallFlowBlock.GetPossibleTransferFromFieldElements),
            SelectionFilterDependency = nameof(RecursiveCallFlowBlock.ReferencedFlowBlocks))]
        public FieldElement TransferFrom { get; set; }

        [Required()]
        [Display(Name = "InvocationFieldTransferConfig_TransferTo", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.ComboBox,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), 
            SelectionFilterMethod = nameof(RecursiveCallFlowBlock.GetPossibleTransferToFieldElements),
            SelectionFilterDependency = nameof(RecursiveCallFlowBlock.TargetFlowBlock))]
        public FieldElement TransferTo { get; set; }
    }

    [FlowBloxUIGroup("RecursiveCallFlowBlock_Groups_Transferrations", 0)]
    [FlowBloxSpecialExplanation("RecursiveCallFlowBlock_SpecialExplanation_TargetPathAndTransfer", Icon = SpecialExplanationIcon.Warning)]
    [Display(Name = "RecursiveCallFlowBlock_DisplayName", Description = "RecursiveCallFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class RecursiveCallFlowBlock : BaseFlowBlock
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.call_merge, 16, SKColors.MediumSlateBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.call_merge, 32, SKColors.MediumSlateBlue);

        public RecursiveCallFlowBlock() : base ()
        {
            FieldTransferConfigs = new ObservableCollection<InvocationFieldTransferConfig>();
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;


        private BaseFlowBlock _targetFlowBlock;

        [Required]
        [Display(Name = "RecursiveCallFlowBlock_TargetFlowBlock", Description = "RecursiveCallFlowBlock_TargetFlowBlock_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleReferencedElements),
            SelectionDisplayMember = nameof(Name))]
        [CustomValidation(typeof(RecursiveCallFlowBlock), nameof(ValidateTargetFlowBlock))]
        public BaseFlowBlock TargetFlowBlock
        {
            get
            {
                return _targetFlowBlock;
            }
            set
            {
                _targetFlowBlock = value;
                OnPropertyChanged(nameof(FieldTransferConfigs));
            }
        }

        public static ValidationResult ValidateTargetFlowBlock(BaseFlowBlock target, ValidationContext context)
        {
            var recursiveCallFlowBlock = (RecursiveCallFlowBlock)context.ObjectInstance;
            if (target == null)
                return ValidationResult.Success;

            var visited = new HashSet<BaseFlowBlock>();
            bool found = IsTargetReachable(recursiveCallFlowBlock, target, visited);

            if (!found)
            {
                var message = FlowBloxResourceUtil.GetLocalizedString("RecursiveCallFlowBlock_Validation_TargetNotReachable");
                return new ValidationResult(message, [context.MemberName]);
            }

            return ValidationResult.Success;
        }

        private static bool IsTargetReachable(BaseFlowBlock current, BaseFlowBlock target, HashSet<BaseFlowBlock> visited)
        {
            if (visited.Contains(current))
                return false;

            visited.Add(current);

            var previousFlowBlocks = current.ReferencedFlowBlocks;
            if (previousFlowBlocks.Contains(target))
                return true;

            foreach (var previousFlowBlock in previousFlowBlocks)
            {
                if (IsTargetReachable(previousFlowBlock, target, visited))
                    return true;
            }

            return false;
        }

        [Display(Name = "RecursiveCallFlowBlock_FieldTransferConfigs", ResourceType = typeof(FlowBloxTexts), GroupName = "RecursiveCallFlowBlock_Groups_Transferrations", Order = 0)]
        [FlowBloxUI(Factory = UIFactory.GridView, Operations = UIOperations.Create | UIOperations.Delete,
            SelectionFilterMethod = nameof(GetPossibleReferencedElements),
            SelectionDisplayMember = nameof(Name))]
        [CustomValidation(typeof(RecursiveCallFlowBlock), nameof(ValidateFieldTransferConfigs))]
        public ObservableCollection<InvocationFieldTransferConfig> FieldTransferConfigs { get; set; }

        public static ValidationResult ValidateFieldTransferConfigs(ObservableCollection<InvocationFieldTransferConfig> transferConfigs, ValidationContext context)
        {
            var block = context.ObjectInstance as RecursiveCallFlowBlock;
            if (block?.TargetFlowBlock is BaseResultFlowBlock targetResultBlock)
            {
                var unmappedFields = targetResultBlock.Fields
                    .Where(f => !transferConfigs.Any(t => t.TransferTo == f))
                    .ToList();

                if (unmappedFields.Count > 0)
                {
                    var fieldNames = string.Join(", ", unmappedFields.Select(f => f.FullyQualifiedName));
                    return new ValidationResult(
                        string.Format(FlowBloxResourceUtil.GetLocalizedString("RecursiveCallFlowBlock_Validation_UnmappedFields"),
                        fieldNames), [context.MemberName]);
                }
            }
            return ValidationResult.Success;
        }

        public virtual List<FieldElement> GetPossibleTransferFromFieldElements()
        {
            var possibleFieldElements = new List<FieldElement>();
            if (ReferencedFlowBlocks.Count > 0)
            {
                var referencedFlowBlock = ReferencedFlowBlocks.Single();
                var resultFlowBlock = referencedFlowBlock as BaseResultFlowBlock;
                if (resultFlowBlock != null)
                    return resultFlowBlock.Fields;
            }
            return possibleFieldElements;
        }

        public virtual List<FieldElement> GetPossibleTransferToFieldElements()
        {
            var possibleFieldElements = new List<FieldElement>();
            if (TargetFlowBlock != null)
            {
                var resultFlowBlock = TargetFlowBlock as BaseResultFlowBlock;
                if (resultFlowBlock != null)
                    return resultFlowBlock.Fields;
            }
            return possibleFieldElements;
        }

        public override ObservableCollection<BaseFlowBlock> ReferencedFlowBlocks
        {
            get
            {
                return base.ReferencedFlowBlocks;
            }
            set
            {
                base.ReferencedFlowBlocks = value;
                OnPropertyChanged(nameof(FieldTransferConfigs));
            }
        }

        public override bool CanBeReferenced => false;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            { 
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (TargetFlowBlock == null)
                    throw new InvalidOperationException("A target flow block must be defined.");

                foreach (var transferConfig in FieldTransferConfigs)
                {
                    var value = transferConfig.TransferFrom.StringValue;
                    transferConfig.TransferTo.SetValue(runtime, value);
                    runtime.Report($"The field value \"{TextHelper.ShortenString(value, 200, true)}\" was copied to \"{transferConfig.TransferTo.FullyQualifiedName}\". Source: \"{transferConfig.TransferFrom.FullyQualifiedName}\"");
                }
                TargetFlowBlock.ExecuteNextFlowBlocks(runtime);
            });
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(FieldTransferConfigs));
            return properties;
        }

        public override void RuntimeStarted(Runtime.BaseRuntime runtime)
        {
            base.RuntimeStarted(runtime);

            var registry = FlowBloxRegistryProvider.GetRegistry();
            if (registry == null)
                return;

            var previousFlowBlocks = GetAllPreviousFlowBlocks(this, registry);
            var iterationContextSourceFlowBlocks = registry.GetFlowBlocks()
                .OfType<BaseFlowBlock>()
                .Where(x => x.IterationContext != null && previousFlowBlocks.Contains(x.IterationContext))
                .Distinct()
                .ToList();

            if (iterationContextSourceFlowBlocks.Count == 0)
                return;

            var sourceFlowBlockNames = string.Join(
                ", ",
                iterationContextSourceFlowBlocks
                    .Select(x => $"\"{x.Name}\"")
                    .OrderBy(x => x));

            runtime.Report(
                $"A recursion is executed within an iteration context. This means the source flow block(s) {sourceFlowBlockNames} are only executed after all results, including recursive results, are available.",
                FlowBloxLogLevel.Warning);
        }

        private static HashSet<BaseFlowBlock> GetAllPreviousFlowBlocks(BaseFlowBlock flowBlock, FlowBloxRegistry registry)
        {
            var result = new HashSet<BaseFlowBlock>();
            var queue = new Queue<BaseFlowBlock>(registry.GetPreviousElements(flowBlock).OfType<BaseFlowBlock>());

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == null || !result.Add(current))
                    continue;

                foreach (var previous in registry.GetPreviousElements(current).OfType<BaseFlowBlock>())
                {
                    if (previous != null)
                        queue.Enqueue(previous);
                }
            }

            return result;
        }
    }
}

