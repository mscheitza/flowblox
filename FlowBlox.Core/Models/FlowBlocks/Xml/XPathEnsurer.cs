using System.Text.RegularExpressions;
using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    public static class XPathEnsurer
    {
        public static void EnsureXPathExists(XmlNode baseNode, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xpath))
                return;

            var doc = baseNode.OwnerDocument ?? (baseNode as XmlDocument);
            var parts = xpath.Trim('/').Split('/');
            XmlNode current = baseNode;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                // Process attributes at the end of the chain
                if (part.StartsWith("@"))
                {
                    if (current is XmlElement el)
                    {
                        string localAttrName = part.Substring(1);
                        if (!el.HasAttribute(localAttrName))
                            el.SetAttribute(localAttrName, "");
                    }
                    return;
                }

                // Process predicate e.g. participant[@id='123']
                var match = Regex.Match(part, @"^(?<name>[^\[]+)(\[@(?<attr>[^=]+)='(?<value>[^']+)'\])?$");
                if (!match.Success)
                    throw new InvalidOperationException($"Invalid XPath part: {part}");

                string nodeName = match.Groups["name"].Value;
                string attrName = match.Groups["attr"].Value;
                string attrValue = match.Groups["value"].Value;

                // XPath mit Prädikat aufbauen für Suche
                string searchXPath = nodeName;
                if (!string.IsNullOrEmpty(attrName))
                    searchXPath += $"[@{attrName}='{attrValue}']";

                XmlNode next = current.SelectSingleNode(searchXPath);

                if (next == null)
                {
                    XmlElement newElement = doc.CreateElement(nodeName);
                    if (!string.IsNullOrEmpty(attrName))
                        newElement.SetAttribute(attrName, attrValue);

                    current.AppendChild(newElement);
                    current = newElement;
                }
                else
                {
                    current = next;
                }
            }
        }
    }
}
