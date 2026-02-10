namespace FlowBlox.Core.Logging
{
    public class FlowBloxLogManager
    {
        private static readonly Lazy<FlowBloxLogManager> _instance = new Lazy<FlowBloxLogManager>(() => new FlowBloxLogManager());

        private static string _applicationId;
        private static ILogger _logger;

        private FlowBloxLogManager()
        {
        }

        public static FlowBloxLogManager Instance => _instance.Value;

        private const string ApplicationLogFileSuffix = "application";

        public string ApplicationLogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationId))
                {
                    Random random = new Random();
                    int randomNumber = random.Next(0x1000, 0x10000);
                    var applicationId = string.Concat("A", randomNumber.ToString("X4"));
                    _applicationId = string.Join("_",
                        DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                        applicationId,
                        ApplicationLogFileSuffix);
                }
                return _applicationId;
            }
        }

        public ILogger GetLogger()
        {
            if (_logger == null)
                _logger = new FlowBloxApplicationLogger(ApplicationLogFileName);

            return _logger;
        }
    }
}
