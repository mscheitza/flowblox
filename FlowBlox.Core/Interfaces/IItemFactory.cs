using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Interfaces
{
    public interface IItemFactory<out T> where T : IFlowBloxComponent
    {
        public T Create();
    }
}
