using System;

namespace Microsoft.Dynamics.Nav.Types.Metadata.Extensions;

internal static class PageDefinitionExtensions
{
	internal static ControlBaseDefinition GetControlByIdentifier(this PageDefinition pageDefinition, string controlId)
	{
		return FindControlInContent(pageDefinition.Content, (ControlBaseDefinition cbd) => string.Equals(cbd.ControlGUID, controlId, StringComparison.OrdinalIgnoreCase));
	}

	internal static ControlBaseDefinition GetControlById(this PageDefinition pageDefinition, int controlId)
	{
		return FindControlInContent(pageDefinition.Content, (ControlBaseDefinition cbd) => cbd.ID.Equals(controlId));
	}

	internal static ControlBaseDefinition GetControlByName(this PageDefinition pageDefinition, string controlName)
	{
		return FindControlInContent(pageDefinition.Content, (ControlBaseDefinition cbd) => string.Equals(cbd.Name, controlName));
	}

	private static ControlBaseDefinition FindControlInContent(ContentDefinition content, Predicate<ControlBaseDefinition> match)
	{
		ControlBaseDefinition controlBaseDefinition = null;
		Predicate<ElementDefinition> match2 = (ElementDefinition ed) => ed is ControlBaseDefinition obj && match(obj);
		try
		{
			foreach (ControlContainerDefinition container in content.Containers)
			{
				controlBaseDefinition = container.Find(match2) as ControlBaseDefinition;
				if (controlBaseDefinition != null)
				{
					break;
				}
			}
		}
		catch (NullReferenceException inner)
		{
			throw new NavMetadataNotFoundException("Content Definition object is missing the expected structure.", inner);
		}
		return controlBaseDefinition;
	}
}
