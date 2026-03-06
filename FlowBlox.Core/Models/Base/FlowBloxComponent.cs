using FlowBlox.Core.Attributes;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace FlowBlox.Core.Models.Base
{
    [FlowBlockUIGroup("Global_Groups_Requirements", 50)]
    public abstract class FlowBloxComponent : FlowBloxReactiveObject, IFlowBloxComponent
    {
        private string _name;

        [Required()]
        [Display(Name = "PropertyNames_Name", ResourceType = typeof(FlowBloxTexts), Order = -1)]
        [CustomValidation(typeof(FlowBloxComponent), nameof(ValidateName))]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public static ValidationResult ValidateName(string name, ValidationContext context) => FlowBloxComponentNameValidator.Validate(name, context);

        public virtual bool HandleRequirements => true;

        public bool ShouldSerializeRequiredFields() => HandleRequirements;

        [ActivationCondition(MemberName = nameof(HandleRequirements), Value = true)]
        [Display(Name = "FlowBloxComponent_RequiredFields", ResourceType = typeof(FlowBloxTexts), GroupName = "Global_Groups_Requirements", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ListView, Operations = UIOperations.Link | UIOperations.Unlink,
            UiOptions = UIOptions.FieldSelectionHideRequired | UIOptions.FieldSelectionDefaultNotRequired,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(GetPossibleFieldElements))]
        [FlowBlockListView(LVColumnMemberNames = [nameof(FieldElement.FlowBlockName), nameof(FieldElement.Name)])]
        public ObservableCollection<FieldElement> RequiredFields { get; set; } = new ObservableCollection<FieldElement>();

        public virtual List<FieldElement> GetPossibleFieldElements()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFieldElements(true)
                .ToList();
        }

        public void SetFieldRequirement(FieldElement fieldElement, bool isRequired)
        {
            if (fieldElement == null)
                throw new ArgumentNullException(nameof(fieldElement));

            if (!IsLoaded)
                return;

            if (isRequired)
            {
                if (!RequiredFields.Contains(fieldElement))
                {
                    RequiredFields.Add(fieldElement);
                    OnPropertyChanged(nameof(RequiredFields));
                }
            }
            else
            {
                if (RequiredFields.Contains(fieldElement))
                {
                    RequiredFields.Remove(fieldElement);
                    OnPropertyChanged(nameof(RequiredFields));
                }
            }
        }

        protected void SetRequiredInputField(
            ref FieldElement storage,
            FieldElement value,
            string[] additionalNotifies = null,
            [CallerMemberName] string propertyName = null)
        {
            if (ReferenceEquals(storage, value))
                return;

            if (storage != null)
                SetFieldRequirement(storage, isRequired: false);

            storage = value;

            if (storage != null)
                SetFieldRequirement(storage, isRequired: true);

            OnPropertyChanged(propertyName);

            if (additionalNotifies != null)
                foreach (var n in additionalNotifies)
                    OnPropertyChanged(n);
        }

        public string Version { get; set; }

        [JsonIgnore]
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.component, 16);

        [JsonIgnore]
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.component, 32);

        /// <summary>
        /// Indicates whether the component has finished loading.
        /// This flag is relevant for event handling: certain events should be suppressed
        /// during the component's loading phase and only executed after it is fully initialized.
        /// </summary>
        protected bool IsLoaded { get; private set; }

        private string GetComponentAssemblyVersion()
        {
            return GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0.0";
        }

        protected FlowBloxComponent()
        {
            Version = GetComponentAssemblyVersion();
        }

        public virtual void RegisterPropertyChangedEventHandlers()
        {
            foreach (var fieldElement in GetAssociatedFields().ExceptNull())
            {
                fieldElement.OnNameChanged -= OnReferencedFieldNameChanged;
                fieldElement.OnNameChanged += OnReferencedFieldNameChanged;
            }
        }

        /// <summary>
        /// Called when a referenced field has been renamed.
        /// </summary>
        /// <param name="field">
        /// The <see cref="FieldElement"/> instance representing the field whose name has changed.
        /// </param>
        /// <param name="oldFQFieldName">
        /// The fully qualified field name before the change.
        /// </param>
        /// <param name="newFQFieldName">
        /// The fully qualified field name after the change.
        /// </param>
        /// <remarks>
        /// This method is responsible for updating all string properties of this FlowBlock
        /// that reference the renamed field via field selection expressions.
        /// The default implementation automatically locates and updates all affected properties
        /// using reflection-based field collectors.  
        /// Override this method only if additional or specialized update logic is required.
        /// </remarks>
        protected virtual void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            var reflectionData = FlowBlockObjectCollector.CollectStringPropertiesContainingFields(this);
            foreach (var (property, instance) in reflectionData)
            {
                
                var currentValue = (string)property.GetValue(instance);
                if (string.IsNullOrWhiteSpace(currentValue))
                    continue;

                string replacedValue;
                replacedValue = FlowBloxFieldHelper.ReplaceFQName(currentValue, oldFQFieldName, newFQFieldName);

                // Only write back if something actually changed
                if (string.Equals(currentValue, replacedValue, StringComparison.Ordinal))
                    continue;

                property.SetValue(instance, replacedValue);
            }
        }

        /// <summary>
        /// Returns all <see cref="FieldElement"/> instances that are referenced through property definitions.
        /// This includes direct associations, field references in string-based properties with field selection enabled,
        /// and any nested field references within associated reactive objects.
        /// </summary>
        /// <returns>A list of associated <see cref="FieldElement"/> instances.</returns>
        public IEnumerable<FieldElement> GetAssociatedFields() => FlowBlockObjectCollector.CollectReferencedFieldElementsRecursive(this);

        /// <summary>
        /// Returns the associated managed objects that are connected via property definitions.
        /// </summary>
        public IEnumerable<IManagedObject> GetAssociatedManagedObjects() => FlowBlockObjectCollector.CollectManagedObjectsRecursive(this);

        public virtual bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            dependencies = new List<IFlowBloxComponent>();
            return true;
        }

        public virtual void OnAfterCreate()
        {
            
        }

        /// <summary>
        /// Called after the component has been fully loaded and initialized.
        /// Override this method to perform additional initialization that depends on a fully constructed state.
        /// <para>
        /// When overriding this method, make sure to call <c>base.OnAfterLoad()</c> first,
        /// before executing any component-specific logic.
        /// </para>
        /// </summary>
        public virtual void OnAfterLoad()
        {
            this.RegisterPropertyChangedEventHandlers();
            this.IsLoaded = true;
        }

        public virtual void OnBeforeSave()
        {
            // Ensure RequiredFields only contains fields that are also associated via property definitions
            var associatedFields = new HashSet<FieldElement>(this.GetAssociatedFields());

            var toRemove = this.RequiredFields
                .Where(required => !associatedFields.Contains(required))
                .ToList();

            foreach (var field in toRemove)
            {
                this.RequiredFields.Remove(field);
            }
        }

        public virtual void OnAfterSave()
        {
            this.RegisterPropertyChangedEventHandlers();
        }

        /// <summary>
        /// Initializes option defaults for this component or FlowBlock.
        /// This overload also receives the currently stored options, allowing the implementation to choose defaults dynamically.
        /// </summary>
        /// <param name="defaults">A list to which default option definitions are added.</param>
        /// <param name="currentOptions">The existing configured options, enabling conditional initialization</param>
        public virtual void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions) => OptionsInit(defaults);


        /// <summary>
        /// Initializes default options for this component or FlowBlock.
        /// Derived classes should override this method to declare their configurable default settings. 
        /// These defaults can be modified by the user.
        /// </summary>
        /// <param name="defaults">A list to which default option definitions are added.</param>
        public virtual void OptionsInit(List<OptionElement> defaults)
        {
            // This method can be implemented in derived classes.
        }

        public virtual void RuntimeFinished(BaseRuntime runtime)
        {
            // This method can be implemented in derived classes.
        }

        public virtual void RuntimeStarted(BaseRuntime runtime)
        {
            // This method can be implemented in derived classes.
        }
    }
}
