using System.Xml.XPath;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public interface IXmlDeltaCalculationRules
{
	bool IsElementRestrictedFromDelete(string sourceNodeIdentifier);

	string ScrubAttributeValueBeforeUpdate(string attributeName, string attributeValue);

	bool IsAllowedAsAddDelta(string nodeName);

	bool RemoveDisallowedFromExistingAddDeltas(IXPathNavigable node);
}
