namespace FlowBlox.UICore.Events
{
    public class AssociationBeforeLinkEventArgs : EventArgs
    {
        public AssociationBeforeLinkEventArgs(string propertyName, object originalLinkedObject)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            OriginalLinkedObject = originalLinkedObject ?? throw new ArgumentNullException(nameof(originalLinkedObject));
            LinkedObject = originalLinkedObject;
        }

        public string PropertyName { get; }

        public object OriginalLinkedObject { get; }

        public object LinkedObject { get; set; }

        public bool Cancel { get; set; }
    }
}