using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Types.Report;

public static class RdlcMetadataDiff
{
	private static readonly XName UniqueIdXName = "UniqueID";

	private static readonly XName IncludeCaptionXName = "IncludeCaption";

	public static IList<ReportChangeRdlc> GetChangedRdlcRenameList(XElement oldXml, XElement newXml)
	{
		if (newXml.GetDefaultNamespace().ToString().Contains("urn:microsoft-dynamics-nav/reports/"))
		{
			return ReportChangeRdlc.MapChangeList(ReportChangeMetadataComparer.CompareReportMetadata(oldXml, newXml, "NavWordReportXmlPart"));
		}
		return GetChangedFieldListFromInternalMetadata(oldXml, newXml);
	}

	private static IList<ReportChangeRdlc> GetChangedFieldListFromInternalMetadata(XElement oldXml, XElement newXml)
	{
		List<ReportChangeRdlc> list = new List<ReportChangeRdlc>();
		foreach (XElement item in oldXml.Descendants(OfficeCustomXmlPart.DataFieldXName))
		{
			bool flag = false;
			string text = (string)item.Element(UniqueIdXName);
			string text2 = (string)item.Element(OfficeCustomXmlPart.NameXName);
			foreach (XElement item2 in newXml.Descendants(OfficeCustomXmlPart.DataFieldXName))
			{
				string text3 = (string)item2.Element(OfficeCustomXmlPart.NameXName);
				if (text != null && text == (string)item2.Element(UniqueIdXName))
				{
					flag = true;
					if (text2 != text3)
					{
						RegisterChangeEntry(item, list, text, text2, text3);
					}
				}
			}
			if (!flag)
			{
				RegisterChangeEntry(item, list, text, text2, string.Empty);
			}
		}
		return list;
	}

	private static void RegisterChangeEntry(XElement oldDataField, ICollection<ReportChangeRdlc> renameEntries, string uniqueId, string oldName, string newName)
	{
		bool isParameter = OfficeCustomXmlPart.FieldIsParameter(oldDataField);
		renameEntries.Add(new ReportChangeRdlc(uniqueId, oldName, newName, isParameter));
		if (oldDataField.Element(IncludeCaptionXName) != null)
		{
			string targetName = (string.IsNullOrEmpty(newName) ? string.Empty : (newName + "Caption"));
			renameEntries.Add(new ReportChangeRdlc(Guid.NewGuid().ToString(), oldName + "Caption", targetName, isParameter: true));
		}
	}
}
