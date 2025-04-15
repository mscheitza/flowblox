using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace FlowBlox.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DisplayAttribute displayAttribute = (DisplayAttribute)fieldInfo.GetCustomAttribute(typeof(DisplayAttribute));
            if (displayAttribute != null)
            {
                var displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false);
                if (!string.IsNullOrEmpty(displayName))
                    return displayName;
            }
            return value.ToString();
        }
    }
}
