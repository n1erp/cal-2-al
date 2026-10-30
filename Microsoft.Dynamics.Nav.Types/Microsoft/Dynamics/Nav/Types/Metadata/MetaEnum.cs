using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaEnum
{
	public const string EnumElementName = "Enum";

	public const string IdAttributeName = "ID";

	public const string NameAttributeName = "Name";

	public const string ExtensibleAttributeName = "Extensible";

	public const string ValuesElementName = "Values";

	public const string ValueElementName = "Value";

	public const string OrdinalAttributeName = "Ordinal";

	[DataMember]
	private List<MetaEnumValue> values = new List<MetaEnumValue>();

	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public bool Extensible { get; set; }

	[DataMember]
	public long MetadataToken { get; set; }

	public List<MetaEnumValue> Values => values;

	public MetaEnum()
	{
	}

	internal MetaEnum Clone()
	{
		MetaEnum metaEnum = MemberwiseClone() as MetaEnum;
		if (Values != null)
		{
			metaEnum.values = new List<MetaEnumValue>(Values.Count);
			for (int i = 0; i < Values.Count; i++)
			{
				metaEnum.values.Add(Values[i].Clone());
			}
		}
		return metaEnum;
	}

	public MetaEnum(XmlNode root)
	{
		for (int i = 0; i < root.Attributes.Count; i++)
		{
			XmlAttribute xmlAttribute = root.Attributes[i];
			switch (xmlAttribute.Name)
			{
			case "ID":
				Id = int.Parse(xmlAttribute.Value, CultureInfo.InvariantCulture);
				break;
			case "Name":
				Name = xmlAttribute.Value;
				break;
			case "MetadataVersion":
				MetadataToken = long.Parse(xmlAttribute.Value, CultureInfo.InvariantCulture);
				break;
			case "Extensible":
				Extensible = MetaBase.Int32Value(xmlAttribute.Value) == 1;
				break;
			}
		}
		foreach (XmlNode childNode in root.ChildNodes)
		{
			string name = childNode.Name;
			if (name == null || !(name == "Values"))
			{
				continue;
			}
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				string name2 = childNode2.Name;
				if (name2 != null && name2 == "Value")
				{
					Values.Add(new MetaEnumValue(childNode2));
				}
			}
		}
	}
}
