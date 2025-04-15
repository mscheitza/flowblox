using CommandLine;
using FlowBlox.Core.Authentication;
using FlowBlox.Core.CommandLine;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.Fonts;
using FlowBlox.UICore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            InstallFonts();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var splash = new SplashWindow())
            {
                splash.ShowDialog();
            }

            var options = ParseCommandLine(args);

            var flowBloxAutoLoginExecutor = new FlowBloxAutoLoginExecutor();
            Task.Run(async () => await flowBloxAutoLoginExecutor.TryAutoLoginAsync()).GetAwaiter().GetResult();

            InitProjectFile(options);
            Application.Run(AppWindow.AppWindow.Instance);
        }

        private static Options ParseCommandLine(string[] args)
        {
            var parserResult = new Parser().ParseArguments<Options>(args);
            Options parsedOptions = null;

            parserResult.WithParsed(opts => parsedOptions = opts)
                        .WithNotParsed(errs => parsedOptions = new Options());

            return parsedOptions;
        }

        private static void InitProjectFile(Options options)
        {
            if (!string.IsNullOrEmpty(options?.ProjectFile))
                AppWindow.AppWindow.Instance.SetProjectFile(options.ProjectFile);
        }


        private static void InstallFonts()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
            string fontsDirectory = Path.Combine(assemblyDir, "data", "fonts");

            if (!Directory.Exists(fontsDirectory))
                return;

            var fontFiles = Directory.GetFiles(fontsDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase));

            bool anyInstalled = false;
            foreach (var fontFile in fontFiles)
            {
                try
                {
                    if (FontInstaller.InstallFontForCurrentUser(fontFile))
                        anyInstalled = true;
                }
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error($"Failed to install font: '{fontFile}'", ex);
                }
            }

            if (anyInstalled)
            {
                Application.Restart();
            }
        }
    }
}
    