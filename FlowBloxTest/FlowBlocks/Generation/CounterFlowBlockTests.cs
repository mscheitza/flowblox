using FlowBlox.Core.Models.FlowBlocks.Generation;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;

namespace FlowBloxTest.FlowBlocks.Generation
{
    [TestClass]
    public class CounterFlowBlockTests : FlowBloxTestsBase
    {
        [TestInitialize]
        public void Initialize()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
        }

        [TestMethod]
        public void OnAfterCreate_UsesConfiguredAlphanumericRange()
        {
            var expected = FlowBloxOptions.GetOptionInstance()
                .OptionCollection["Counter.Alphanumeric.Range"].Value;

            var counter = CreateFlowBlock<CounterFlowBlock>();

            Assert.AreEqual(expected, counter.AlphaRange);
            Assert.IsFalse(string.IsNullOrWhiteSpace(counter.AlphaRange));
        }
    }
}
