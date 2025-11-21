using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Interfaces
{
    public interface IOptionsRegistration
    {
        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions);
    }
}
