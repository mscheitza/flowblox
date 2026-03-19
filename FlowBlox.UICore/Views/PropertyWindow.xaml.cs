using FlowBlox.UICore.ViewModels.PropertyView;
using MahApps.Metro.Controls;
using System.ComponentModel;
using FlowBlox.Core.Models.Base;

namespace FlowBlox.UICore.Views
{
    public class PropertyWindowArgs
    {
        public object Target { get; set; }
        public bool ReadOnly { get; set; }
        public bool DeepCopy { get; set; }
        public bool CanSave { get; set; }
        public bool Detached { get; set; }

        public string PreselectedProperty { get; set; }
        public FlowBloxReactiveObject PreselectedInstance { get; set; }

        public PropertyWindowArgs()
        {
            ReadOnly = false;
            DeepCopy = true;
            CanSave = true;
            Detached = false;
        }

        public PropertyWindowArgs(
            object target, 
            bool readOnly = false, 
            bool deepCopy = true, 
            bool canSave = true, 
            string preselectedProperty = null, 
            FlowBloxReactiveObject preselectedInstance = null,
            bool detached = false)
        {
            Target = target;
            ReadOnly = readOnly;
            DeepCopy = deepCopy;
            CanSave = canSave;
            PreselectedProperty = preselectedProperty;
            PreselectedInstance = preselectedInstance;
            Detached = detached;
        }
    }

    public partial class PropertyWindow : MetroWindow
    {
        public PropertyWindow()
        {
            InitializeComponent();
        }

        public PropertyWindow(PropertyWindowArgs propertyWindowArgs) : this()
        {
            this.DataContext = new PropertyWindowViewModel(this, propertyWindowArgs);
            this.Closing += PropertyView_Closing;
        }

        private void PropertyView_Closing(object sender, CancelEventArgs e)
        {
            if (DialogResult != true && this.DataContext is PropertyWindowViewModel viewModel)
            {
                viewModel.Rollback();
            }
        }
    }
}
