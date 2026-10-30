using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public abstract class MetaQueryFilter
{
	[DataMember]
	public FilterType TypeOfFilter { get; set; }

	[DataMember]
	public string Value { get; set; }

	protected MetaQueryFilter()
	{
	}

	protected MetaQueryFilter(XmlNode root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "FILTERTYPE":
				TypeOfFilter = (FilterType)Enum.Parse(typeof(FilterType), childNode.InnerText, ignoreCase: true);
				break;
			case "FILTERVALUE":
				Value = childNode.InnerText;
				break;
			}
		}
	}
}
