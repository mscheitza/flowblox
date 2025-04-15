using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestDefinitionUsageResolver
    {
        public List<BaseFlowBlock> ResolveUsages(FlowBloxTestDefinition testDefinition)
        {
            var result = new List<BaseFlowBlock>();
            var registry = FlowBloxRegistryProvider.GetRegistry();
            foreach(var flowBlock in testDefinition.Entries.Select(x => x.FlowBlock))
            {
                if (flowBlock == null)
                    continue;

                if (flowBlock.TestDefinitions.Contains(testDefinition))
                    result.Add(flowBlock);
            }
            return result;
        }
    }
}
