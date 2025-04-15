using System.Collections.Generic;
using System.Drawing;
using FlowBlox.Grid;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Core.Provider;

namespace FlowBlox.AppWindow.Contents
{
    // Erster Versuch einer automatischen Ausrichtung der Grid Elemente.
    //
    // Wichtige Hinweise:
    //
    // - Prototyp
    // - Wird derzeit nicht verwendet
    // - Die Funktion UF_LayoutGrid ist nicht ausreichend getestet

    public partial class ProjectPanel
    {
        private const int GridBlockWidth = 360;
        private const int GridBlockHeight = 180;

        private Dictionary<string, FlowBlockUIElement> _blockMap = new Dictionary<string, FlowBlockUIElement>();

        private void UF_LayoutGrid()
        {
            this._blockGridUpdate = false;

            mainPanel.VerticalScroll.Value = 0;
            mainPanel.HorizontalScroll.Value = 0;

            _blockMap.Clear();

            foreach (var uiElement in FlowBloxUIRegistry.UIElements)
            {
                LayoutElement(uiElement);
            }
        }

        private void UF_LayoutElement(FlowBlockUIElement uiElement)
        {
            this._blockGridUpdate = false;
            _blockMap.Clear();
            LayoutElement(uiElement);
        }

        private Point GetGridLocationFromBlockLocation(FlowBlockUIElement element, int X, int Y, int BlocksReserved)
        {
            int GridLocationX = X * GridBlockWidth;
            int GridLocationY = Y * GridBlockHeight;
            GridLocationY += ((GridBlockHeight * BlocksReserved) - element.Height) / 2;
            return new Point(GridLocationX, GridLocationY);
        }

        private void AppendBlock(FlowBlockUIElement uiElement, int BlockLocationX, int BlockLocationY)
        {
            int BlocksReserved = (uiElement.Height / GridBlockHeight) + 1;

            for (int iCounter = 0; iCounter < BlocksReserved; iCounter++)
            {
                int BlockLocationY2 = BlockLocationY + iCounter;
                string LocationKey = "X" + BlockLocationX + "Y" + BlockLocationY2;
                _blockMap[LocationKey] = uiElement;
            }

            uiElement.Location = GetGridLocationFromBlockLocation(uiElement, BlockLocationX, BlockLocationY, BlocksReserved);
        }

        private void LayoutElement(FlowBlockUIElement uiElement)
        {
            int BlockLocationX = uiElement.Location.X / (GridBlockWidth);
            int BlockLocationY = uiElement.Location.Y / (GridBlockHeight);

            string LocationKey = "X" + BlockLocationX + "Y" + BlockLocationY;

            if (!_blockMap.ContainsKey(LocationKey))
            {
                AppendBlock(uiElement, BlockLocationX, BlockLocationY);
            }
            else
            {
                int BlockLocationX2 = (uiElement.Location.X + uiElement.Width) / GridBlockWidth;
                int BlockLocationY2 = (uiElement.Location.Y + uiElement.Height) / GridBlockHeight;
                
                string LocationKey2 = "X" + BlockLocationX2 + "Y" + BlockLocationY;
                string LocationKey3 = "X" + BlockLocationX2 + "Y" + BlockLocationY2;

                if (!_blockMap.ContainsKey(LocationKey2))
                {
                    AppendBlock(uiElement, BlockLocationX2, BlockLocationY);
                }
                else if (!_blockMap.ContainsKey(LocationKey3))
                {
                    AppendBlock(uiElement, BlockLocationX2, BlockLocationY);
                }
            }
        }
	}
}
