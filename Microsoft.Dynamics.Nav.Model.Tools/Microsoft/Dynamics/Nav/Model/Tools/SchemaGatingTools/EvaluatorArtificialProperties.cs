using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools.SchemaGatingTools;

internal class EvaluatorArtificialProperties
{
	internal static bool IsPrimary(IElement key)
	{
		return key.Parent.GetSubElements(key.ElementTypeInfo.ElementType).ToList().IndexOf(key) == 0;
	}

	internal static int FirstKeyIndex(IElement field)
	{
		List<IElement> list = field.Parent.GetSubElements(ElementType.Key).ToList();
		IElement element = list.FirstOrDefault((IElement k) => k.GetStringProperty(PropertyType.Key).Split(EvaluatorConstants.ParamSplitString, StringSplitOptions.None).Contains(field.Name));
		if (element != null)
		{
			return list.IndexOf(element);
		}
		return -1;
	}

	internal static bool IsPartOfActiveKey(IElement field)
	{
		return field.Parent.GetSubElements(ElementType.Key).ToList().FirstOrDefault((IElement k) =>
		{
			if (!k.TryGetStringProperty(PropertyType.ObsoleteState, out var value) || value == null)
			{
				value = DefaultPropertyValues.GetDefaultPropertyValue(PropertyType.ObsoleteState);
			}
			if (!k.TryGetStringProperty(PropertyType.Enabled, out var value2) || value2 == null)
			{
				value2 = DefaultPropertyValues.GetDefaultPropertyValue(PropertyType.Enabled);
			}
			return !value.Equals("Removed") && value2.Equals("Yes") && k.GetStringProperty(PropertyType.Key).Split(EvaluatorConstants.ParamSplitString, StringSplitOptions.None).Contains(field.Name);
		}) != null;
	}
}
