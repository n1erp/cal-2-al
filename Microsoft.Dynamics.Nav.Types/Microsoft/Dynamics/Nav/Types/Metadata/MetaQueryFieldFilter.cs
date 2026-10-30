using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryFieldFilter : MetaQueryFilter
{
	[DataMember]
	public int FieldNo { get; set; }

	public MetaQueryFieldFilter()
	{
	}

	public MetaQueryFieldFilter(XmlNode root)
		: base(root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			string text = childNode.Name.ToUpper(CultureInfo.InvariantCulture);
			if (text != null && text == "FIELDNO")
			{
				FieldNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
			}
		}
	}
}
