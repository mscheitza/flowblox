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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}