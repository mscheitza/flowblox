using FlowBlox.Core.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml;
using static FlowBlox.Core.Models.Components.OptionElement;

namespace FlowBlox.Core.Models.Components
{
    public class OptionElement
    {
        public enum OptionType
        {
            Text,
            Password,
            Integer,
            Boolean
        };

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public OptionType Type { get; set; }
        public bool SystemOption { get; internal set; }

        public OptionElement()
        {
            this.Type = OptionType.Text;
        }

        private string _internalValue { get; set; }
        public string Value
        {
            get
            {
                if (Type == OptionType.Password)
                {
                    if (string.IsNullOrEmpty(_internalValue))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return FlowBloxSecureStorageManager.GetProtectedData(Name, _internalValue);
                    }

                }
                return (_internalValue != null) ? Environment.ExpandEnvironmentVariables(_internalValue) : string.Empty;
            }
            set
            {
                if (Type == OptionType.Password)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this._internalValue = string.Empty;
                    }
                    else
                    {
                        var encryptedData = FlowBloxSecureStorageManager.SetProtectedData(Name, value);
                        this._internalValue = encryptedData;
                    }
                }
                else
                {
                    this._internalValue = value;
                }
            }
        }

        public OptionElement(string name, string value, string description, OptionType type, string displayName = null)
        {
            this.Name = name;
            this.DisplayName = displayName;
            this._internalValue = value;
            this.Description = description;
            this.Type = type;
            this.SystemOption = true;
        }

        public void Validate()
        {
            if (this.Type == OptionType.Integer)
            {
                if (!int.TryParse(Value, out _))
                {
                    throw new ValidationException($"The value '{Value}' of the option '{Name}' is not a valid integer.");
                }
            }
            else if (this.Type == OptionType.Boolean)
            {
                if (!bool.TryParse(Value, out _))
                {
                    throw new ValidationException($"The value '{Value}' of the option '{Name}' is not a valid boolean.");
                }
            }
        }

        public int GetValueInt()
        {
            int value;
            if (int.TryParse(Value, out value))
            {
                return value;
            }
            return 0;
        }

        public bool GetValueBoolean()
        {
            bool value;
            if (bool.TryParse(Value, out value))
            {
                return value;
            }
            return false;
        }

        public List<string> GetValuestrings()
        {
            return Value.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public Dictionary<string, string> GetValueMap()
        {
            Dictionary<string, string> Map = new Dictionary<string, string>();

            foreach (string Value in GetValuestrings())
            {
                string[] astrValue = Value.Split('=');

                string Key = astrValue[0];
                string MapValue = (astrValue.Length > 1) ? astrValue[1] : string.Empty;

                Map[Key] = MapValue;
            }

            return Map;
        }

        internal static OptionElement FromXml(XmlNode xmlNode)
        {
            OptionElement optionElement = new OptionElement();

            string name = xmlNode.Attributes["name"].InnerText;
            string displayName = xmlNode.Attributes["displayName"].InnerText;
            string value = xmlNode.Attributes["value"].InnerText;
            string description = xmlNode.Attributes["desc"].InnerText;
            OptionType Type = (OptionType)Enum.Parse(typeof(OptionType), xmlNode.Attributes["type"].InnerText);

            optionElement.Name = name;
            optionElement.DisplayName = displayName;
            optionElement._internalValue = value;
            optionElement.Description = description;
            optionElement.Type = Type;
            optionElement.SystemOption = xmlNode.Attributes["system"].InnerText.Equals("True");

            return optionElement;
        }

        internal XmlNode SaveXml(XmlDocument xmlDocument)
        {
            XmlNode xnOptionElement = xmlDocument.CreateElement("option_element");

            XmlAttribute xaName = xmlDocument.CreateAttribute("name");
            XmlAttribute xaDisplayName = xmlDocument.CreateAttribute("displayName");
            XmlAttribute xaValue = xmlDocument.CreateAttribute("value");
            XmlAttribute xaDesc = xmlDocument.CreateAttribute("desc");
            XmlAttribute xaType = xmlDocument.CreateAttribute("type");
            XmlAttribute xaSystem = xmlDocument.CreateAttribute("system");

            xaName.InnerText = Name;
            xaDisplayName.InnerText = DisplayName;
            xaValue.InnerText = _internalValue;
            xaDesc.InnerText = Description;
            xaType.InnerText = Type.ToString();
            xaSystem.InnerText = SystemOption.ToString();

            xnOptionElement.Attributes.Append(xaName);
            xnOptionElement.Attributes.Append(xaDisplayName);
            xnOptionElement.Attributes.Append(xaValue);
            xnOptionElement.Attributes.Append(xaDesc);
            xnOptionElement.Attributes.Append(xaType);
            xnOptionElement.Attributes.Append(xaSystem);

            return xnOptionElement;
        }
    }
}
