using FlowBlox.Core.Util.ShellExecution;

namespace FlowBloxTest.Util.ShellExecution
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBloxShellExecutorTests
    {
        [TestMethod]
        public void Execute_PreservesQuotedExecutableAndArguments()
        {
            var command = OperatingSystem.IsWindows()
                ? $"\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "where.exe")}\" \"cmd.exe\""
                : "\"/usr/bin/printf\" \"value with spaces\"";

            var result = FlowBloxShellExecutor.Execute(new FlowBloxShellExecutionRequest
            {
                Command = command,
                TimeoutMilliseconds = 10_000
            });

            Assert.IsTrue(result.Success, result.ExceptionMessage + result.StandardError);
            StringAssert.Contains(
                result.StandardOutput,
                OperatingSystem.IsWindows() ? "cmd.exe" : "value with spaces");
        }
    }
}
