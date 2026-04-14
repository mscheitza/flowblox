namespace FlowBlox.Core.Attributes
{
    namespace FlowBlox.Core.Attributes
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class FlowBloxUIFileSelectionAttribute : Attribute
        {
            public string Filter { get; set; }

            public FlowBloxUIFileSelectionAttribute(string filter)
            {
                Filter = filter;
            }
        }
    }
}
