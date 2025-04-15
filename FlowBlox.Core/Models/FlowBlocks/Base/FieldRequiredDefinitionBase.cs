using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Components;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public abstract class FieldRequiredDefinitionBase
    {
        public abstract FieldElement Field { get; set; }

        public abstract bool IsRequired { get; set; }
    }
}
