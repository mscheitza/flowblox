using FlowBlox.Core.Util.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;


namespace FlowBlox.Core.Extensions
{
    public static class EnumExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : Attribute
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<TAttribute>();
        }

        public static string GetLocalizedEnumName(this Enum value)
        {
            var displayAttribute = value.GetAttribute<DisplayAttribute>();
            if (displayAttribute == null)
                return value.ToString();

            return FlowBloxResourceUtil.GetDisplayName(displayAttribute);
        }
    }
}
