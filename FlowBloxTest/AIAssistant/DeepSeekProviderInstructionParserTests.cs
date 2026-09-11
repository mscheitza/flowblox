using FlowBlox.AIAssistant.Services;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    public sealed class DeepSeekProviderInstructionParserTests
    {
        [TestMethod]
        public void Parse_MapsDsmlToolCallsToAssistantInstructionForDeepSeek()
        {
            var parser = new AiAssistantInstructionParser();
            var output =
                "<||DSML||tool_calls>" +
                "<||DSML||invoke name=\"FlowBloxAIToolApi-SearchFlowBlock\">" +
                "<||DSML||parameter name=\"searchForNames\" string=\"true\">Script Powershell Python Command Shell Execute Http Client Download Page Body Transform Rows</||DSML||parameter>" +
                "</||DSML||invoke>" +
                "<||DSML||invoke name=\"FlowBloxAIToolApi-GetTypeKindsInfo\">" +
                "<||DSML||parameter name=\"typeFullName\" string=\"true\">FlowBlox.Core.Models.FlowBlocks.Selection.RegexSelectorFlowBlock</||DSML||parameter>" +
                "</||DSML||invoke>" +
                "</||DSML||tool_calls>";

            var result = parser.Parse(output, new DeepSeekAIProvider());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Instruction.ToolCalls.Count);
            Assert.AreEqual("SearchFlowBlock", result.Instruction.ToolCalls[0].ToolName);
            Assert.AreEqual(
                "Script Powershell Python Command Shell Execute Http Client Download Page Body Transform Rows",
                result.Instruction.ToolCalls[0].Arguments.Value<string>("searchForNames"));
            Assert.AreEqual("GetTypeKindsInfo", result.Instruction.ToolCalls[1].ToolName);
            Assert.AreEqual(
                "FlowBlox.Core.Models.FlowBlocks.Selection.RegexSelectorFlowBlock",
                result.Instruction.ToolCalls[1].Arguments.Value<string>("typeFullName"));
            StringAssert.Contains(result.Instruction.InternalContent, "\"toolCalls\"");

            var normalizedResult = parser.Parse(result.Instruction.InternalContent);
            Assert.IsTrue(normalizedResult.Success);
            Assert.AreEqual("SearchFlowBlock", normalizedResult.Instruction.ToolCalls[0].ToolName);
        }

        [TestMethod]
        public void Parse_DoesNotUseDsmlProviderParserForOtherProviders()
        {
            var parser = new AiAssistantInstructionParser();
            const string output =
                "<||DSML||tool_calls>" +
                "<||DSML||invoke name=\"SearchFlowBlock\">" +
                "<||DSML||parameter name=\"searchForNames\" string=\"true\">Html Table Regex</||DSML||parameter>" +
                "</||DSML||invoke>" +
                "</||DSML||tool_calls>";

            var result = parser.Parse(output, new OpenAIProvider());

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void Parse_ToleratesEscapedToolCallsTagAndClosingTagsForDeepSeek()
        {
            var parser = new AiAssistantInstructionParser();
            const string output =
                "<||DSML||tool\\_calls>" +
                "<||DSML||invoke name=\"SearchFlowBlock\">" +
                "<||DSML||parameter name=\"searchForNames\" string=\"true\">Html Table Regex Text Replace Map Split\\</||DSML||parameter>" +
                "\\</||DSML||invoke>" +
                "<||DSML||invoke name=\"SearchFlowBlock\">" +
                "<||DSML||parameter name=\"searchForNames\" string=\"true\">Select Row Column Loop Iterator\\</||DSML||parameter>" +
                "\\</||DSML||invoke>" +
                "<||DSML||invoke name=\"GetTypeKindsInfo\">" +
                "<||DSML||parameter name=\"typeFullName\" string=\"true\">FlowBlox.Core.Models.FlowBlocks.Web.WebRequestFlowBlock\\</||DSML||parameter>" +
                "\\</||DSML||invoke>" +
                "<||DSML||invoke name=\"GetTypeKindsInfo\">" +
                "<||DSML||parameter name=\"typeFullName\" string=\"true\">FlowBlox.Core.Models.FlowBlocks.IO.TableWriterFlowBlock\\</||DSML||parameter>" +
                "\\</||DSML||invoke>" +
                "<||DSML||invoke name=\"GetTypeKindsInfo\">" +
                "<||DSML||parameter name=\"typeFullName\" string=\"true\">FlowBlox.Core.Models.FlowBlocks.IO.FileWriterFlowBlock\\</||DSML||parameter>" +
                "\\</||DSML||invoke>" +
                "\\</||DSML||tool\\_calls>";

            var result = parser.Parse(output, new DeepSeekAIProvider());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, result.Instruction.ToolCalls.Count);
            Assert.AreEqual("SearchFlowBlock", result.Instruction.ToolCalls[0].ToolName);
            Assert.AreEqual("Html Table Regex Text Replace Map Split", result.Instruction.ToolCalls[0].Arguments.Value<string>("searchForNames"));
            Assert.AreEqual("SearchFlowBlock", result.Instruction.ToolCalls[1].ToolName);
            Assert.AreEqual("Select Row Column Loop Iterator", result.Instruction.ToolCalls[1].Arguments.Value<string>("searchForNames"));
            Assert.AreEqual("GetTypeKindsInfo", result.Instruction.ToolCalls[2].ToolName);
            Assert.AreEqual("FlowBlox.Core.Models.FlowBlocks.Web.WebRequestFlowBlock", result.Instruction.ToolCalls[2].Arguments.Value<string>("typeFullName"));
            Assert.AreEqual("FlowBlox.Core.Models.FlowBlocks.IO.TableWriterFlowBlock", result.Instruction.ToolCalls[3].Arguments.Value<string>("typeFullName"));
            Assert.AreEqual("FlowBlox.Core.Models.FlowBlocks.IO.FileWriterFlowBlock", result.Instruction.ToolCalls[4].Arguments.Value<string>("typeFullName"));
        }

        [TestMethod]
        public void Parse_TreatsDeepSeekDsmlAsToolCallsBeforeEmbeddedJson()
        {
            var parser = new AiAssistantInstructionParser();
            const string marker = "\uFF5C\uFF5CDSML\uFF5C\uFF5C";
            var output =
                $"<{marker}tool_calls>" +
                $"<{marker}invoke name=\"FlowBloxAIToolApi-UpdateManagedObject\">" +
                $"<{marker}parameter name=\"name\" string=\"true\">FileObject_StaedteXlsx\\</{marker}parameter>" +
                $"<{marker}parameter name=\"path\" string=\"true\">/FilePath\\</{marker}parameter>" +
                $"<{marker}parameter name=\"value\" string=\"true\">{{\"resolveManagedObjectByName\":\"FileObjectStaedteXlsx\"}}\\</{marker}parameter>" +
                $"\\</{marker}invoke>" +
                $"<{marker}invoke name=\"FlowBloxAIToolApi-UpdateFlowBlock\">" +
                $"<{marker}parameter name=\"name\" string=\"true\">TableWriter_Staedte\\</{marker}parameter>" +
                $"<{marker}parameter name=\"updates\" string=\"false\">[{{\"path\":\"/TableColumnDefinitions/0:FlowBlox.Core.Models.FlowBlocks.IO.TableColumnDefinition/Field/Link\",\"value\":{{\"resolveFieldElementByFQName\":\"$RegexSelector_Stadt::Stadt\"}}}}]\\</{marker}parameter>" +
                $"\\</{marker}invoke>" +
                $"\\</{marker}tool_calls>";

            var result = parser.Parse(output, new DeepSeekAIProvider());

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.Instruction.Final);
            Assert.AreEqual(2, result.Instruction.ToolCalls.Count);
            Assert.AreEqual("UpdateManagedObject", result.Instruction.ToolCalls[0].ToolName);
            Assert.AreEqual("FileObject_StaedteXlsx", result.Instruction.ToolCalls[0].Arguments.Value<string>("name"));
            Assert.AreEqual("{\"resolveManagedObjectByName\":\"FileObjectStaedteXlsx\"}", result.Instruction.ToolCalls[0].Arguments.Value<string>("value"));
            Assert.AreEqual("UpdateFlowBlock", result.Instruction.ToolCalls[1].ToolName);
            Assert.IsInstanceOfType(result.Instruction.ToolCalls[1].Arguments["updates"], typeof(JArray));
            Assert.AreEqual(
                "$RegexSelector_Stadt::Stadt",
                result.Instruction.ToolCalls[1].Arguments["updates"]?[0]?["value"]?.Value<string>("resolveFieldElementByFQName"));
        }
    }
}