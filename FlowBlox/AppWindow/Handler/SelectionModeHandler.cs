using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Core.Provider;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowBlox.AppWindow.Handler
{
    public class SelectionModeHandler : ModeHandlerBase
    {
        private static Color SelectionRectangleColor = Color.CadetBlue;

        private Point InitialCursorLocation;
        private Point RecentCursorLocation;
        private Point InitialMouseLocation;

        private bool isMouseCaptured = false;

        public SelectionModeHandler(BackgroundWorker backgroundWorker_PrintGrid, int mouseMoveTimeunit) : base(backgroundWorker_PrintGrid, mouseMoveTimeunit)
        {

        }

        public override bool Print(Graphics Graphics)
        {
            if (!isMouseCaptured)
                return false;

            Point Difference = RecentCursorLocation;

            Difference.X -= InitialCursorLocation.X;
            Difference.Y -= InitialCursorLocation.Y;

            Point RecentMouseLocation = new Point
                (
                    InitialMouseLocation.X + Difference.X,
                    InitialMouseLocation.Y + Difference.Y
                );

            // Abstand ermitteln
            Difference.X = Difference.X < 0 ? Difference.X * -1 : Difference.X;
            Difference.Y = Difference.Y < 0 ? Difference.Y * -1 : Difference.Y;

            float[] dashValues = { 5, 2, 5, 2 };
            Pen Pen = new Pen(SelectionRectangleColor, 2.00f);
            Pen.DashPattern = dashValues;

            if (RecentMouseLocation.X > InitialMouseLocation.X)
            {
                if (RecentMouseLocation.Y > InitialMouseLocation.Y)
                {
                    Graphics.DrawRectangle(Pen, InitialMouseLocation.X, InitialMouseLocation.Y, Difference.X, Difference.Y);
                }
                else
                {
                    Graphics.DrawRectangle(Pen, InitialMouseLocation.X, RecentMouseLocation.Y, Difference.X, Difference.Y);
                }
            }
            else
            {
                if (RecentMouseLocation.Y > InitialMouseLocation.Y)
                {
                    Graphics.DrawRectangle(Pen, RecentMouseLocation.X, InitialMouseLocation.Y, Difference.X, Difference.Y);
                }
                else
                {
                    Graphics.DrawRectangle(Pen, RecentMouseLocation.X, RecentMouseLocation.Y, Difference.X, Difference.Y);
                }
            }

            return true;
        }

        public override void DoMouseDown(MouseButtons button, Point location)
        {
            if (button == MouseButtons.Left)
            {
                InitialCursorLocation = Cursor.Position;
                RecentCursorLocation = Cursor.Position;
                InitialMouseLocation = location;
                isMouseCaptured = true;
            }
        }

        public override void DoMouseMove(MouseButtons Button, Point location)
        {
            if (Button == MouseButtons.Left)
            {
                if (isMouseCaptured)
                {
                    RecentCursorLocation = Cursor.Position;

                    if (!_backgroundWorker_PrintGrid.IsBusy)
                        _backgroundWorker_PrintGrid.RunWorkerAsync(new object[] { _mouseMoveTimeunit, false });
                }
            }
        }

        public override void DoMouseUp(MouseButtons Button)
        {
            if (Button == MouseButtons.Left)
            {
                isMouseCaptured = false;

                if (!_backgroundWorker_PrintGrid.IsBusy)
                    _backgroundWorker_PrintGrid.RunWorkerAsync();

                int DifferenceX = RecentCursorLocation.X - InitialCursorLocation.X;
                int DifferenceY = RecentCursorLocation.Y - InitialCursorLocation.Y;

                Point RecentMouseLocation = new Point
                    (
                        InitialMouseLocation.X + DifferenceX,
                        InitialMouseLocation.Y + DifferenceY
                    );

                foreach (var uiElement in _gridUIRegistry.UIElements)
                {
                    uiElement.MarkElement(ElementState.Unmarked);
                }

                foreach (var uiElement in _gridUIRegistry.UIElements)
                {
                    if (RecentMouseLocation.X > InitialMouseLocation.X)
                    {
                        if (RecentMouseLocation.Y > InitialMouseLocation.Y)
                        {
                            if (uiElement.Location.X > InitialMouseLocation.X &&
                                uiElement.Location.X < RecentMouseLocation.X &&
                                uiElement.Location.Y > InitialMouseLocation.Y &&
                                uiElement.Location.Y < RecentMouseLocation.Y)
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                        else
                        {
                            if (uiElement.Location.X > InitialMouseLocation.X &&
                                uiElement.Location.X < RecentMouseLocation.X &&
                                uiElement.Location.Y > RecentMouseLocation.Y &&
                                uiElement.Location.Y < InitialMouseLocation.Y)
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                    }
                    else
                    {
                        if (RecentMouseLocation.Y > InitialMouseLocation.Y)
                        {
                            if (uiElement.Location.X > RecentMouseLocation.X &&
                                uiElement.Location.X < InitialMouseLocation.X &&
                                uiElement.Location.Y > InitialMouseLocation.Y &&
                                uiElement.Location.Y < RecentMouseLocation.Y)
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                        else
                        {
                            if (uiElement.Location.X > RecentMouseLocation.X &&
                                uiElement.Location.X < InitialMouseLocation.X &&
                                uiElement.Location.Y > RecentMouseLocation.Y &&
                                uiElement.Location.Y < InitialMouseLocation.Y)
                            {
                                uiElement.MarkElement(ElementState.Marked);
                            }
                        }
                    }
                }
            }
        }
    }
}
