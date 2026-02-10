namespace FlowBlox.UICore.Utilities
{
    public class DisplayItem
    {
        public object Value { get; set; }

        public string DisplayName { get; set; }
    }

    public class GenericSelectionHandler
    {
        public List<DisplayItem> Items { get; protected set; }
    }

    public class GenericSelectionHandler<T> : GenericSelectionHandler
    {
        public GenericSelectionHandler(IEnumerable<T> items, Func<T, string> displayNameSelector)
        {
            this.Items = items.Select(item => new DisplayItem { Value = item, DisplayName = displayNameSelector(item) }).ToList();
        }

        public T GetObjectForDisplayItem(DisplayItem displayItem) => (T)displayItem?.Value;
    }
}
