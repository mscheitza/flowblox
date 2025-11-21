using FlowBlox.Core.Models.FlowBlocks.Additions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBlockDatasetSelector
    {
        List<FlowBlockOutDataset> GetResults();
    }
}
