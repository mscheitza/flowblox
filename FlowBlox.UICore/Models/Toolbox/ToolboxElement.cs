using System;
using System.IO;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using Newtonsoft.Json;

namespace FlowBlox.UICore.Models.Toolbox
{
    [Serializable()]
    public class ToolboxElement
    {
        public string ToolboxCategory { get; set; } 

        public string Name { get; set; }

        public string Content { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public string UnderlyingToolboxFile { get; set; }

        [JsonIgnore]
        public bool IsEditable { get; set; }
    }
}
