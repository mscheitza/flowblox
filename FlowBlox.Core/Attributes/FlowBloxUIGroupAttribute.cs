namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FlowBloxUIGroupAttribute : Attribute
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public ControlAlignment ControlAlignment { get; set; }

        public bool Hide { get; set; }

        public FlowBloxUIGroupAttribute()
        {

        }

        public FlowBloxUIGroupAttribute(string name, bool hide = false)
        {
            Name = name;
            Hide = hide;
        }

        public FlowBloxUIGroupAttribute(string name, int order, ControlAlignment controlAlignment = ControlAlignment.Fill, bool hide = false)
        {
            Name = name;
            Order = order;
            ControlAlignment = controlAlignment;
            Hide = hide;
        }
    }

    public enum ControlAlignment
    {
        Fill,
        Top
    }
}
