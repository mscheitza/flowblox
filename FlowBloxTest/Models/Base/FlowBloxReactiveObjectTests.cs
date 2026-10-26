using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.FlowBlocks;

namespace FlowBloxTest.Models.Base
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FlowBloxReactiveObjectTests
    {
        [TestMethod]
        public void IsReactiveObjectOnly_DistinguishesOwnedReactiveObjectsFromComponents()
        {
            var reactiveObject = new FlowBloxReactiveObject();
            var component = new FieldElement();

            Assert.IsTrue(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly((object)reactiveObject));
            Assert.IsTrue(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(reactiveObject));
            Assert.IsTrue(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(typeof(FlowBloxReactiveObject)));

            Assert.IsFalse(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(new object()));
            Assert.IsFalse(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly((FlowBloxReactiveObject)component));
            Assert.IsFalse(FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(typeof(FieldElement)));
        }

        [TestMethod]
        public void ConcreteNonComponentTypes_OverrideToString()
        {
            var typesWithoutMeaningfulToString = typeof(FlowBloxReactiveObject).Assembly
                .GetTypes()
                .Where(type =>
                    type != typeof(FlowBloxReactiveObject) &&
                    !type.IsAbstract &&
                    FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(type))
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
