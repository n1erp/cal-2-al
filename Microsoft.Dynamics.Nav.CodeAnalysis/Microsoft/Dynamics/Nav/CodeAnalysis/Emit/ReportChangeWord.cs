using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ReportChangeWord : ReportChange
{
	private readonly string customXmlRootName;

	public string XPathFrom { get; set; }

	public string XPathTo { get; set; }

	public ReportChangeWord(ReportChange change, string customXmlRootName)
		: base(change)
	{
		this.customXmlRootName = customXmlRootName;
		XPathFrom = GenerateXpathExpression(SourceParentPath, SourceName, ItemType);
		XPathTo = GenerateXpathExpression(TargetParentPath, TargetName, ItemType);
	}

	public static Dictionary<string, ReportChangeWord> MapChangeList(IDictionary<string, ReportChange> changeList, string customXmlRootName)
	{
		return changeList.Select((KeyValuePair<string, ReportChange> reportChange) => new ReportChangeWord(reportChange.Value, customXmlRootName)).ToDictionary((ReportChangeWord wordChange) => wordChange.XPathFrom);
	}

	private string GenerateXpathExpression(string parentPath, string elementName, ReportChangeOptions.ElementType itemType)
	{
		StringBuilder stringBuilder = new StringBuilder("/ns0:" + customXmlRootName + "[1]");
		if (itemType == ReportChangeOptions.ElementType.Label)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "/ns0:Labels[1]/ns0:{0}[1]", elementName);
		}
		else
		{
			if (string.IsNullOrEmpty(parentPath))
			{
				return string.Empty;
			}
			string[] array = parentPath.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string arg in array)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "/ns0:{0}[1]", arg);
			}
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "/ns0:{0}", elementName);
			if (itemType == ReportChangeOptions.ElementType.Field)
			{
				stringBuilder.Append("[1]");
			}
		}
		return stringBuilder.ToString();
	}
}
