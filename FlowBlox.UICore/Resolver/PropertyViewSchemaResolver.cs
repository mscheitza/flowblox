using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.ViewModels.PropertyView;

namespace FlowBlox.UICore.Resolver
{
    public class PropertyViewSchemaResolver
    {
        private readonly PropertyControlResolver _controlResolver;
        private Window _window;

        public PropertyViewSchemaResolver(Window window, object parent = null)
        {
            this._window = window;
            this._controlResolver = new PropertyControlResolver(window, parent);
        }

        public IEnumerable<TabViewModel> ResolveTabs(object target, bool readOnly, object preselectedInstance)
        {
            var bindingContext = new PropertyControlBindingContext(target);

            var groupAttributes = target.GetType()
                .GetCustomAttributes<FlowBloxUIGroupAttribute>(true)
                .OrderBy(x => x.Order)
                .ToList();

            var groupedProperties = target.GetType().GetProperties()
                .Where(property => property.GetCustomAttribute<DisplayAttribute>() != null)
                .GroupBy(property => GetGroupName(property))
                .OrderBy(propertyGroup =>
                {
                    var groupAttribute = groupAttributes.FirstOrDefault(x => x.Name == propertyGroup.Key);
                    return groupAttribute == null ? 0 : groupAttributes.IndexOf(groupAttribute);
                });

            foreach (var propertyGroup in groupedProperties)
            {
                var tabViewModel = new TabViewModel
                {
                    TabTitle = FlowBloxResourceUtil.GetLocalizedString(propertyGroup.Key),
                };

                foreach (var property in propertyGroup.OrderBy(p => p.GetCustomAttribute<DisplayAttribute>().GetOrder() ?? 0))
                {
                    var controlViewModel = _controlResolver.Resolve(property, target, readOnly, preselectedInstance, bindingContext);
                    if (controlViewModel != null)
                        tabViewModel.Controls.Add(controlViewModel);
                }
                yield return tabViewModel;
            }
        }

        private string GetGroupName(PropertyInfo property)
        {
            string groupName = property.GetCustomAttribute<DisplayAttribute>()?.GroupName;
            return string.IsNullOrEmpty(groupName) ? "Global_Groups_Default" : groupName;
        }
    }
}
