using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Services.Base;
using FlowBlox.Core.Util.Resources;
using System.Collections.Generic;

namespace FlowBlox.Core.Services
{
    public sealed class FlowBlockCategoryRegistrationService : FlowBlockCategoryRegistrationServiceBase
    {
        public override IEnumerable<FlowBlockCategory> GetAllCategoriesInModule()
        {
            return
            [
                FlowBlockCategory.Calculation,
                FlowBlockCategory.Logic,
                FlowBlockCategory.Persistence,
                FlowBlockCategory.TextOperations,
                FlowBlockCategory.Additional,
                FlowBlockCategory.Selection,
                FlowBlockCategory.Web,
                FlowBlockCategory.IO,
                FlowBlockCategory.Generation,
                FlowBlockCategory.ControlFlow,
                FlowBlockCategory.AI,
                FlowBlockCategory.Serialization,
                FlowBlockCategory.Xml,
                FlowBlockCategory.Json,
                FlowBlockCategory.Extensions
            ];
        }
    }
}