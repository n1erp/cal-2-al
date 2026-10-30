using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaReportLabel
{
	public string Name { get; private set; }

	public int Id { get; private set; }

	public bool PerRow { get; private set; }

	public int ParentDataItem { get; private set; }

	public string CaptionML { get; private set; }

	public int? FieldNo { get; private set; }

	public int? TableNo { get; private set; }

	public int DataSetIndex { get; internal set; }

	public MetaReportLabel(XmlNode labelNode, MetaReport parent)
	{
		if (labelNode == null)
		{
			throw new ArgumentNullException("labelNode");
		}
		foreach (XmlNode childNode in labelNode.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "NAME":
				Name = childNode.InnerText;
				break;
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "PERROW":
				PerRow = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "PARENTDATAITEM":
				ParentDataItem = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "CAPTIONML":
				CaptionML = childNode.InnerText;
				break;
			case "FIELDNO":
				FieldNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "TABLENO":
				TableNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			}
		}
		CaptionML = MetaTable.MultiLanguageTextMerge(CaptionML, 0, ObjectType.Report, parent.Id, Id, parent.LanguageAppGroupId);
	}
}
