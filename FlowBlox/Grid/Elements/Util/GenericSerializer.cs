using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace FlowBlox.Grid.Elements.Util
{
    public static class GenericSerializer
    {
        public static T FromXml<T>(XmlNode xmlNode) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlNode.OuterXml)))
            using (var sr = new StreamReader(ms))
            {
                return (T)serializer.Deserialize(sr);
            }
        }

        public static XmlNode SaveXml<T>(T obj, XmlDocument xmlDocument) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            string xmlContent;
            using (var sw = new StringWriter())
            using (XmlTextWriter xtw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(xtw, obj);
                xmlContent = sw.ToString();
            }

            XmlDocument _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(xmlContent);

            var importedNode = xmlDocument.ImportNode(_xmlDocument.FirstChild, true);
            return importedNode;
        }
    }
}
