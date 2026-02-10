using System.Reflection;

namespace FlowBlox.Core.Extensions
{
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// Tries to get the value of a property without throwing an exception if access fails.
        /// </summary>
        /// <param name="property">The PropertyInfo object.</param>
        /// <param name="obj">The object instance containing the property.</param>
        /// <param name="value">The output value, or null if retrieval failed.</param>
        /// <returns>True if the value was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetValue(this PropertyInfo property, object obj, out object value)
        {
            try
            {
                value = property.GetValue(obj);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
