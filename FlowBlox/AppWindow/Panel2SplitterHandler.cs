using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.AppWindow
{
    internal class Panel2SplitterHandler
    {
        const float MinimumWidth = 100f;

        bool isSplitterDragging = false;
        private Point dragStartPoint;
        private Panel panelSplitter;
        private TableLayoutPanel tableLayoutPanel;
        private int index;
        private Func<bool> activator;
        private bool adjustLeft;
        private bool adjustRight;
        private float initialLeftWidth, initialRightWidth;

        public Panel2SplitterHandler(TableLayoutPanel tableLayoutPanel, Panel panelSplitter, int index, bool adjustLeft = true, bool adjustRight = true, Func<bool> activator = null)
        {
            this.panelSplitter = panelSplitter;
            this.tableLayoutPanel = tableLayoutPanel;
            this.index = index;
            this.activator = activator;

            this.adjustLeft = adjustLeft;
            this.adjustRight = adjustRight;

            panelSplitter.MouseDown += panelSplitter_MouseDown;
            panelSplitter.MouseMove += panelSplitter_MouseMove;
            panelSplitter.MouseUp += panelSplitter_MouseUp;

            panelSplitter.MouseEnter += PanelSplitter_MouseEnter;
            panelSplitter.MouseLeave += PanelSplitter_MouseLeave;
        }

        private void PanelSplitter_MouseEnter(object sender, EventArgs e)
        {
            if (activator != null && !activator())
                return;

            panelSplitter.Cursor = Cursors.VSplit;
        }

        private void PanelSplitter_MouseLeave(object sender, EventArgs e)
        {
            panelSplitter.Cursor = Cursors.Default;
        }

        private void panelSplitter_MouseDown(object sender, MouseEventArgs e)
        {
            if (activator != null && !activator())
                return;

            if (e.Button == MouseButtons.Left)
            {
                isSplitterDragging = true;
                dragStartPoint = e.Location;
                panelSplitter.Capture = true;

                // Speichere die ursprünglichen Breiten der angrenzenden Spalten
                initialLeftWidth = tableLayoutPanel.ColumnStyles[index - 1].Width;
                initialRightWidth = tableLayoutPanel.ColumnStyles[index + 1].Width;
            }
        }

        private void panelSplitter_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSplitterDragging)
            {
                // Berechne die Verschiebung
                int delta = e.Location.X - dragStartPoint.X;

                // Temporär berechnete neue Breiten, aber nicht zuweisen
                var tempNewLeftWidth = initialLeftWidth + delta;
                var tempNewRightWidth = initialRightWidth - delta;
            }
        }

        private void panelSplitter_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isSplitterDragging)
            {
                isSplitterDragging = false;
                panelSplitter.Capture = false;

                // Berechne die endgültige Verschiebung
                int delta = e.Location.X - dragStartPoint.X;

                // Berechne und weise die neuen Breiten zu, wenn die Maus losgelassen wird
                var newLeftWidth = initialLeftWidth + delta;
                var newRightWidth = initialRightWidth - delta;

                if (adjustLeft && newLeftWidth > MinimumWidth)
                    tableLayoutPanel.ColumnStyles[index - 1].Width = newLeftWidth;

                if (adjustRight && newRightWidth > MinimumWidth)
                    tableLayoutPanel.ColumnStyles[index + 1].Width = newRightWidth;
            }
        }
    }
}
