using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class XmlDeltaCalculationRules : IXmlDeltaCalculationRules
{
	private const string PromotedActionCategoriesOrder = "PromotedActionCategoriesOrder";

	private const string PromotedCategory = "PromotedCategory";

	private const string Caption = "Caption";

	private const string Trigger = "Trigger";

	private readonly MasterPage originalMasterPage;

	public XmlDeltaCalculationRules(MasterPage originalMasterPage)
	{
		if (originalMasterPage == null)
		{
			throw new ArgumentNullException("originalMasterPage");
		}
		this.originalMasterPage = originalMasterPage;
	}

	public bool IsElementRestrictedFromDelete(string sourceNodeIdentifier)
	{
		return originalMasterPage.RemovedControls?.Contains(sourceNodeIdentifier) ?? false;
	}

	public string ScrubAttributeValueBeforeUpdate(string attributeName, string attributeValue)
	{
		if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
		{
			return attributeValue;
		}
		if (attributeName == "PromotedActionCategoriesOrder")
		{
			XDocument xDocument = XDocument.Parse(attributeValue);
			if (xDocument.Root != null)
			{
				xDocument.Root.Descendants("PromotedCategory").Attributes("Caption").Remove();
				StringBuilder stringBuilder = new StringBuilder();
				StringWriter textWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
				xDocument.Save(textWriter);
				attributeValue = stringBuilder.ToString();
			}
		}
		return attributeValue;
	}

	public bool IsAllowedAsAddDelta(string nodeName)
	{
		return string.CompareOrdinal(nodeName, "Trigger") == 0;
	}

	public bool RemoveDisallowedFromExistingAddDeltas(IXPathNavigable node)
	{
		if (node is XmlNode xmlNode && string.CompareOrdinal(xmlNode.Name, "Trigger") == 0 && xmlNode.ParentNode != null && !xmlNode.ParentNode.IsReadOnly)
		{
			xmlNode.ParentNode.RemoveChild(xmlNode);
			return true;
		}
		return false;
	}
}
