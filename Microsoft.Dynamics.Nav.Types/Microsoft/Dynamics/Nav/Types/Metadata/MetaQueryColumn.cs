using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryColumn
{
	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public int FieldNo { get; set; }

	[DataMember]
	public NavType ColumnType { get; set; }

	[DataMember]
	public AggregationType FieldTotalingMethod { get; set; }

	[DataMember]
	public DateMethodType FieldDateMethod { get; set; }

	[DataMember]
	public int QueryColumnIndex { get; set; }

	[DataMember]
	public string CaptionML { get; set; }

	[DataMember]
	public string Caption { get; set; }

	[DataMember]
	public bool FilterOnly { get; set; }

	[DataMember]
	public bool ReverseSign { get; set; }

	public MetaQueryColumn()
	{
	}

	public MetaQueryColumn(XmlNode root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "NAME":
				Name = childNode.InnerText;
				break;
			case "CAPTIONML":
				CaptionML = childNode.InnerText;
				break;
			case "COLUMNTYPE":
				if (string.Compare(childNode.InnerText, "UNDEF", StringComparison.OrdinalIgnoreCase) == 0)
				{
					ColumnType = NavType.None;
				}
				else
				{
					ColumnType = (NavType)Enum.Parse(typeof(NavType), childNode.InnerText);
				}
				break;
			case "FIELDNO":
				FieldNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "TOTALINGMETHOD":
				FieldTotalingMethod = (AggregationType)Enum.Parse(typeof(AggregationType), childNode.InnerText);
				break;
			case "DATEMETHOD":
				FieldDateMethod = (DateMethodType)Enum.Parse(typeof(DateMethodType), childNode.InnerText);
				break;
			case "QUERYCOLUMNINDEX":
				QueryColumnIndex = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "FILTERONLY":
				FilterOnly = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture) == 1;
				break;
			case "REVERSESIGN":
				ReverseSign = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture) == 1;
				break;
			}
		}
	}
}
