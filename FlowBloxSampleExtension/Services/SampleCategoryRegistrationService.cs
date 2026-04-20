using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Services.Base;
using FlowBloxSampleExtension.Models.Components;

namespace FlowBloxSampleExtension.Services
{
    public class SampleCategoryRegistrationService : FlowBlockCategoryRegistrationServiceBase
    {
        public override IEnumerable<FlowBlockCategory> GetAllCategoriesInModule()
        {
            return
            [
                FlowBloxSampleCategories.Sample
            ];
        }
    }
}