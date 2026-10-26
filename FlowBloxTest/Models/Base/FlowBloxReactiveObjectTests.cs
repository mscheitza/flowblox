using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;

namespace FlowBloxTest.Models.Base
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FlowBloxReactiveObjectTests
    {
        [TestMethod]
        public void ConcreteNonComponentTypes_OverrideToString()
        {
            var typesWithoutMeaningfulToString = typeof(FlowBloxReactiveObject).Assembly
                .GetTypes()
                .Where(type =>
                    type != typeof(FlowBloxReactiveObject) &&
                    !type.IsAbstract &&
                    typeof(FlowBloxReactiveObject).IsAssignableFrom(type) &&
                    !typeof(IFlowBloxComponent).IsAssignableFrom(type))
                .Where(type => type.GetMethod(nameof(ToString), Type.EmptyTypes)?.DeclaringType == typeof(object))
                .Select(type => type.FullName)
                .OrderBy(typeName => typeName)
                .ToArray();

            Assert.AreEqual(
                0,
                typesWithoutMeaningfulToString.Length,
                "These reactive object types still use the type-only Object.ToString():" +
                Environment.NewLine +
                string.Join(Environment.NewLine, typesWithoutMeaningfulToString));
        }
    }
}
