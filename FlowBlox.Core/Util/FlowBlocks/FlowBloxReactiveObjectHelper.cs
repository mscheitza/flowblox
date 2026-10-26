using FlowBlox.Core.Models.Base;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public static class FlowBloxReactiveObjectHelper
    {
        public static bool IsReactiveObjectOnly(object target)
            => target is FlowBloxReactiveObject reactiveObject &&
               IsReactiveObjectOnly(reactiveObject);

        public static bool IsReactiveObjectOnly(FlowBloxReactiveObject target)
            => target != null && target is not FlowBloxComponent;

        public static bool IsReactiveObjectOnly(Type type)
            => type != null &&
               typeof(FlowBloxReactiveObject).IsAssignableFrom(type) &&
               !typeof(FlowBloxComponent).IsAssignableFrom(type);
    }
}
