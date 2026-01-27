using FlowBlox.Core;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Grid.Elements.UI.CustomActions
{
    public class ExcelTableUIActions : ComponentUIActions<ExcelTable>
    {
        private readonly object? _dataSourceUiActions;

        public ExcelTableUIActions(ExcelTable component) : base(component)
        {
            if (component.DataSource != null)
                _dataSourceUiActions = UIActionHelper.GetComponentUIActionForType(component.DataSource.GetType(), component.DataSource);
        }

        public SKImage OpenIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_edit, 16, SKColors.DodgerBlue);

        public bool CanOpen()
        {
            return UIActionHelper.InvokeUIActionMethod<bool>(_dataSourceUiActions, nameof(CanOpen));
        }

        [Display(Name = "ExcelTableUIActions_Open", ResourceType = typeof(FlowBloxTexts))]
        public void Open()
        {
            UIActionHelper.InvokeUIActionMethod(_dataSourceUiActions, nameof(Open));
        }
    }
}