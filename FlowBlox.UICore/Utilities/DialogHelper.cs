using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlowBlox.UICore.Utilities
{
    public static class DialogHelper
    {
        public static FieldSelectionWindowResult InvokeFieldSelectionWindow(object target, FlowBlockUIAttribute flowBlockUI, IList items, Window window, FieldSelectionMode[] fieldSelectionModes)
        {
            var args = new FieldSelectionWindowArgs
            {
                FlowBlock = target as BaseFlowBlock,
                FieldElements = items.OfType<FieldElement>(),
                SelectionMode = FieldSelectionMode.Fields,
                AllowedFieldSelectionModes = fieldSelectionModes,
                IsRequired = !flowBlockUI.UiOptions.HasFlag(UIOptions.FieldSelectionDefaultNotRequired),
                HideRequired = flowBlockUI.UiOptions.HasFlag(UIOptions.FieldSelectionHideRequired)
            };

            var win = new Views.FieldSelectionWindow(args)
            {
                Owner = window,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (win.ShowDialog() != true || win.Result == null)
                return null;

            var result = win.Result;

            return result;
        }
    }
}
