using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Util;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    public class ReflectionHelperTypeEndingTests
    {
        [TestMethod]
        public void GetTypeByFullNameFromLastPart_ResolvesUniqueLastTypeNamePart()
        {
            var resolved = ReflectionHelper.GetTypeByFullNameFromLastPart(
                "Wrong.Namespace.TextOperations.ConcatUriFlowBlock");

            Assert.AreEqual(typeof(ConcatUriFlowBlock), resolved);
        }

        [TestMethod]
        public void GetTypeByFullNameFromLastPart_ExpandsUntilTypeNamePartsAreUnambiguous()
        {
            var resolved = ReflectionHelper.GetTypeByFullNameFromLastPart(
                "One.Shared.TargetType");

            Assert.AreEqual(typeof(ReflectionHelperFixtures.One.Shared.TargetType), resolved);
        }
    }
}

namespace FlowBloxTest.AIAssistant.ReflectionHelperFixtures.One.Shared
{
    public sealed class TargetType
    {
    }
}

namespace FlowBloxTest.AIAssistant.ReflectionHelperFixtures.Two.Shared
{
    public sealed class TargetType
    {
    }
}
