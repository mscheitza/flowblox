using FlowBlox.Core;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.FlowBlocks.AI
{
    public class TokenTranslation : FlowBloxReactiveObject
    {
        [Display(Name = "TokenTranslation_SourceCharacter", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [MinLength(1)]
        [MaxLength(1)]
        [Required()]
        public string SourceCharacter { get; set; }

        [Display(Name = "TokenTranslation_DestinationCharacter", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [MinLength(1)]
        [MaxLength(1)]
        public string DestinationCharacter { get; set; }

        [Display(Name = "TokenTranslation_SpecialCharacter", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public SpecialCharacter? SpecialCharacter { get; set; }
    }
}