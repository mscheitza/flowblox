using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace FlowBloxTest.Validations
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class ReactiveObjectRequiredStateCheckerTests
    {
        [TestMethod]
        public void IsRequired_ReflectsCurrentRequiredState()
        {
            var target = new RequiredStateFixture();

            Assert.IsTrue(IsRequired(target, nameof(RequiredStateFixture.AlwaysRequired)));
            Assert.IsFalse(IsRequired(target, nameof(RequiredStateFixture.ConditionallyRequired)));
            Assert.IsFalse(IsRequired(target, nameof(RequiredStateFixture.RequiredIfEnabled)));
            Assert.IsFalse(IsRequired(target, nameof(RequiredStateFixture.ActivationRequired)));

            target.Enabled = true;

            Assert.IsTrue(IsRequired(target, nameof(RequiredStateFixture.ConditionallyRequired)));
            Assert.IsTrue(IsRequired(target, nameof(RequiredStateFixture.RequiredIfEnabled)));
            Assert.IsTrue(IsRequired(target, nameof(RequiredStateFixture.ActivationRequired)));
        }

        private static bool IsRequired(RequiredStateFixture target, string propertyName) =>
            ReactiveObjectRequiredStateChecker.IsRequired(
                target,
                typeof(RequiredStateFixture).GetProperty(propertyName)!);

        private sealed class RequiredStateFixture : FlowBloxReactiveObject
        {
            public bool Enabled { get; set; }

            [Required]
            public object AlwaysRequired { get; set; }

            [ConditionallyRequired]
            [ActivationCondition(MemberName = nameof(Enabled), Value = true)]
            public object ConditionallyRequired { get; set; }

            [RequiredIf(nameof(Enabled), true)]
            public object RequiredIfEnabled { get; set; }

            [ActivationCondition(MemberName = nameof(Enabled), Value = true, IsRequired = true)]
            public object ActivationRequired { get; set; }

            public override string ToString() => nameof(RequiredStateFixture);
        }
    }
}
