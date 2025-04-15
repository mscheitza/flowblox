namespace FlowBlox.Grid
{
    public class GridElementProperty
    {
        public enum PropertyTypes { Text, Password }

        public string PropertyName { get; private set; }
        public string PropertyValue { get; private set; }
        public string PropertyDescription { get; private set; }
        public PropertyTypes PropertyType { get; private set; }

        public GridElementProperty(string propertyName, string propertyValue, string propertyDescription, PropertyTypes propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
            this.PropertyDescription = propertyDescription;
            this.PropertyValue = propertyValue;
        }
    }
}
