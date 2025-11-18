namespace FlowBlox.Core.Logging
{
    public interface ILogger
    {
        void Error(string message, Exception e = null);
        void Exception(Exception e);
        void Info(string message);
        void Warn(string message, Exception e = null);
        string GetLogfilePath();
    }
}