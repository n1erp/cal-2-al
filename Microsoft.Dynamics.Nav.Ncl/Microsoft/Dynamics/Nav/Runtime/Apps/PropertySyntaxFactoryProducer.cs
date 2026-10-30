using System;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class PropertySyntaxFactoryProducer
{
	internal static PropertySyntaxAbstractFactory GetFactory(XmlElement node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		switch (node.Attributes["xsi:type"]?.Value)
		{
		case "ActionGroupDefinition":
			return new ActionGroupPropertySyntaxFactory();
		case "ActionDefinition":
			return new ActionPropertySyntaxFactory();
		case "ControlDefinition":
			return new ControlPropertySyntaxFactory();
		case "ControlGroupDefinition":
			return new ControlGroupPropertySyntaxFactory();
		case "InfopartPageDefinition":
		case "InfopartChartDefinition":
		case "InfopartSystemDefinition":
			return new InfopartPropertySyntaxFactory();
		case "SeparatorDefinition":
			return new SeparatorPropertySyntaxFactory();
		case "StackGroupDefinition":
			return new StackGroupPropertySyntaxFactory();
		default:
			throw new InvalidOperationException("Unsupported");
		}
	}
}
