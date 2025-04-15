using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.Resources;

namespace FlowBloxSampleExtension.Model
{
    public static class FlowBloxSampleCategories
    {
        public static FlowBlockCategory Sample => new FlowBlockCategory(Get("SampleCategory_Sample"));
    
        private static string Get(string resourceKey)
        {
            return FlowBloxResourceUtil.GetLocalizedString(resourceKey, typeof(SampleExtensionResources));
        }
    }
}
