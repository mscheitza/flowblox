using System.Collections.Generic;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.Models.DTO
{
    internal class BatchInfo
    {
        public string FilePath { get; }

        public List<FieldElement> UserFields { get; } = new List<FieldElement>();

        public BatchInfo(string filePath, List<FieldElement> userFields)
        {
            FilePath = filePath;
            UserFields = userFields;
        }
    }
}