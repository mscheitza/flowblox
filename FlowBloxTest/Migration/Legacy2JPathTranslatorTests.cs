using FlowBlox.Core.Migration;

namespace FlowBloxTest.Migration
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class Legacy2JPathTranslatorTests
    {
        [TestMethod]
        [DataRow("$")]
        [DataRow("$.participant.addresses[0]")]
        [DataRow("$[*].id")]
        [DataRow("$.addresses[?(@.Country == 'Germany')].Street")]
        public void IsNewtonsoftJPath_ValidJPath_ReturnsTrue(string path)
        {
            Assert.IsTrue(Legacy2JPathTranslator.IsNewtonsoftJPath(path));
        }

        [TestMethod]
        [DataRow("participant/addresses", "$.participant.addresses")]
        [DataRow("participant/addresses/0/Street", "$.participant.addresses[0].Street")]
        [DataRow("$/id", "$[*].id")]
        [DataRow("addresses/@Country=Germany/Street", "$.addresses[?(@.Country == 'Germany')].Street")]
        [DataRow("items/@Price>=20/Name", "$.items[?(@.Price >= 20)].Name")]
        [DataRow("items/@Code is null/Name", "$.items[?(@.Code == null)].Name")]
        public void Translate_LegacyPath_ReturnsJPath(string legacyPath, string expected)
        {
            Assert.IsFalse(Legacy2JPathTranslator.IsNewtonsoftJPath(legacyPath));
            Assert.AreEqual(expected, Legacy2JPathTranslator.Translate(legacyPath));
        }

        [TestMethod]
        public void Translate_ExistingJPath_DoesNotChangePath()
        {
            const string path = "$.items[?(@.Price >= 20)].Name";

            Assert.AreEqual(path, Legacy2JPathTranslator.Translate(path));
        }
    }
}
