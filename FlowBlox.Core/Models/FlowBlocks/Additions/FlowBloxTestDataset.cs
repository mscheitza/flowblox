using FlowBlox.Core.Models.FlowBlocks.Base;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    public class FlowBlockTestDataset : INotifyPropertyChanged
    {
        private BaseFlowBlock _flowBlock;
        private bool _execute;
        private List<FlowBloxTestConfiguration> _flowBloxTestConfigurations;

        public BaseFlowBlock FlowBlock
        {
            get => _flowBlock;
            set
            {
                if (_flowBlock != value)
                {
                    _flowBlock = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Execute
        {
            get => _execute;
            set
            {
                if (_execute != value)
                {
                    _execute = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<FlowBloxTestConfiguration> FlowBloxTestConfigurations
        {
            get => _flowBloxTestConfigurations;
            set
            {
                if (_flowBloxTestConfigurations != value)
                {
                    _flowBloxTestConfigurations = value;
                    OnPropertyChanged();
                }
            }
        }

        public FlowBlockTestDataset()
        {
            FlowBloxTestConfigurations = new List<FlowBloxTestConfiguration>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
