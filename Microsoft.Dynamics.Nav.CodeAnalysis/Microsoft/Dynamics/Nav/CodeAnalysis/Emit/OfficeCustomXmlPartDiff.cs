using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class OfficeCustomXmlPartDiff
{
	[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
	public static Dictionary<string, ReportChangeWord> GetChangedCustomXmlList(XElement oldXml, XElement newXml, string customXmlRootName, out string xmlNamespace, out bool forceCustomXmlUpdate)
	{
		xmlNamespace = GetXmlNamespaceChanges(oldXml, newXml);
		Dictionary<string, ReportChangeWord> result = new Dictionary<string, ReportChangeWord>();
		if (oldXml == null)
		{
			forceCustomXmlUpdate = true;
			return result;
		}
		Dictionary<string, ReportChange> changeList = ReportChangeMetadataComparer.CompareReportMetadata(oldXml, newXml, customXmlRootName);
		CountItemsInXmlPart(oldXml, customXmlRootName, out var parameterCount, out var dataItemCount, out var columnCount);
		CountItemsInXmlPart(newXml, customXmlRootName, out var parameterCount2, out var dataItemCount2, out var columnCount2);
		forceCustomXmlUpdate = parameterCount != parameterCount2 || dataItemCount != dataItemCount2 || columnCount != columnCount2;
		return ReportChangeWord.MapChangeList(changeList, customXmlRootName);
	}

	private static void CountItemsInXmlPart(XElement oldXml, string customXmlRootName, out int parameterCount, out int dataItemCount, out int columnCount)
	{
		parameterCount = 0;
		dataItemCount = 0;
		columnCount = 0;
		XElement xElement = oldXml.Element(customXmlRootName);
		if (xElement == null)
		{
			return;
		}
		parameterCount = xElement.Elements("Labels").Count();
		foreach (XElement item in xElement.Descendants())
		{
			XAttribute xAttribute;
			if ((xAttribute = item.Attribute("ElementType")) != null)
			{
				if (xAttribute.Value.Equals("DataItem", StringComparison.Ordinal))
				{
					dataItemCount++;
				}
				else if (xAttribute.Value.Equals("Column", StringComparison.Ordinal))
				{
					columnCount++;
				}
			}
		}
	}

	private static string GetXmlNamespaceChanges(XElement oldXml, XElement newXml)
	{
		XNamespace xNamespace = newXml.Name.Namespace;
		if (oldXml == null)
		{
			return xNamespace.ToString();
		}
		if (!(oldXml.Name.Namespace != xNamespace))
		{
			return string.Empty;
		}
		return xNamespace.ToString();
	}
}
