using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Services.Base;

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
                FlowBlockCategory.Communication,
                FlowBlockCategory.Authorization,
                FlowBlockCategory.AI,
                FlowBlockCategory.ShellExecution,
                FlowBlockCategory.Compression,
                FlowBlockCategory.Serialization,
                FlowBlockCategory.Xml,
                FlowBlockCategory.Json,
                FlowBlockCategory.Extensions
            ];
        }
    }
}
