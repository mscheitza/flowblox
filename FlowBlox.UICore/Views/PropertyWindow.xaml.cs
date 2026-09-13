using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.Base;
using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.ViewModels.PropertyView;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows.Threading;

namespace FlowBlox.UICore.Views
{
    public class PropertyWindowArgs
    {
        public object Target { get; set; }
        public object Parent { get; set; }
        public bool ReadOnly { get; set; }
        public bool DeepCopy { get; set; }
        public bool CanSave { get; set; }
        public bool Detached { get; set; }
        public bool IsNew { get; set; }

        public string PreselectedProperty { get; set; }
        public FlowBloxReactiveObject PreselectedInstance { get; set; }

        public PropertyWindowArgs()
        {
            ReadOnly = false;
            DeepCopy = true;
            CanSave = true;
            Detached = false;
            IsNew = false;
        }

        public PropertyWindowArgs(
            object target,
            object parent = null,
            bool readOnly = false,
            bool deepCopy = true,
            bool canSave = true,
            string preselectedProperty = null,
            FlowBloxReactiveObject preselectedInstance = null,
            bool detached = false,
            bool isNew = false)
        {
            Target = target;
            Parent = parent;
            ReadOnly = readOnly;
            DeepCopy = deepCopy;
            CanSave = canSave;
            PreselectedProperty = preselectedProperty;
            PreselectedInstance = preselectedInstance;
            Detached = detached;
            IsNew = isNew;
        }
    }

    public partial class PropertyWindow : MetroWindow
    {
        private PropertyWindowArgs _propertyWindowArgs;
        private bool _quickStartPopUpRequested;

        public PropertyWindow()
        {
            InitializeComponent();
        }

        public PropertyWindow(PropertyWindowArgs propertyWindowArgs) : this()
        {
            _propertyWindowArgs = propertyWindowArgs;
            DataContext = new PropertyWindowViewModel(this, propertyWindowArgs);
            Closing += PropertyView_Closing;
            Loaded += PropertyWindow_Loaded;
        }

        private void PropertyWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_quickStartPopUpRequested)
                return;

            _quickStartPopUpRequested = true;
            Dispatcher.BeginInvoke(ShowQuickStartPopUpIfAvailable, DispatcherPriority.ApplicationIdle);
        }

        private void ShowQuickStartPopUpIfAvailable()
        {
            if (_propertyWindowArgs?.Target == null)
                return;

            var quickStartPopUpService = FlowBloxServiceLocator.Instance.GetService<IQuickStartPopUpService>();
            quickStartPopUpService?.ShowFor(_propertyWindowArgs.Target, this);
        }

        private void PropertyView_Closing(object sender, CancelEventArgs e)
        {
            if (DialogResult != true && DataContext is PropertyWindowViewModel viewModel)
            {
                viewModel.Rollback();
            }
        }
    }
}
