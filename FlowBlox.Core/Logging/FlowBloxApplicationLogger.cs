using log4net;

namespace FlowBlox.Core.Logging
{
    public class FlowBloxApplicationLogger : FlowBloxLoggerBase
    {
        private string _applicationId;

        public FlowBloxApplicationLogger(string applicationLogFileName) : base("log4net.application.config")
        {
            if (string.IsNullOrEmpty(applicationLogFileName))
                throw new ArgumentNullException(nameof(applicationLogFileName));

            _applicationId = applicationLogFileName;

            GlobalContext.Properties["applicationLogFileName"] = _applicationId;

            Configure();
        }

        public override string GetLogfilePath()
        {
            string logFilePath = base.GetLogfilePath();

            if (!string.IsNullOrEmpty(logFilePath))
                logFilePath = logFilePath.Replace("%property{applicationId}", _applicationId);

            return logFilePath;
        }
    }
}