using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class ConverterElementChecker
{
	private static readonly HashSet<string> ElementTypeWhitelistForUpdate = new HashSet<string> { "ActionDefinition", "ActionGroupDefinition", "ControlDefinition", "ControlGroupDefinition", "InfopartPageDefinition", "InfopartSystemDefinition", "InfopartChartDefinition", "SeparatorDefinition", "StackGroupDefinition" };

	private static readonly HashSet<string> ElementTypeWhitelistForMove = new HashSet<string> { "ActionGroupDefinition", "ControlGroupDefinition", "StackGroupDefinition", "ActionContainerPlaceHolder", "ControlContainerPlaceHolder", "NavigationGroupDefinition" };

	private static readonly HashSet<string> ElementTypeWhitelistForOrder = new HashSet<string> { "ActionGroupDefinition", "ControlGroupDefinition", "StackGroupDefinition", "ActionContainerPlaceHolder", "ControlContainerPlaceHolder", "NavigationGroupDefinition" };

	private static readonly HashSet<string> ElementTypeWhitelistForAdd = new HashSet<string> { "ActionGroupDefinition", "ControlGroupDefinition", "StackGroupDefinition", "ActionContainerPlaceHolder", "ControlContainerPlaceHolder", "NavigationGroupDefinition" };

	private static readonly HashSet<string> ElementTypeWhitelistForDelete = new HashSet<string> { "ActionDefinition", "ActionGroupDefinition", "ControlDefinition", "ControlGroupDefinition", "InfopartPageDefinition", "InfopartSystemDefinition", "InfopartChartDefinition" };

	public static bool VerifyElementType(XmlElement node, PersonalizationV1DeltaType personalizationV1DeltaType)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		bool result = false;
		if (node.Attributes["xsi:type"] != null)
		{
			string value = node.Attributes["xsi:type"].Value;
			result = personalizationV1DeltaType switch
			{
				PersonalizationV1DeltaType.Add => ElementTypeWhitelistForAdd.Contains(value), 
				PersonalizationV1DeltaType.Delete => ElementTypeWhitelistForDelete.Contains(value), 
				PersonalizationV1DeltaType.Move => ElementTypeWhitelistForMove.Contains(value), 
				PersonalizationV1DeltaType.Order => ElementTypeWhitelistForOrder.Contains(value), 
				PersonalizationV1DeltaType.Update => ElementTypeWhitelistForUpdate.Contains(value), 
				_ => throw new InvalidOperationException("Unreachable"), 
			};
		}
		return result;
	}
}
