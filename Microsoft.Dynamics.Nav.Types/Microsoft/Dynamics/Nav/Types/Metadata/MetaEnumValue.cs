using System.Globalization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaEnumValue
{
	public int Ordinal { get; set; }

	public string Name { get; set; }

	public string CaptionML { get; set; }

	public MetaEnumValue()
	{
	}

	public MetaEnumValue(XmlNode enumValueNode)
	{
		for (int i = 0; i < enumValueNode.Attributes.Count; i++)
		{
			XmlAttribute xmlAttribute = enumValueNode.Attributes[i];
			switch (xmlAttribute.Name)
			{
			case "Ordinal":
				Ordinal = int.Parse(xmlAttribute.Value, CultureInfo.InvariantCulture);
				break;
			case "Name":
				Name = xmlAttribute.Value;
				break;
			}
		}
		foreach (XmlNode childNode in enumValueNode.ChildNodes)
		{
			string name = childNode.Name;
			if (name != null && name == "CaptionML")
			{
				CaptionML = childNode.InnerText;
			}
		}
	}

	internal MetaEnumValue Clone()
	{
		return (MetaEnumValue)MemberwiseClone();
	}
}
