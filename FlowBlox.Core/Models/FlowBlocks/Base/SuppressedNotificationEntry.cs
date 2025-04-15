using FlowBlox.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    /// <summary>
    /// Represents an override configuration for notification types assigned to enum values.
    /// </summary>
    public class OverriddenNotificationEntry
    {
        public OverriddenNotificationEntry()
        {
            this.Overrides = new Dictionary<long, NotificationType>();
        }

        /// <summary>
        /// The assembly-qualified name of the enum type this override applies to.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Maps enum values (as long) to user-defined NotificationType overrides.
        /// </summary>
        public Dictionary<long, NotificationType> Overrides { get; set; }
    }
}
