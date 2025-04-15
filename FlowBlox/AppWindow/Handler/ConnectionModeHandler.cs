using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Core.Provider;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlowBlox.Core.Enums;
using Org.BouncyCastle.Asn1.Ocsp;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Grid.Provider;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.UICore.Actions;

namespace FlowBlox.AppWindow.Handler
{
    public class ConnectionModeHandler : ModeHandlerBase
    {
        private Point startLocation;
        private Point endLocation;

        private bool isConnecting = false;
        private bool isMouseDown = false;

        private static Color ArrowColor = Color.DarkOrange;
        private FlowBlockUIElement startElement = null;
        private FlowBlockUIElement endElement = null;
        private FlowBloxProjectComponentProvider _componentProvider;

        public ConnectionModeHandler(BackgroundWorker backgroundWorker_PrintGrid, int mouseMoveTimeunit) : base(backgroundWorker_PrintGrid, mouseMoveTimeunit)
        {
            _componentProvider = FlowBloxServiceLocator.Instance.GetService<FlowBloxProjectComponentProvider>();
        }

        public override bool Print(Graphics Graphics)
        {
            if (!isConnecting)
                return false;

            Pen pen = new Pen(ArrowColor, 2.00f);

            pen.DashPattern = new float[] { 5, 2, 5, 2 };
            pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
            Graphics.DrawLine(pen, startLocation, endLocation);

            return true;
        }

        public override void DoMouseDown(MouseButtons buttons, Point location)
        {
            if (buttons == MouseButtons.Left)
            {
                startElement = GetUIElementAtPoint(location);
                if (startElement == null)
                    return;

                startLocation = location;

                isMouseDown = true;
            }
        }

        public override void DoMouseMove(MouseButtons button, Point location)
        {
            if (isMouseDown)
                isConnecting = true;

            if (isConnecting)
            {
                endLocation = location;

                if (!_backgroundWorker_PrintGrid.IsBusy)
                    _backgroundWorker_PrintGrid.RunWorkerAsync(new object[] { _mouseMoveTimeunit, false });
            }
        }

        public override void DoMouseUp(MouseButtons button)
        {
            isMouseDown = false;
            if (isConnecting)
            {
                isConnecting = false;

                endElement = GetUIElementAtPoint(endLocation);

                if (!_backgroundWorker_PrintGrid.IsBusy)
                     _backgroundWorker_PrintGrid.RunWorkerAsync();

                if (!CanConnect(startElement, endElement))
                    return;

                FlowBloxBaseAction connectAction;
                if (startElement.InternalFlowBlock is InvokerFlowBlock)
                {
                    connectAction = new FlowBloxInvokeAction()
                    {
                        From = (InvokerFlowBlock)startElement.InternalFlowBlock,
                        To = endElement.InternalFlowBlock
                    };
                }
                else
                {
                    connectAction = new FlowBloxConnectAction()
                    {
                        From = startElement.InternalFlowBlock,
                        To = endElement.InternalFlowBlock
                    };
                }

                connectAction.Invoke();

                _componentProvider.GetCurrentChangelist().AddChange(connectAction);
            }
        }

        private bool CanConnect(FlowBlockUIElement startElement, FlowBlockUIElement endElement)
        {
            if (startElement == null || endElement == null)
                return false;

            if (startElement == endElement)
                return false;

            if (endElement.InternalFlowBlock.GetInputCardinality() == FlowBlockCardinalities.None)
                return false;

            if (endElement.InternalFlowBlock.GetInputCardinality() == FlowBlockCardinalities.One &&
                endElement.InternalFlowBlock.ReferencedFlowBlocks.Count > 0)
            {
                return false;
            }

            if (endElement.InternalFlowBlock.ReferencedFlowBlocks.Contains(startElement.InternalFlowBlock))
                return false;

            if (startElement.InternalFlowBlock.ReferencedFlowBlocks.Contains(endElement.InternalFlowBlock))
                return false;

            return true;
        }

        public FlowBlockUIElement GetUIElementAtPoint(Point point)
        {
            foreach (var uiElement in _gridUIRegistry.UIElements)
            {
                // Erstellt ein Rechteck basierend auf der Position und der Größe des UIElements.
                Rectangle rect = new Rectangle(uiElement.Location, uiElement.Size);

                // Überprüft, ob sich der Punkt innerhalb des Rechtecks befindet.
                if (rect.Contains(point))
                {
                    return uiElement;
                }
            }
            return null;
        }
    }
}
