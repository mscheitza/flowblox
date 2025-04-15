using FlowBlox.Core.Enums;
using Microsoft.Identity.Client;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class FlowBlockNotificationAttribute : Attribute
    {
        public FlowBlockNotificationAttribute()
        {
            
        }

        public NotificationType NotificationType { get; set; }
    }
}
