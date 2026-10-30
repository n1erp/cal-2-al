using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryOrderBy
{
	[DataMember]
	public int QueryColumnId { get; set; }

	[DataMember]
	public MetaQuerySortingType Sorting { get; set; }

	protected MetaQueryOrderBy()
	{
	}

	public MetaQueryOrderBy(XmlNode root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "SORTINGTYPE":
				Sorting = (MetaQuerySortingType)Enum.Parse(typeof(MetaQuerySortingType), childNode.InnerText, ignoreCase: true);
				break;
			case "COLUMNID":
				QueryColumnId = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			}
		}
	}
}
