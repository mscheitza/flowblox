using FlowBlox.UICore.Factory.PropertyView;
using System.Windows;

namespace FlowBlox.UICore.Resolver
{
    public class PropertyControlWithFactoryResult
    {
        public PropertyControlWithFactoryResult(FrameworkElement frameworkElement)
        {
            FrameworkElement = frameworkElement;
        }

        public PropertyControlWithFactoryResult(FrameworkElement frameworkElement, WpfPropertyViewControlFactory propertyViewControlFactory) : this(frameworkElement)
        {
            PropertyViewControlFactory = propertyViewControlFactory;
        }

        public FrameworkElement FrameworkElement { get; set; }

        public WpfPropertyViewControlFactory PropertyViewControlFactory { get; set; }
    }
}
