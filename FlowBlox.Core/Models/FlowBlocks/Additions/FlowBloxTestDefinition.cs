using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Interfaces;
using CsvHelper;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FlowBlox.Core.Models.Base;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    public class FlowBloxTestDefinition : ManagedObject, INotifyPropertyChanged
    {
        public List<FlowBlockTestDataset> Entries { get; set; }

        [Display(Name = "FlowBloxTestDefinition_RequiredForExecution", ResourceType = typeof(FlowBloxTexts))]
        public bool RequiredForExecution { get; set; }

        public FlowBloxTestDefinition()
        {
            Entries = new List<FlowBlockTestDataset>();
        }
    }
}
