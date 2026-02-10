using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Windows.Forms;

namespace FlowBlox.Grid.Views.Main
{
    public interface IFlowBlockCustomTabPageProvider
    {
        bool ReadOnly { get; set; }

        TabControl TabControl { get; }

        void UpdateUI();

        bool Apply();
    }

    public interface IFlowBlockCustomTabPageProvider<in T> : IFlowBlockCustomTabPageProvider where T : BaseFlowBlock
    {
        void Initialize(T gridElement);
    }
}
