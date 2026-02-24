using System;
using System.Collections.Generic;

namespace FlowBlox.Core.Models.Project
{
    public class FlowBloxProjectLocalData
    {
        /// <summary>
        /// Unique identifier of the project space.
        /// </summary>
        public string ProjectSpaceGuid { get; set; }

        /// <summary>
        /// Local project space version.
        /// </summary>
        public int? ProjectSpaceVersion { get; set; }

        /// <summary>
        /// Endpoint URI of the project space backend service.
        /// </summary>
        public string ProjectSpaceEndpointUri { get; set; }

        /// <summary>
        /// Local values for user fields.
        /// Key = Field identifier, Value = Local value.
        /// </summary>
        public Dictionary<string, string> LocalUserFieldValues { get; set; } = new();
    }
}