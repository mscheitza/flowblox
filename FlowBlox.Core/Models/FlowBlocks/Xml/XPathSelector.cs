using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    public static class XPathSelector
    {
        public static List<string> SelectValues(
            XmlDocument xmlDocument,
            string xpath)
        {
            ArgumentNullException.ThrowIfNull(xmlDocument);

            var nodes = xmlDocument.SelectNodes(xpath);
            if (nodes == null || nodes.Count == 0)
                return [];

            var values = new List<string>();

            foreach (XmlNode node in nodes)
            {
                if (node is XmlAttribute or XmlText or XmlCDataSection)
                {
                    values.Add(node.Value ?? string.Empty);
                    continue;
                }

                var value = node.InnerXml?.Trim();
                if (!string.IsNullOrEmpty(value))
                    values.Add(value);
            }

            return values;
        }
    }
}
