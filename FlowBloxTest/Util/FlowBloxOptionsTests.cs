using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Fields;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.Util
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBloxOptionsTests
    {
        private const string RootOption = "Tests.Options.Root";
        private const string ChildOption = "Tests.Options.Child";
        private const string NestedOption = "Tests.Options.Nested";
        private const string SelfOption = "Tests.Options.Self";
        private const string CycleAOption = "Tests.Options.CycleA";
        private const string CycleBOption = "Tests.Options.CycleB";

        [TestCleanup]
        public void Cleanup()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            foreach (var key in new[] { RootOption, ChildOption, NestedOption, SelfOption, CycleAOption, CycleBOption })
                options.OptionCollection.Remove(key);
        }

        [TestMethod]
        public void ValueResolvesNestedOptionsAndEnvironmentVariables()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            AddOption(options, RootOption, @"%TEMP%\FlowBloxOptionsTest");
            AddOption(options, ChildOption, $@"$Options::{RootOption}\child");
            AddOption(options, NestedOption, $@"$Options::{ChildOption}\nested");

            Assert.AreEqual(@"%TEMP%\FlowBloxOptionsTest", options.GetOption(RootOption).PersistentValue);
            var expected = Path.Combine(
                Environment.ExpandEnvironmentVariables("%TEMP%"),
                "FlowBloxOptionsTest",
                "child",
                "nested");
            Assert.AreEqual(expected, options.GetOption(NestedOption).Value);
            Assert.AreEqual(expected, FlowBloxFieldHelper.ReplaceFieldsInString($"$Options::{NestedOption}"));
            CollectionAssert.Contains(options.OptionCollection.Keys.ToList(), NestedOption);
        }

        [TestMethod]
        public void ValueLeavesSelfAndCyclicReferencesUnresolved()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            AddOption(options, SelfOption, $@"$Options::{SelfOption}\self");
            AddOption(options, CycleAOption, $@"$Options::{CycleBOption}\a");
            AddOption(options, CycleBOption, $@"$Options::{CycleAOption}\b");

            Assert.AreEqual($@"$Options::{SelfOption}\self", options.GetOption(SelfOption).Value);
            StringAssert.Contains(options.GetOption(CycleAOption).Value, $"$Options::{CycleAOption}");
        }

        [TestMethod]
        public void PersistentValueIsSerializedUsingValuePropertyName()
        {
            var option = new OptionElement(
                RootOption,
                @"%TEMP%\FlowBloxOptionsTest",
                "Option serialization test.",
                OptionElement.OptionType.Text,
                isPlaceholderEnabled: true);

            var json = JObject.Parse(JsonConvert.SerializeObject(option));
            Assert.AreEqual(option.PersistentValue, json.Value<string>("Value"));
            Assert.IsNull(json["PersistentValue"]);

            var deserialized = JsonConvert.DeserializeObject<OptionElement>(json.ToString());
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(option.PersistentValue, deserialized!.PersistentValue);
        }

        private static void AddOption(FlowBloxOptions options, string name, string value)
        {
            options.OptionCollection[name] = new OptionElement(
                name,
                value,
                "Option placeholder resolution test.",
                OptionElement.OptionType.Text,
                isPlaceholderEnabled: true);
        }
    }
}
