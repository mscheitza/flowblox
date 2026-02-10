using System;
using System.Linq;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    public class FormHeightAdjustmentHandler
    {
        private const int DefaultColumnWidth = 150;

        private TableLayoutPanel _tableLayoutPanel;
        private Form _form;
        private bool _adjustedFormHeight;

        public static FormHeightAdjustmentHandler Register(TableLayoutPanel tableLayoutPanel) => new FormHeightAdjustmentHandler(tableLayoutPanel);

        private FormHeightAdjustmentHandler(TableLayoutPanel tableLayoutPanel)
        {
            _tableLayoutPanel = tableLayoutPanel;
            _tableLayoutPanel.Layout += _tableLayoutPanel_Layout;
        }
        
        private void _tableLayoutPanel_Layout(object sender, LayoutEventArgs e)
        {
            if (_form == null)
                _form = _tableLayoutPanel.FindForm();

            if (_form != null)
            {
                _form.Shown -= _form_Shown;
                _form.Shown += _form_Shown;

                _form.Layout -= _form_Layout;
                _form.Layout += _form_Layout;
            }
        }

        private void _form_Layout(object sender, LayoutEventArgs e) => AdjustFormHeight();

        private void _form_Shown(object sender, EventArgs e) => AdjustFormHeight();

        public void AdjustFormHeight()
        {
            if (_adjustedFormHeight)
                return;

            // Stelle sicher, dass das Formular und das TableLayoutPanel gültige Referenzen haben
            if (_form == null || _tableLayoutPanel == null)
                return;

            // Aktuelle Höhe ermitteln
            int formHeight = _form.Height;

            // Berechne die erforderliche Höhe, um alle Zeilen ohne Scrollbalken anzuzeigen
            int requiredHeight = _tableLayoutPanel.GetRowHeights().Sum();

            int difference = requiredHeight - _tableLayoutPanel.ClientSize.Height;
            if (difference > 0)
                formHeight += difference;

            if (formHeight > 1000)
                formHeight = 1000;

            _form.Height = formHeight;

            _adjustedFormHeight = true;
        }
    }
}
