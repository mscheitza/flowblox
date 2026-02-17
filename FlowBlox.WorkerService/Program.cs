using FlowBlox.WorkerService;
using FlowBlox.WorkerService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace FlowBlox.WorkerService
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // allow overriding config file path via --config <path>
            var configPath = CliArgs.TryGetValue(args, "--config");
            var isConsole = args.Any(a => string.Equals(a, "--console", StringComparison.OrdinalIgnoreCase));

            var builder = Host.CreateApplicationBuilder();

            // Configuration: default appsettings.json + optional override path
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (!string.IsNullOrWhiteSpace(configPath))
            {
                builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: true);
            }

            // Logging: EventLog works best for Windows Service.
            builder.Logging.ClearProviders();
            builder.Logging.AddEventLog(new EventLogSettings
            {
                SourceName = "FlowBlox.Service"
            });
            builder.Logging.AddConsole(); // useful for --console

            // Windows Service integration (only when not in console mode)
            if (!isConsole)
            {
                builder.Services.AddWindowsService(options =>
                {
                    options.ServiceName = builder.Configuration["FlowBloxService:ServiceName"] ?? "FlowBlox.Service";
                });
            }

            builder.Services.Configure<FlowBloxServiceOptions>(builder.Configuration.GetSection("FlowBloxService"));

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();

            await host.RunAsync();
            return 0;
        }
    }

    internal static class CliArgs
    {
        public static string? TryGetValue(string[] args, string key)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }
    }
}
