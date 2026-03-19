namespace FlowBlox.UICore.Models
{
    public class NotificationEventArgs
    {
        public NotificationEventArgs(string message) 
        {
            Message = message;
        }

        public string Message { get; }
    }
}
