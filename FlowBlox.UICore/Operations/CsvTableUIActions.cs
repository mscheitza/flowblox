using FlowBlox.Core;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Util;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace FlowBlox.Grid.Elements.UI.CustomActions
{
    public class CsvTableUIActions : ComponentUIActions<CsvTable>
    {
        private readonly object? _dataSourceUiActions;

        public CsvTableUIActions(CsvTable component) : base(component)
        {
            if (component.DataSource != null)
                _dataSourceUiActions = UIActionHelper.GetComponentUIActionForType(component.DataSource.GetType(), component.DataSource);
        }

        public bool CanOpen()
        {
            return UIActionHelper.InvokeUIActionMethod<bool>(_dataSourceUiActions, nameof(CanOpen));
        }

        [Display(Name = "CsvTableUIActions_Open", ResourceType = typeof(FlowBloxTexts))]
        public void Open()
        {
            UIActionHelper.InvokeUIActionMethod(_dataSourceUiActions, nameof(Open));
        }
    }
}