using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.UICore.Interfaces
{
    public abstract class ComponentUIActions<T> where T : IFlowBloxComponent
    {
        public T Component { get; set; }

        protected ComponentUIActions(T component)
        {
            Component = component;
        }
    }
}
