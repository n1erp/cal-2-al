using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public static class ExtensionMethods
{
	public static bool HasProperty(this InlineItem[] items, PropertyType propertyType)
	{
		return items?.Any((InlineItem t) => t.Property == propertyType) ?? false;
	}

	public static bool IsTrue(this IElement element, PropertyType propertyType)
	{
		if (element.GetPropertySetState(propertyType))
		{
			return element.GetStringProperty(propertyType) == "Yes";
		}
		return false;
	}

	public static bool IsFalse(this IElement element, PropertyType propertyType)
	{
		if (element.GetPropertySetState(propertyType))
		{
			return element.GetStringProperty(propertyType) == "No";
		}
		return false;
	}
}
