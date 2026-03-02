using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base.DatasetSelection;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Runtime.WorkItems;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.FlowBlocks;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Reflection;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    [FlowBlockUIGroup("Global_Groups_Default", -10, ControlAlignment.Top)]
    [FlowBlockUIGroup("BaseFlowBlock_Groups_Dependencies", 10)]
    [FlowBlockUIGroup("BaseFlowBlock_Groups_Input", 20)]
    [FlowBlockUIGroup("Global_Groups_Requirements", 30)]
    [FlowBlockUIGroup("BaseFlowBlock_Groups_Tests", 40)]
    [Serializable()]
    public abstract class BaseFlowBlock : FlowBloxComponent
    {
        public delegate void OnPropertyValuesChangedEventHandler();
        public delegate void WarnEventHandler(BaseRuntime runtime, string message);
        public delegate void ErrorEventHandler(BaseRuntime runtime, string message);
        public delegate void UndoNotificationEventHandler(BaseRuntime runtime);
        public delegate void FlagsChangedEventHandler(BaseRuntime runtime, FlowBlockFlags flowBlockFlags);

        public delegate void FlowBlockNameChangeEventHandler(BaseFlowBlock flowBlock, string oldName, string newName);
        public delegate void PropertyChangeEventHandler(string propertyName);

        public delegate void IterationStartHandler(BaseRuntime runtime);
        public delegate void IterationEndHandler(BaseRuntime runtime);

        public delegate void OnBeforeInputProcessingEventHandler();

        public event OnPropertyValuesChangedEventHandler OnPropertyValuesChanged;
        public event UndoNotificationEventHandler OnUndoWarn;
        public event UndoNotificationEventHandler OnUndoError;
        public event WarnEventHandler OnWarn;
        public event ErrorEventHandler OnError;
        public event FlagsChangedEventHandler OnFlagsChanged;
        public event FlowBlockNameChangeEventHandler OnNameChanged;
        public event IterationStartHandler IterationStart;
        public event IterationEndHandler IterationEnd;
        public event OnBeforeInputProcessingEventHandler OnBeforeInputProcessing;

        internal void RaiseIterationStart(BaseRuntime runtime) => IterationStart?.Invoke(runtime);
        internal void RaiseIterationEnd(BaseRuntime runtime) => IterationEnd?.Invoke(runtime);

        public void CreateNotification(BaseRuntime runtime, Enum notificationEnumValue, Exception e = null)
        {
            if (GetCurrentNotificationType(notificationEnumValue) == NotificationType.None)
                return;

            string message = notificationEnumValue.GetDisplayName();

            var memberInfo = notificationEnumValue.GetType().GetMember(notificationEnumValue.ToString()).Single();
            var attribute = memberInfo.GetCustomAttribute<FlowBlockNotificationAttribute>();
            if (attribute == null)
                throw new InvalidOperationException(
                    $"Missing {nameof(FlowBlockNotificationAttribute)} on enum value '{notificationEnumValue}'. " +
                     "Please annotate the enum with [FlowBlockNotification(...)] to define its NotificationType.");

            var notificationType = attribute.NotificationType;
            if (notificationType == NotificationType.Warning)
            {
                runtime.Report(message, FlowBloxLogLevel.Warning, e);
                runtime.NotifyWarning(this, message);
                OnWarn?.Invoke(runtime, message);
            }
            else
            {
                runtime.Report(message, FlowBloxLogLevel.Error, e);
                runtime.NotifyError(this, message, e);
                OnError?.Invoke(runtime, message);
            }
        }

        private void UndoWarn(BaseRuntime runtime) => OnUndoWarn?.Invoke(runtime);

        private void UndoError(BaseRuntime runtime) => OnUndoError?.Invoke(runtime);

        [DeepCopierIgnore()]
        public virtual List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = new List<Type>
                {
                    typeof(BaseFlowBlockNotifications)
                };
                return notificationTypes;
            }
        }

        [DeepCopierIgnore()]
        public ObservableCollection<OverriddenNotificationEntry> OverriddenNotificationEntries { get; set; }

        public NotificationType GetCurrentNotificationType(Enum enumValue)
        {
            var enumType = enumValue.GetType();
            var enumKey = Convert.ToInt64(enumValue);

            var entry = OverriddenNotificationEntries
                .FirstOrDefault(x => x.TypeName == enumType.AssemblyQualifiedName);

            if (entry != null && entry.Overrides.TryGetValue(enumKey, out var overridden))
                return overridden;

            var memberInfo = enumType.GetMember(enumValue.ToString()).Single();
            var attribute = memberInfo.GetCustomAttribute<FlowBlockNotificationAttribute>();
            if (attribute == null)
                throw new InvalidOperationException($"Missing FlowBlockNotificationAttribute on {enumValue}");

            return attribute.NotificationType;
        }

        public void OverrideNotificationType(Enum enumValue, NotificationType notificationType)
        {
            var enumType = enumValue.GetType();
            var enumKey = Convert.ToInt64(enumValue);

            var entry = OverriddenNotificationEntries
                .FirstOrDefault(x => x.TypeName == enumType.AssemblyQualifiedName);

            if (notificationType == GetDefaultNotificationType(enumValue))
            {
                if (entry != null && entry.Overrides.Remove(enumKey) && entry.Overrides.Count == 0)
                    OverriddenNotificationEntries.Remove(entry);

                return;
            }

            if (entry == null)
            {
                entry = new OverriddenNotificationEntry
                {
                    TypeName = enumType.AssemblyQualifiedName
                };
                OverriddenNotificationEntries.Add(entry);
            }

            entry.Overrides[enumKey] = notificationType;
        }

        private NotificationType GetDefaultNotificationType(Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).Single();
            var attribute = memberInfo.GetCustomAttribute<FlowBlockNotificationAttribute>();
            if (attribute == null)
                throw new InvalidOperationException($"Missing FlowBlockNotificationAttribute on {enumValue}");

            return attribute.NotificationType;
        }

        public void Flag(BaseRuntime runtime, FlowBlockFlags flags)
        {
            this.CurrentFlags = flags;
            OnFlagsChanged?.Invoke(runtime, flags);
        }

        public void ResetNotifications(BaseRuntime runtime)
        {
            this.UndoWarn(runtime);
            this.UndoError(runtime);
            this.Flag(runtime, FlowBlockFlags.None);
        }

        public void PropertyValuesChanged() => OnPropertyValuesChanged?.Invoke();

        internal const string Regex_FullyQualifiedFieldNames = "\\$[A-Za-z0-9_-öäüÖÄÜ]*::[A-Za-z0-9_-öäüÖÄÜ]*";
        internal const string Regex_ValidateIdentifier = "^[A-Za-z0-9_-öäüÖÄÜ]*$";

        [JsonIgnore()]
        public virtual string NamePrefix
        {
            get
            {
                string typeName = this.GetType().Name;
                if (typeName.EndsWith(GlobalConstants.FlowBlockTypeNameSuffix))
                {
                    var length = typeName.Length - GlobalConstants.FlowBlockTypeNameSuffix.Length;
                    return typeName.Substring(0, length) + '_';
                }
                else
                {
                    return typeName;
                }
            }
        }

        [JsonIgnore()]
        public virtual bool CreateNumericNameSuffix => true;

        public Point Location { get; set; }

        public int ElementIndex { get; set; } = -1;

        public bool IsNotExecuted { get; set; }

        [JsonIgnore()]
        public virtual bool CanGoBack => true;

        [JsonIgnore()]
        public virtual bool CanBeReferenced => true;

        public bool BreakPoint { get; set; }

        protected BaseFlowBlock()
        {
            this.InputBehaviorAssignments = new ObservableCollection<InputBehaviorAssignment>();
            this.ReferencedFlowBlocks = new ObservableCollection<BaseFlowBlock>();
            this.ActivationConditions = new ObservableCollection<LogicalCondition>();
            this.TestDefinitions = new ObservableCollection<FlowBloxTestDefinition>();
            this.GenerationStrategies = new ObservableCollection<FlowBloxGenerationStrategyBase>();

            this.InheritRequirementsNotMet = true;

            this.OverriddenNotificationEntries = new ObservableCollection<OverriddenNotificationEntry>();
        }

        public override void OnAfterLoad()
        {
            base.OnAfterLoad();
        }

        private string _name;

        /// <summary>
        /// Der Elementbezeichner des Grid-Elements.
        /// </summary>
        [Display(Name = "BaseFlowBlock_Name", ResourceType = typeof(FlowBloxTexts), GroupName = "Global_Groups_Default", Order = -10)]
        [CustomValidation(typeof(FlowBloxComponent), nameof(ValidateName))]
        [Required()]
        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                string oldName = _name;
                string newName = value;

                if (oldName != newName)
                {
                    this._name = newName;
                    OnNameChanged?.Invoke(this, oldName, newName);
                }
            }
        }

        [JsonIgnore()]
        public BaseFlowBlock Parent { get; protected set; }

        public abstract FlowBlockCategory GetCategory();

        public abstract FlowBlockCardinalities GetInputCardinality();

        private ObservableCollection<BaseFlowBlock> _referencedFlowBlocks = new ObservableCollection<BaseFlowBlock>();

        /// <summary>
        /// Returns the flow blocks that are connected via flow logic (e.g., based on execution path or data flow).
        /// Does not include flow blocks that are associated via property definitions — use <see cref="GetAssociatedFlowBlocks"/> for that.
        /// </summary>
        [Display(Name = "BaseFlowBlock_ReferencedElements", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseFlowBlock_Groups_Dependencies", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ListView, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(BaseFlowBlock.GetPossibleReferencedElements),
            SelectionDisplayMember = nameof(Name))]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(BaseFlowBlock.Name) })]
        public virtual ObservableCollection<BaseFlowBlock> ReferencedFlowBlocks
        {
            get => _referencedFlowBlocks;
            set
            {
                if (value == null)
                    throw new NotSupportedException($"Setting {nameof(ReferencedFlowBlocks)} to null is not supported.");

                if (_referencedFlowBlocks != null)
                    _referencedFlowBlocks.CollectionChanged -= _referencedFlowBlocks_CollectionChanged;

                _referencedFlowBlocks = value;

                _referencedFlowBlocks.CollectionChanged -= _referencedFlowBlocks_CollectionChanged;
                _referencedFlowBlocks.CollectionChanged += _referencedFlowBlocks_CollectionChanged;

                OnAfterReferencedFlowBlocksChanged();
            }
        }

        private void _referencedFlowBlocks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnAfterReferencedFlowBlocksChanged();
        }

        public virtual List<IManagedObject> DefinedManagedObjects
        {
            get
            {
                return new List<IManagedObject>();
            }
        }

        public virtual bool IsManaged(IManagedObject managedObject)
        {
            if (this.TestDefinitions.Contains(managedObject))
                return true;

            if (this.DefinedManagedObjects.Contains(managedObject))
                return true;

            return false;
        }

        public virtual bool IsDeletable(out List<BaseFlowBlock> dependencies)
        {
            dependencies = new List<BaseFlowBlock>();

            var registry = FlowBloxRegistryProvider.GetRegistry();

            foreach (var flowBlock in registry.GetFlowBlocks())
            {
                if (flowBlock.ReferencedFlowBlocks.Contains(registry.GetOriginalRef(this)))
                    dependencies.AddIfNotExists(flowBlock);

                if (flowBlock.GetAssociatedFlowBlocks().Contains(registry.GetOriginalRef(this)))
                    dependencies.AddIfNotExists(flowBlock);
            }

            return !dependencies.Any();
        }

        protected virtual void OnAfterReferencedFlowBlocksChanged()
        {
            if (!this.IsLoaded)
                return;

            SyncReferencedFlowBlocksWithInputBehaviorAssignments();
        }

        private void SyncReferencedFlowBlocksWithInputBehaviorAssignments()
        {
            bool anyChange = false;

            // Fehlende hinzufügen
            foreach (var missingFb in _referencedFlowBlocks.Where(x => !this.InputBehaviorAssignments.Any(y => y.FlowBlock == x)))
            {
                this.InputBehaviorAssignments.Add(new InputBehaviorAssignment()
                {
                    Behavior = InputBehavior.Cross,
                    FlowBlock = missingFb
                });

                anyChange = true;
            }

            // Nicht mehr vorhandene entfernen
            foreach (var unusedInputBehaviorAssignment in this.InputBehaviorAssignments
                .Where(x => !_referencedFlowBlocks.Any(y => y == x.FlowBlock))
                .ToList())
            {
                this.InputBehaviorAssignments.Remove(unusedInputBehaviorAssignment);
                anyChange = true;
            }

            // Doppelte FlowBlock-Zuweisungen bereinigen
            var duplicates = this.InputBehaviorAssignments
                .GroupBy(x => x.FlowBlock)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Skip(1))
                .ToList();

            foreach (var duplicate in duplicates)
            {
                this.InputBehaviorAssignments.Remove(duplicate);
                anyChange = true;
            }

            if (anyChange)
                OnPropertyChanged(nameof(this.InputBehaviorAssignments));
        }

        public bool HasInputReference 
        {
            get
            {
                if (this.IterationContext != null)
                    return true;

                return false;
            }
        }

        [Display(Name = "BaseFlowBlock_AssociatedIterationContext", 
            Description = "BaseFlowBlock_AssociatedIterationContext_Tooltip", 
            ResourceType = typeof(FlowBloxTexts), 
            GroupName = "BaseFlowBlock_Groups_Input", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink, ReadOnlyMethod = nameof(GetInputReferenceReadonly), 
            SelectionFilterMethod = nameof(GetPossibleInputReference), 
            SelectionDisplayMember = nameof(Name))]
        [AssociatedFlowBlockResolvableCustom(nameof(IterationContext), nameof(CanDisplayAssociatedIterationContextHint))]
        [JsonProperty("AssociatedIterationContext")]
        public BaseFlowBlock AssociatedIterationContext { get; set; }

        [JsonProperty("AssociatedInputReference")]
        private BaseFlowBlock LegacyAssociatedInputReference
        {
            set
            {
                if (AssociatedIterationContext == null)
                    AssociatedIterationContext = value;
            }
        }

        public bool CanDisplayAssociatedIterationContextHint() => this.ReferencedFlowBlocks.Count() > 1;

        public virtual BaseFlowBlock IterationContext
        {
            get
            {
                if (this.ReferencedFlowBlocks.Count() > 1)
                    return CommonFlowBlockResolver.FindCommonFlowBlock(this);

                return AssociatedIterationContext;
            }
        }

        public bool GetInputReferenceReadonly() => this.ReferencedFlowBlocks.OfType<BaseResultFlowBlock>().Count() > 1;

        [Display(Name = "BaseFlowBlock_InputIgnoreDuplicates", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseFlowBlock_Groups_Input", Order = 1)]
        public bool InputIgnoreDuplicates { get; set; }

        [Display(Name = "BaseFlowBlock_InputBehaviorAssignments", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseFlowBlock_Groups_Input", Order = 2)]
        [FlowBlockUI(Factory = UIFactory.GridView, Operations = UIOperations.Edit)]
        public ObservableCollection<InputBehaviorAssignment> InputBehaviorAssignments { get; set; }

        [Display(Name = "BaseFlowBlock_InheritRequirementsNotMet", ResourceType = typeof(FlowBloxTexts), GroupName = "Global_Groups_Requirements", Order = 0)]
        public bool InheritRequirementsNotMet { get; set; }

        [Display(Name = "BaseFlowBlock_ActivationConditions", ResourceType = typeof(FlowBloxTexts), GroupName = "Global_Groups_Requirements", Order = 2)]
        [FlowBlockUI(
            Factory = UIFactory.GridView,
            UiOptions = UIOptions.EnableFieldSelection,
            CreatableTypes = new[] { 
                typeof(FieldLogicalComparisonCondition), 
                typeof(LogicalGroupCondition) })]
        [FlowBlockDataGrid(GridColumnMemberNames = [
            nameof(LogicalCondition.LogicalOperator),
            nameof(LogicalCondition.DisplayName)
            ])]
        public ObservableCollection<LogicalCondition> ActivationConditions { get; set; } = new ObservableCollection<LogicalCondition>();

        [Display(Name = "BaseFlowBlock_TestDefinitions", Description = "BaseFlowBlock_TestDefinitions_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseFlowBlock_Groups_Tests", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ListView, 
            Operations = UIOperations.Link | UIOperations.Unlink | UIOperations.Create | UIOperations.Edit | UIOperations.Delete,
            SelectionFilterMethod = nameof(GetPossibleTestDefinitions),
            SelectionDisplayMember = nameof(FlowBloxTestDefinition.Name))]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(FlowBloxTestDefinition.Name) })]
        public ObservableCollection<FlowBloxTestDefinition> TestDefinitions { get; set; }

        [Display(Name = "BaseFlowBlock_GenerationStrategies", Description = "BaseFlowBlock_GenerationStrategies_Tooltip", ResourceType = typeof(FlowBloxTexts), GroupName = "BaseFlowBlock_Groups_Tests", Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ListView, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(FlowBloxGenerationStrategyBase.Name) })]
        [CustomValidation(typeof(BaseFlowBlock), nameof(ValidateGenerationStrategies))]
        public ObservableCollection<FlowBloxGenerationStrategyBase> GenerationStrategies { get; set; }

        public static ValidationResult ValidateGenerationStrategies(ObservableCollection<FlowBloxGenerationStrategyBase> generationStrategies, ValidationContext context)
        {
            string message = "";

            foreach (var generationStrategy in generationStrategies)
            {
                if (!generationStrategy.CanExecute(out var testDefinitionToMessages, out var messages))
                {
                    message = string.Join(Environment.NewLine, messages);

                    foreach (var kvp in testDefinitionToMessages)
                    {
                        var testCase = kvp.Key;
                        var messagesForTestCase = kvp.Value;

                        message = string.Join(Environment.NewLine, message, $"Problems with test case \"{testCase.Name}\": {string.Join(", ", messagesForTestCase)}");
                    }
                }
            }

            if (string.IsNullOrEmpty(message))
                return ValidationResult.Success;

            return new ValidationResult(message, [context.MemberName]);
        }

        public List<FlowBloxTestDefinition> GetPossibleTestDefinitions()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var testDefinitions = registry.GetManagedObjects<FlowBloxTestDefinition>();
            return testDefinitions
                .Except(this.TestDefinitions)
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Returns the associated flow blocks that are connected via property definitions.
        /// Does not include flow blocks that are connected via flow logic — use <see cref="ReferencedFlowBlocks"/> for that.
        /// </summary>
        public IEnumerable<BaseFlowBlock> GetAssociatedFlowBlocks() => FlowBlockObjectCollector.CollectFlowBlocks(this);

        protected T GetPreviousFlowBlockOnPath<T>(BaseFlowBlock flowBlock)
            where T : BaseFlowBlock
        {
            return (T)GetPreviousFlowBlockOnPath(flowBlock, [typeof(T)]);
        }

        protected BaseFlowBlock GetPreviousFlowBlockOnPath(BaseFlowBlock flowBlock, Type[] ofTypes)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            if (registry == null)
                return null;

            return GetPreviousFlowBlockOnPath(registry.GetPreviousElements(flowBlock), ofTypes);
        }

        protected BaseFlowBlock GetPreviousFlowBlockOnPath(IEnumerable<BaseFlowBlock> flowBlocks, Type[] ofTypes)
        {
            var flowBlocksOfType = flowBlocks.OfType<BaseFlowBlock>()
                .Where(x => ofTypes.Any(ofType => ofType == x.GetType()));

            if (flowBlocksOfType.Count() == 1)
                return flowBlocksOfType.Single();
            else if (flowBlocksOfType.Count() > 1)
            {
                var typeNames = string.Join(", ", ofTypes.Select(t => t.Name));
                throw new InvalidOperationException($"Multiple matching flow blocks found on path. Expected at most one of: {typeNames}.");
            }
            else
            {
                return flowBlocks.Select(x => GetPreviousFlowBlockOnPath(FlowBloxRegistryProvider.GetRegistry().GetPreviousElements(x), ofTypes))
                    .ExceptNull()
                    .Distinct()
                    .SingleOrDefault();
            }
        }

        protected override void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            PropertyValuesChanged();
            base.OnReferencedFieldNameChanged(field, oldFQFieldName, newFQFieldName);
        }

        public virtual List<BaseFlowBlock> GetPossibleInputReference()
        {
            return FlowBloxRegistryProvider.GetRegistry().GetFlowBlocks()
                .Where(x => x.Name != this.Name)
                .ToList();
        }

        public virtual List<BaseFlowBlock> GetPossibleReferencedElements()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetFlowBlocks()
                .Where(x => x.Name != this.Name)
                .Where(x => x.CanBeReferenced)
                .ToList();
        }

        public override List<FieldElement> GetPossibleFieldElements()
        { 
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var fieldElements = registry.GetFieldElements(true)
                .OrderByDescending(x => this.ReferencedFlowBlocks.Contains(x.Source))
                .ToList();

            return fieldElements;
        }

        public static string GetValidIdentifier(string identifier)
        {
            identifier = IOUtil.GetValidFileName(identifier);
            while ((identifier.IndexOf("_") == 0) || (identifier.IndexOf("-") == 0))
            {
                identifier = identifier.Substring(1);
            }
            return identifier;
        }

        protected void Wait(BaseRuntime runtime)
        {
            if (runtime.StepwiseExecution)
                runtime.Pause = true;

            if (BreakPoint)
                runtime.Pause = true;

            runtime.HandlePause();

            if (runtime.StepTimeunit > 0)
                Thread.Sleep(runtime.StepTimeunit);
        }

        public void SetParentElement(object parentElement)
        {
            if (parentElement is BaseFlowBlock)
            {
                this.Parent = (BaseFlowBlock)parentElement;
            }
        }

        public abstract bool Execute(BaseRuntime runtime, object data);

        public List<BaseFlowBlock> GetNextFlowBlocks()
        {
            return FlowBloxRegistryProvider.GetRegistry().GetInputFlowBlocks()
                .Where(x => x.ReferencedFlowBlocks.Contains(this))
                .OrderBy(x => x.ElementIndex)
                .ToList();
        }

        /// <summary>
        /// Executes the subsequent flow blocks within the current runtime context.
        /// </summary>
        /// <param name="runtime">The runtime instance managing the current flow execution context.</param>
        public virtual void ExecuteNextFlowBlocks(BaseRuntime runtime)
        {
            runtime.TaskRunner.ScheduleNext(this);
        }

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            if (this.HasInputReference)
                runtime.Report($"FlowBlock \"{Name}\": Iteration context is \"{IterationContext.Name}\". Inputs will be collected until iteration end, then this block will execute.");

            OnUndoWarn?.Invoke(runtime);
            OnUndoError?.Invoke(runtime);

            if (this.IterationContext != null)
            {
                this.IterationContext.IterationStart -= InputReference_IterationStart;
                this.IterationContext.IterationStart += InputReference_IterationStart;

                this.IterationContext.IterationEnd -= InputReference_IterationEnd;
                this.IterationContext.IterationEnd += InputReference_IterationEnd;
            }
        }

        public override void RuntimeFinished(BaseRuntime runtime)
        {
            if (this.IterationContext != null)
            {
                this.IterationContext.IterationStart -= InputReference_IterationStart;
                this.IterationContext.IterationEnd -= InputReference_IterationEnd;
            }
        }

        public bool ValidateRequirements(out List<string> messages)
        {
            messages = new List<string>();
            bool isValid = ValidateRequirements(messages);
            return isValid;
        }

        protected bool ValidateRequirements()
        {
            var messages = new List<string>();
            return ValidateRequirements(messages);
        }

        public IEnumerable<RequiredFieldContext> GetRequiredFieldContexts()
        {
            var result = new List<RequiredFieldContext>();

            if (this.HandleRequirements && this.RequiredFields != null)
            {
                result.AddRange(this.RequiredFields.Select(x => new RequiredFieldContext()
                {
                    FieldElement = x,
                    FlowBloxComponent = this
                }));
            }

            result.AddRange(FlowBlockObjectCollector.CollectRequiredFieldContextsRecursive(this));

            return result;
        }

        /// <summary>
        /// Returns all required fields of this component and its associated managed objects (recursively),
        /// including this instance’s own <see cref="FlowBloxComponent.RequiredFields"/> if <see cref="HandleRequirements"/> is true.
        /// </summary>
        public IEnumerable<FieldElement> GetRequiredFields()
        {
            var result = new List<FieldElement>();

            if (this.HandleRequirements && this.RequiredFields != null)
                result.AddRange(this.RequiredFields);

            result.AddRange(FlowBlockObjectCollector.CollectRequiredFieldContextsRecursive(this)
                .Select(x => x.FieldElement));

            return result;
        }

        private bool EvaluateActivationConditions()
        {
            if (ActivationConditions == null || ActivationConditions.Count == 0)
                return true;

            bool? result = null;

            foreach (var condition in ActivationConditions)
            {
                bool current = condition.Check();
                if (result == null)
                {
                    result = current;
                    continue;
                }

                switch (condition.LogicalOperator)
                {
                    case LogicalOperator.And:
                        result = result.Value && current;
                        break;

                    case LogicalOperator.Or:
                        result = result.Value || current;
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported logical operator: {condition.LogicalOperator}");
                }
            }

            return result ?? true;
        }

        protected virtual bool ValidateRequirements(List<string> messages)
        {
            if (InheritRequirementsNotMet &&
                ReferencedFlowBlocks.Any() &&
                ReferencedFlowBlocks.All(x => x.CurrentFlags.HasFlag(FlowBlockFlags.RequirementsNotMet)))
            {
                messages.Add("The activation conditions of the previous FlowBlocks were not met.");
                return false;
            }

            if (!EvaluateActivationConditions())
            {
                var summary = string.Join(" ", ActivationConditions.Select((c, i) => i == 0 ?
                    c.DisplayName :
                    $"{(c.LogicalOperator == LogicalOperator.And ? "and" : "or")} {c.DisplayName}"));

                messages.Add($"Activation conditions were not met: {summary}");
                return false;
            }

            // Check RequiredFields
            var missingRequiredFields = GetRequiredFields()
                .Where(field => field?.Value == null || (field.Value is string s && string.IsNullOrWhiteSpace(s)));

            foreach (var field in missingRequiredFields)
            {
                messages.Add($"Required field \"{field.FullyQualifiedName}\" is not set.");
            }

            return messages.Count == 0;
        }


        private Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>> _passedResults = new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>();

        private Action _storedExecutor;

        protected virtual bool Invoke(BaseRuntime runtime, object data, Action executor)
        {
            if (!runtime.ExecutionFlowEnabled)
                return InvokeExecutor(runtime, executor);

            if (this.HasInputReference)
            {
                var callingFlowBlock = (BaseFlowBlock)data;
                if (callingFlowBlock == null)
                    throw new InvalidOperationException("The calling flow block could not be determined.");

                if (callingFlowBlock is BaseResultFlowBlock)
                {
                    if (!_passedResults.ContainsKey(callingFlowBlock))
                        _passedResults.Add(callingFlowBlock, new HashSet<FlowBlockOut>());

                    var callingFlowBlockResult = ((BaseResultFlowBlock)callingFlowBlock).GridElementResult;
                    if (!_passedResults[callingFlowBlock].Contains(callingFlowBlockResult))
                        _passedResults[callingFlowBlock].Add(callingFlowBlockResult);
                }

                this._storedExecutor = executor;
            }
            else
            {
                return InvokeExecutor(runtime, executor);
            }

            return true;
        }

        public enum BaseFlowBlockNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Error)]
            [Display(Name = "An unexpected error has occurred.")]
            UnexpectedError
        }

        protected virtual bool InvokeExecutor(BaseRuntime runtime, Action executor)
        {
            ResetNotifications(runtime);

            if (!this.ValidateRequirements())
            {
                Flag(runtime, FlowBlockFlags.RequirementsNotMet);
                return false;
            }

            runtime.NotifyInvocationStarted(this);
            try
            {
                executor.Invoke();
                return true;
            }
            catch (GridElementExecutionException e)
            {
                CreateNotification(runtime, BaseFlowBlockNotifications.UnexpectedError, e);
                return false;
            }
            catch (RuntimeCancellationException)
            {
                throw;
            }
            catch (Exception e)
            {
                CreateNotification(runtime, BaseFlowBlockNotifications.UnexpectedError, e);
                return false;
            }
            finally
            {
                runtime.NotifyInvocationFinished(this);
            }
        }

        private void InputReference_IterationStart(BaseRuntime runtime)
        {
            _passedResults.Clear();
        }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        protected int InputDatasets_CurrentIndex { get; private set; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        protected int InputDatasets_Count { get; private set; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public List<FlowBlockOutDataset> InputDatasets { get; private set; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public FlowBlockOutDataset InputDataset_CurrentlyProcessing { get; private set; }

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public FlowBlockFlags CurrentFlags { get; private set; }

        private void InputReference_IterationEnd(BaseRuntime runtime)
        {
            var selector = FlowBlockDatasetSelectorFactory.Create(_passedResults, this.InputBehaviorAssignments);
            var results = selector.GetResults();
            FilterInputDatasets(ref results);

            InputDatasets = results;
            InputDatasets_CurrentIndex = 0;
            InputDatasets_Count = results.Count;

            OnBeforeInputProcessing?.Invoke();

            foreach (var dataset in results)
            {
                runtime.TaskRunner.Enqueue(new InputDatasetWorkItem(
                    block: this,
                    dataset: dataset,
                    applyDatasetAndExecute: (rt, blk, dataset) =>
                    {
                        blk.InputDataset_CurrentlyProcessing = dataset;

                        foreach (var fieldValueMapping in dataset.FieldValueMappings)
                        {
                            fieldValueMapping.Field.SetValue(rt, fieldValueMapping.Value);
                            SetPrecedingFieldValues(rt, blk, fieldValueMapping.PrecedingFieldValues);
                        }
                        blk.InvokeExecutor(rt, blk._storedExecutor);
                        blk.InputDatasets_CurrentIndex++;
                    }
                ));
            }

            InputDataset_CurrentlyProcessing = null;
        }

        private void FilterInputDatasets(ref List<FlowBlockOutDataset> results)
        {
            var _alreadyProcessedDatasets = new HashSet<string>();

            if (this.InputIgnoreDuplicates)
            {
                var filteredResults = new List<FlowBlockOutDataset>();

                foreach (var dataset in results)
                {
                    var datasetHash = string.Join('|', dataset.FieldValueMappings.Select(x => x.Value));
                    if (!_alreadyProcessedDatasets.Contains(datasetHash))
                    {
                        _alreadyProcessedDatasets.Add(datasetHash);
                        filteredResults.Add(dataset);
                    }
                }

                results = filteredResults;
            }
        }

        private void SetPrecedingFieldValues(BaseRuntime runtime, BaseFlowBlock baseFlowBlock, Dictionary<FieldElement, string> precedingFieldValues)
        {
            foreach(var referencedFlowBlock in baseFlowBlock.ReferencedFlowBlocks.OfType<BaseResultFlowBlock>())
            {
                if (referencedFlowBlock == IterationContext)
                    break;

                foreach(var field in referencedFlowBlock.Fields)
                {
                    if (precedingFieldValues.TryGetValue(field, out var value))
                        field.SetValueWithoutEvaluation(runtime, value);
                }
            }
        }

        protected void SetFieldRequirements(IEnumerable<FieldRequiredDefinitionBase> definitions)
        {
            foreach (var definition in definitions)
            {
                if (definition.Field != null)
                    SetFieldRequirement(definition.Field, definition.IsRequired);
            }
        }

        public override void OnAfterSave()
        {
            this.PropertyValuesChanged();
            base.OnAfterSave();
        }

        public virtual List<string> GetDisplayableProperties() => new List<string>() { nameof(Name) };

        public override string ToString() => this.Name;
    }
}
