using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FlowBlox.UICore.ViewModels.PropertyView
{
    public class TabViewModel : INotifyPropertyChanged
    {
        public TabViewModel()
        {
            Controls = new ObservableCollection<PropertyControlViewModel>();
        }

        public string TabTitle { get; set; }

        public ObservableCollection<PropertyControlViewModel> Controls { get; set; }

        public bool HasMaximizedControl => Controls.Any(x => x.Maximize);

        public IEnumerable<PropertyControlViewModel> MaximizedControls => Controls.Where(x => x.Maximize);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
