namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FlowBloxUIGroupAttribute : Attribute
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public ControlAlignment ControlAlignment { get; set; }

        public FlowBloxUIGroupAttribute()
        {

        }

        public FlowBloxUIGroupAttribute(string name, int order, ControlAlignment controlAlignment = ControlAlignment.Fill)
        {
            Name = name;
            Order = order;
            ControlAlignment = controlAlignment;
        }
    }

    public enum ControlAlignment
    {
        Fill,
        Top
    }
}
