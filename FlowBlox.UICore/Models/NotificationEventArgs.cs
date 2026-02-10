namespace FlowBlox.UICore.Models
{
    public class NotificationEventArgs
    {
        public NotificationEventArgs(string message) 
        {
            this.Message = message;
        }

        public string Message { get; }
    }
}
