using FlowBlox.Core.Constants;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.Grid.Elements.Util
{
    public class FlowBloxComponentHelper
    {
        public static void RaisePropertyChanged(object component, string propertyName)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name must not be empty.", nameof(propertyName));

            if (component is not FlowBloxReactiveObject reactiveItem)
                return;

            var method = component.GetType().GetMethod(
                "OnPropertyChanged",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            method?.Invoke(component, new object[] { propertyName });
        }


        public static string GetDisplayName(object inst)
        {
            if (inst == null)
                throw new ArgumentNullException(nameof(inst));

            var type = inst.GetType();

            // Versuche zuerst, den Titel aus der Display-Attribut zu holen
            string displayName = null;
            var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
                displayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, false);

            // Wenn kein Titel durch das Display-Attribut gefunden wurde, suche nach einer GetDisplayName Methode
            if (string.IsNullOrEmpty(displayName))
            {
                var displayNameProperty = type.GetProperty(GlobalConstants.PropertyNameDisplayName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (displayNameProperty != null && displayNameProperty.PropertyType == typeof(string))
                {
                    displayName = (string)displayNameProperty.GetValue(inst);
                }
            }

            // Wenn keine passende Methode oder Display-Attribut gefunden wurde, verwende den Typnamen
            if (string.IsNullOrEmpty(displayName))
                displayName = type.Name;

            return displayName;
        }

        public static string GetDescription(object inst)
        {
            if (inst == null)
                throw new ArgumentNullException(nameof(inst));

            var type = inst.GetType();
            var displayAttribute = type.GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null)
                return FlowBloxResourceUtil.GetDescription(displayAttribute);

            return string.Empty;
        }

        public static SKImage GetIcon32(object inst)
        {
            if (inst is FlowBloxComponent component)
                return component.Icon32;

            if (inst is FlowBloxReactiveObject reactiveObject)
                return reactiveObject.Icon32;

            return null;
        }

        public static SKImage GetIcon16(object inst)
        {
            if (inst is FlowBloxComponent component)
                return component.Icon16;

            if (inst is FlowBloxReactiveObject reactiveObject)
                return reactiveObject.Icon16;

            return null;
        }
    }
}
