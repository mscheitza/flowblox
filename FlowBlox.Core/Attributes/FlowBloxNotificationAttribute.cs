using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Attributes
{
    public enum NotificationType
    {
        [Display(Name = "NotificationType_None", ResourceType = typeof(FlowBloxTexts))]
        None,
        [Display(Name = "NotificationType_Warning", ResourceType = typeof(FlowBloxTexts))]
        Warning,
        [Display(Name = "NotificationType_Error", ResourceType = typeof(FlowBloxTexts))]
        Error
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FlowBloxNotificationAttribute : Attribute
    {
        public FlowBloxNotificationAttribute()
        {
            
        }

        public NotificationType NotificationType { get; set; }
    }
}
