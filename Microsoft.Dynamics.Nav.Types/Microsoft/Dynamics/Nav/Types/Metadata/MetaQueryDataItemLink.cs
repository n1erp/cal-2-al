using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryDataItemLink
{
	[DataMember]
	public string SourceDataItemName { get; set; }

	[DataMember]
	public int SourceFieldNo { get; set; }

	[DataMember]
	public int DestinationFieldNo { get; set; }

	[DataMember]
	public string LinkOperator { get; set; }

	public MetaQueryDataItemLink()
	{
	}

	public MetaQueryDataItemLink(XmlNode root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "SOURCEDATAITEM":
				SourceDataItemName = childNode.InnerText;
				break;
			case "SOURCEFIELD":
				SourceFieldNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "DESTINATIONFIELD":
				DestinationFieldNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "LINKOPERATOR":
				LinkOperator = childNode.InnerText;
				break;
			}
		}
	}
}
