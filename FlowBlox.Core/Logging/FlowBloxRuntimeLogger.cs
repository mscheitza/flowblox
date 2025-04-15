using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Util;
using log4net;
using log4net.Config;
using log4net.Util;

namespace FlowBlox.Core.Logging
{
    public class FlowBloxRuntimeLogger : FlowBloxLoggerBase
    {
        private readonly string _runtimeLogFileName;
        private readonly string _runtimeLogDirectory;

        public FlowBloxRuntimeLogger(string runtimeLogFileName) : base("log4net.runtime.config")
        {
            if (string.IsNullOrEmpty(runtimeLogFileName))
                throw new ArgumentNullException(nameof(runtimeLogFileName));

            _runtimeLogFileName = runtimeLogFileName;
            _runtimeLogDirectory = FlowBloxOptions.GetOptionInstance().GetOption("General.RuntimeLogDir").Value;

            GlobalContext.Properties["runtimeLogFileName"] = _runtimeLogFileName;
            GlobalContext.Properties["runtimeLogDirectory"] = _runtimeLogDirectory;

            Configure();
        }

        public override string GetLogfilePath()
        {
            string logFilePath = base.GetLogfilePath();

            if (!string.IsNullOrEmpty(logFilePath))
            {
                logFilePath = logFilePath
                    .Replace("%property{runtimeLogFileName}", _runtimeLogFileName)
                    .Replace("%property{runtimeLogDirectory}", _runtimeLogDirectory);
            }

            return logFilePath;
        }
    }
}