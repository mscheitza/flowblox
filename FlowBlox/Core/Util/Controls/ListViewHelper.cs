using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;

namespace FlowBlox.Core.Util.Controls
{
    internal enum ListViewExtendedStyles
    {
        /// <summary>
        /// LVS_EX_GRIDLINES
        /// </summary>
        GridLines = 0x00000001,
        /// <summary>
        /// LVS_EX_SUBITEMIMAGES
        /// </summary>
        SubItemImages = 0x00000002,
        /// <summary>
        /// LVS_EX_CHECKBOXES
        /// </summary>
        CheckBoxes = 0x00000004,
        /// <summary>
        /// LVS_EX_TRACKSELECT
        /// </summary>
        TrackSelect = 0x00000008,
        /// <summary>
        /// LVS_EX_HEADERDRAGDROP
        /// </summary>
        HeaderDragDrop = 0x00000010,
        /// <summary>
        /// LVS_EX_FULLROWSELECT
        /// </summary>
        FullRowSelect = 0x00000020,
        /// <summary>
        /// LVS_EX_ONECLICKACTIVATE
        /// </summary>
        OneClickActivate = 0x00000040,
        /// <summary>
        /// LVS_EX_TWOCLICKACTIVATE
        /// </summary>
        TwoClickActivate = 0x00000080,
        /// <summary>
        /// LVS_EX_FLATSB
        /// </summary>
        FlatsB = 0x00000100,
        /// <summary>
        /// LVS_EX_REGIONAL
        /// </summary>
        Regional = 0x00000200,
        /// <summary>
        /// LVS_EX_INFOTIP
        /// </summary>
        InfoTip = 0x00000400,
        /// <summary>
        /// LVS_EX_UNDERLINEHOT
        /// </summary>
        UnderlineHot = 0x00000800,
        /// <summary>
        /// LVS_EX_UNDERLINECOLD
        /// </summary>
        UnderlineCold = 0x00001000,
        /// <summary>
        /// LVS_EX_MULTIWORKAREAS
        /// </summary>
        MultilWorkAreas = 0x00002000,
        /// <summary>
        /// LVS_EX_LABELTIP
        /// </summary>
        LabelTip = 0x00004000,
        /// <summary>
        /// LVS_EX_BORDERSELECT
        /// </summary>
        BorderSelect = 0x00008000,
        /// <summary>
        /// LVS_EX_DOUBLEBUFFER
        /// </summary>
        DoubleBuffer = 0x00010000,
        /// <summary>
        /// LVS_EX_HIDELABELS
        /// </summary>
        HideLabels = 0x00020000,
        /// <summary>
        /// LVS_EX_SINGLEROW
        /// </summary>
        SingleRow = 0x00040000,
        /// <summary>
        /// LVS_EX_SNAPTOGRID
        /// </summary>
        SnapToGrid = 0x00080000,
        /// <summary>
        /// LVS_EX_SIMPLESELECT
        /// </summary>
        SimpleSelect = 0x00100000
    }

    internal enum ListViewMessages
    {
        First = 0x1000,
        SetExtendedStyle = First + 54,
        GetExtendedStyle = First + 55,
    }

    /// <summary>
    /// Contains helper methods to change extended styles on ListView, including enabling double buffering.
    /// </summary>
    internal class ListViewHelper
    {
        public enum MoveDirections { MoveUp = -1, MoveDown = 1 }

        private ListViewHelper()
        {
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr handle, int messg, int wparam, int lparam);

        public static void SetExtendedStyle(Control control, ListViewExtendedStyles exStyle)
        {
            ListViewExtendedStyles styles;
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            styles |= exStyle;
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }

        public static void EnableDoubleBuffer(Control control)
        {
            ListViewExtendedStyles styles;
            // read current style
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            // enable double buffer and border select
            styles |= ListViewExtendedStyles.DoubleBuffer | ListViewExtendedStyles.BorderSelect;
            // write new style
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }
        public static void DisableDoubleBuffer(Control control)
        {
            ListViewExtendedStyles styles;
            // read current style
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            // disable double buffer and border select
            styles -= styles & ListViewExtendedStyles.DoubleBuffer;
            styles -= styles & ListViewExtendedStyles.BorderSelect;
            // write new style
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }
        private static void SwapLvItems(int selIdx1, int selIdx2, ListView.ListViewItemCollection currentitems)
        {
            string cache;
            for (int i = 0; i < currentitems[selIdx1].SubItems.Count; i++)
            {
                cache = currentitems[selIdx2].SubItems[i].Text;
                currentitems[selIdx2].SubItems[i].Text =
                  currentitems[selIdx1].SubItems[i].Text;
                currentitems[selIdx1].SubItems[i].Text = cache;
            }
            currentitems[selIdx1].Selected = false;
            currentitems[selIdx2].Selected = true;
        }
        public static void MoveListViewItem(ref ListView lv, MoveDirections move)
        {
            ListViewItem currentitem = lv.SelectedItems[0];
            ListViewGroup currentgroup = currentitem.Group;
            int selIdx = currentgroup.Items.IndexOf(currentitem);
            if (selIdx == 0 && move == MoveDirections.MoveUp)
            {
                //Replace these lines with return;, to eliminate wrap around
                currentgroup.Items.RemoveAt(0);
                currentgroup.Items.Add(currentitem);
            }
            else
                if (selIdx == currentgroup.Items.Count - 1 && move == MoveDirections.MoveDown)
            {
                //Replace these lines with return;, to eliminate wrap around
                for (int i = selIdx; i >= 1; i--)
                {
                    SwapLvItems(i, i - 1, currentgroup.Items);
                }
            }
            else
                SwapLvItems(selIdx, selIdx + (int)move, currentgroup.Items);
            lv.Refresh();
            lv.Focus();
        }

        public static void AppendListViewItem(ref ListView listView, string[] @params, string key, object tag = null)
        {
            if (!listView.Items.ContainsKey(key))
            {
                ListViewItem lvItemFieldElement = new ListViewItem
                {
                    Name = key,
                    Text = @params.FirstOrDefault(),
                    Tag = tag
                };
                for (int i = 1; i < @params.Length; i++)
                {
                    lvItemFieldElement.SubItems.Add(@params[i]);
                }
                listView.Items.Add(lvItemFieldElement);
            }
        }

        public static string GetIndexBasedItemName(string name, int index) => $"{name} #" + index;

        public static void MakeIndexBasedItemNames(ref ListView listView, string name)
        {
            int currentIndex = 1;
            for (int i = 0; i < listView.Items.Count; i++)
            {
                ListViewItem item = listView.Items[i];
                item.Text = GetIndexBasedItemName(name, currentIndex++);
            }
        }
    }
}
