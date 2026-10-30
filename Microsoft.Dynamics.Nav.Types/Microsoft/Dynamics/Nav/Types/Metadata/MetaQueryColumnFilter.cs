using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryColumnFilter : MetaQueryFilter
{
	[DataMember]
	public int QueryColumnId { get; set; }

	public MetaQueryColumnFilter()
	{
	}

	public MetaQueryColumnFilter(XmlNode root)
		: base(root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			string text = childNode.Name.ToUpper(CultureInfo.InvariantCulture);
			if (text != null && text == "COLUMNID")
			{
				QueryColumnId = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
			}
		}
	}
}
