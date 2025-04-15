using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBloxCategoryRegistrationService
    {
        void Register();
        void Unregister();
    }
}
