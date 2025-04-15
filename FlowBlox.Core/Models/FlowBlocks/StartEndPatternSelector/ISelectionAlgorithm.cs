using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.StartEndPatternSelector
{
    public interface ISelectionAlgorithm
    {
        List<string> Select(string content, string startPattern, string endPattern, bool enableMultiline);
    }
}
