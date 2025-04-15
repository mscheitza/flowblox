using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer
{
    public class QATokenizedInput
    {
        public long[] InputIds { get; set; }
        public long[] AttentionMask { get; set; }
        public long[] TokenTypeIds { get; set; }
    }
}
