using FlowBlox.Core.Models.Components.IO;
using OfficeOpenXml;
using System.Data;

namespace FlowBloxTest.FlowBlocks.IO
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class ExcelTableTests
    {
        [TestMethod]
        public void Read_EmptyWorksheet_ReturnsEmptyTable()
        {
            ExcelPackage.License.SetNonCommercialPersonal("FlowBlox Tests");
            var dataSource = new TestDataObject();
            var excelTable = new ExcelTable { DataSource = dataSource };

            excelTable.Write(new DataTable());

            var result = excelTable.Read();

            Assert.AreEqual(0, result.Columns.Count);
            Assert.AreEqual(0, result.Rows.Count);
        }

        private sealed class TestDataObject : DataObjectBase
        {
            public override byte[] Content { get; set; } = Array.Empty<byte>();

            public override bool CanRead() => Content.Length > 0;
        }
    }
}
