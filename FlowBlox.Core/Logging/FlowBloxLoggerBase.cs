using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;
using System;
using System.IO;
using System.Reflection;

namespace FlowBlox.Core.Logging
{
    public abstract class FlowBloxLoggerBase : ILogger
    {
        protected ILog logger;
        private string _configFileName;

        protected FlowBloxLoggerBase(string configFileName)
        {
            _configFileName = configFileName;
        }

        protected virtual void Configure()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configFileName);
            XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));
            logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Warn(string message, Exception e = null)
        {
            if (e != null)
                logger.Warn(message, e);
            else
                logger.Warn(message);
        }

        public void Error(string message, Exception e = null)
        {
            if (e != null)
                logger.Error(message, e);
            else
                logger.Error(message);
        }

        public void Exception(Exception e)
        {
            logger.Error("An exception occurred.", e);
        }

        public virtual string GetLogfilePath()
        {
            try
            {
                ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

                var appender = logRepository.GetAppenders()
                    .OfType<RollingFileAppender>()
                    .FirstOrDefault();

                if (appender != null)
                {
                    string logFilePath = appender.File;
                    logFilePath = Environment.ExpandEnvironmentVariables(logFilePath);
                    return logFilePath;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get log file path", ex);
            }

            return null;
        }
    }
}