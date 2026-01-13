using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public abstract class BaseSingleResultFlowBlock : BaseResultFlowBlock
    {
        /// <summary>
        /// Default field type for auto-create of the result field. Can be overridden in derived classes. Default: Text
        /// </summary>
        public virtual FieldTypes DefaultResultFieldType => FieldTypes.Text;

        [Display(Name = "PropertyNames_Name", ResourceType = typeof(FlowBloxTexts), Order = -10)]
        [CustomValidation(typeof(FlowBloxComponent), nameof(ValidateName))]
        [Required()]
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ResultField));
            }
        }

        [Display(Name = "Global_ResultField", ResourceType = typeof(FlowBloxTexts), Order = 100)]
        [FlowBlockUI(Factory = UIFactory.Association, SelectionDisplayMember = nameof(Name), Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [Required()]
        public FieldElement ResultField { get; set; }

        public override List<FieldElement> Fields
        {
            get
            {
                var fields = new List<FieldElement>();
                if (this.ResultField != null)
                    fields.Add(this.ResultField);
                return fields;
            }
        }

        protected void CreateDefaultResultField()
        {
            if (this.ResultField == null)
            {
                this.ResultField = FlowBloxRegistryProvider.GetRegistry().CreateField(this,
                    FieldNameGenerationMode.DeriveFromFlowBlock,
                    DefaultResultFieldType);
            }
        }

        public override void OnAfterCreate()
        {
            CreateDefaultResultField();
        }
    }
}
