using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Enums
{
    public enum FieldTypes
    {
        [Display(Name = "Text")]
        Text,

        [Display(Name = "Integer")]
        Integer,

        [Display(Name = "Long")]
        Long,

        [Display(Name = "Float")]
        Float,

        [Display(Name = "Double")]
        Double,

        [Display(Name = "Boolean")]
        Boolean,

        [Display(Name = "DateTime")]
        DateTime,

        [Display(Name = "Byte Array")]
        ByteArray
    }
}