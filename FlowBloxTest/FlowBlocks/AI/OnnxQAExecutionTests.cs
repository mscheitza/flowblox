using FlowBlox.Core.Models.FlowBlocks.AI;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.ShellExecution;
using FlowBlox.Test.Runtime;
using System.Diagnostics;

namespace FlowBloxTest.FlowBlocks.AI
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.IntegrationTests)]
    public class OnnxQAExecutionTests : FlowBloxTestsBase
    {
        private const string PublishedExportScriptName = "export_onnx_qa_model.py";
        private static readonly string ModelRoot = Path.Combine(
            Path.GetTempPath(), "FlowBlox", "Onnx", "QA");

        private FlowBloxProject _project = null!;

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void TestInitialisieren()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void MDebertaV3MultilingualSquad2_BeantwortetFrageNachPersonenname()
        {
            ModellAusführenUndAntwortPrüfen(
                modelId: "timpal0l/mdeberta-v3-base-squad2",
                folderName: "mdeberta-v3-base-squad2",
                question: "What is the employee's full name?",
                context: "The employee's full name is Ada Lovelace. She works in the research department.",
                expectedAnswer: "Ada Lovelace",
                sentencePieceSource: "microsoft/mdeberta-v3-base");
        }

        [TestMethod]
        public void DebertaV3BaseSquad2_BeantwortetFrageNachPersonenname()
        {
            ModellAusführenUndAntwortPrüfen(
                modelId: "deepset/deberta-v3-base-squad2",
                folderName: "deberta-v3-base-squad2",
                question: "What is the employee's full name?",
                context: "The employee's full name is Ada Lovelace. She works in the research department.",
                expectedAnswer: "Ada Lovelace");
        }

        [TestMethod]
        public void XlmRobertaBaseSquad2_LiestArtikelnummerAusStrukturiertemText()
        {
            ModellAusführenUndAntwortPrüfen(
                modelId: "deepset/xlm-roberta-base-squad2",
                folderName: "xlm-roberta-base-squad2",
                question: "What is the article number?",
                context: "Invoice: { Article number: AX-2048, Quantity: 3, Total amount: EUR 149.90 }",
                expectedAnswer: "AX-2048");
        }

        [TestMethod]
        public void GelectraGermanquad_LiestRechnungsbetragAusText()
        {
            ModellAusführenUndAntwortPrüfen(
                modelId: "deepset/gelectra-base-germanquad",
                folderName: "gelectra-base-germanquad",
                question: "What is the total amount?",
                context: "The invoice for article AX-2048 has a total amount of EUR 149.90.",
                expectedAnswer: "EUR 149.90");
        }

        private void ModellAusführenUndAntwortPrüfen(
            string modelId,
            string folderName,
            string question,
            string context,
            string expectedAnswer,
            string? sentencePieceSource = null)
        {
            var modelFolder = Path.Combine(ModelRoot, folderName);
            ModellBeiBedarfExportieren(modelId, modelFolder, sentencePieceSource);

            var start = CreateFlowBlock<StartFlowBlock>();
            var qa = CreateFlowBlock<OnnxQAFlowBlock>(start);
            qa.ModelFolder = modelFolder;
            qa.Question = question;
            qa.Context = context;
            qa.AllowNoAnswer = false;

            var runtime = new FlowBloxUnitTestRuntime(_project);
            runtime.Execute();

            var actual = qa.GridElementResult.Results
                .SelectMany(result => result.FieldValueMappings)
                .Select(mapping => mapping.Value)
                .Single();
            Assert.AreEqual(expectedAnswer, actual);
        }

        private void ModellBeiBedarfExportieren(
            string modelId,
            string modelFolder,
            string? sentencePieceSource)
        {
            if (IstVollständigerModellordner(modelFolder))
            {
                TestContext.WriteLine($"Vorhandenes Modell wird verwendet: {modelFolder}");
                return;
            }

            var python = PythonSuchenOderTestFehlschlagenLassen();
            var scriptPath = VeröffentlichtesExportskriptSuchen();
            Directory.CreateDirectory(modelFolder);

            var arguments = new List<string>();
            arguments.AddRange(python.PrefixArguments);
            arguments.Add(scriptPath);
            arguments.Add("--model-id");
            arguments.Add(modelId);
            arguments.Add("--output-directory");
            arguments.Add(modelFolder);
            if (!string.IsNullOrWhiteSpace(sentencePieceSource))
            {
                arguments.Add("--sentencepiece-source");
                arguments.Add(sentencePieceSource);
            }

            var command = string.Join(" ", new[] { python.Executable }
                .Concat(arguments)
                .Select(BefehlsargumentMaskieren));
            TestContext.WriteLine($"Starte veröffentlichtes QA-Exportskript:{Environment.NewLine}{command}");

            var result = FlowBloxShellExecutor.Execute(new FlowBloxShellExecutionRequest
            {
                Command = command,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)!,
                TimeoutMilliseconds = (int)TimeSpan.FromHours(1).TotalMilliseconds
            });

            SkriptergebnisProtokollieren(result);
            if (!result.Success)
                Assert.Fail(SkriptFehlermeldungErzeugen(modelId, result));

            Assert.IsTrue(
                IstVollständigerModellordner(modelFolder),
                $"Das Exportskript wurde erfolgreich beendet, aber '{modelFolder}' enthält kein vollständiges ONNX-QA-Modell mit Tokenizer.");
        }

        private void SkriptergebnisProtokollieren(FlowBloxShellExecutionResult result)
        {
            TestContext.WriteLine($"ExitCode: {result.ExitCode}");
            TestContext.WriteLine($"Zeitüberschreitung: {result.TimedOut}");
            TestContext.WriteLine($"Standardausgabe:{Environment.NewLine}{result.StandardOutput}");
            TestContext.WriteLine($"Fehlerausgabe:{Environment.NewLine}{result.StandardError}");
            if (!string.IsNullOrWhiteSpace(result.ExceptionMessage))
                TestContext.WriteLine($"Prozessfehler: {result.ExceptionMessage}");
        }

        private static string SkriptFehlermeldungErzeugen(
            string modelId,
            FlowBloxShellExecutionResult result)
        {
            return $"ONNX-Export für '{modelId}' fehlgeschlagen.{Environment.NewLine}" +
                   $"ExitCode: {result.ExitCode}{Environment.NewLine}" +
                   $"Zeitüberschreitung: {result.TimedOut}{Environment.NewLine}" +
                   $"Prozessfehler: {result.ExceptionMessage}{Environment.NewLine}" +
                   $"Standardausgabe:{Environment.NewLine}{result.StandardOutput}{Environment.NewLine}" +
                   $"Fehlerausgabe:{Environment.NewLine}{result.StandardError}";
        }

        private static bool IstVollständigerModellordner(string modelFolder)
        {
            if (!File.Exists(Path.Combine(modelFolder, "model.onnx")) ||
                !File.Exists(Path.Combine(modelFolder, "config.json")) ||
                !File.Exists(Path.Combine(modelFolder, "tokenizer_config.json")))
                return false;

            var hasSingleFileTokenizer = new[]
            {
                "vocab.txt",
                "spm.model",
                "sentencepiece.bpe.model",
                "tokenizer.model"
            }.Any(name => File.Exists(Path.Combine(modelFolder, name)));
            var hasByteLevelBpe =
                File.Exists(Path.Combine(modelFolder, "vocab.json")) &&
                File.Exists(Path.Combine(modelFolder, "merges.txt"));
            return hasSingleFileTokenizer || hasByteLevelBpe;
        }

        private static (string Executable, string[] PrefixArguments) PythonSuchenOderTestFehlschlagenLassen()
        {
            foreach (var candidate in new[]
                     {
                         (Executable: "python", PrefixArguments: Array.Empty<string>()),
                         (Executable: "py", PrefixArguments: new[] { "-3" })
                     })
            {
                try
                {
                    var info = new ProcessStartInfo
                    {
                        FileName = candidate.Executable,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    foreach (var argument in candidate.PrefixArguments)
                        info.ArgumentList.Add(argument);
                    info.ArgumentList.Add("--version");

                    using var process = Process.Start(info);
                    if (process != null && process.WaitForExit(15_000) && process.ExitCode == 0)
                        return candidate;
                }
                catch
                {
                    // Den nächsten üblichen Python-Launcher versuchen.
                }
            }

            Assert.Fail(
                "Für die ONNX-QA-Integrationstests wird Python 3 benötigt. " +
                "Python von https://www.python.org/downloads/ installieren, 'Add Python to PATH' aktivieren und den Test erneut ausführen.");
            throw new InvalidOperationException("Assert.Fail hat den Test nicht beendet.");
        }

        private static string VeröffentlichtesExportskriptSuchen()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "FlowBlox",
                    "ApplicationDir",
                    "data",
                    "python",
                    PublishedExportScriptName);
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new AssertFailedException(
                $"Das veröffentlichte QA-Exportskript '{PublishedExportScriptName}' wurde ausgehend von " +
                $"'{AppContext.BaseDirectory}' nicht unter FlowBlox/ApplicationDir/data/python gefunden.");
        }

        private static string BefehlsargumentMaskieren(string value) =>
            $"\"{value.Replace("\"", "\\\"")}\"";
    }
}
