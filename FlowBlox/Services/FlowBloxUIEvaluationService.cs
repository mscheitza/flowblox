using FlowBlox.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Services
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class FlowBloxUIEvaluationService : IFlowBloxUIEvaluationService
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public bool IsUISupported()
        {
            return true;
        }
    }
}
