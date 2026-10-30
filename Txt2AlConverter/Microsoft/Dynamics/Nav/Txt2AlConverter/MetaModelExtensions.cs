using System;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal static class MetaModelExtensions
{
	public static bool IsOfType(this IElement element, ElementType type)
	{
		return element.ElementTypeInfo.ElementType == type;
	}

	public static bool IsUserControl(this IElement element)
	{
		if (element.TryGetStringProperty(PropertyType.ControlAddIn, out var value))
		{
			return !string.IsNullOrEmpty(value);
		}
		return false;
	}

	public static bool IsLabelControl(this IElement element)
	{
		return AlUtilities.ControlTypeToControlKind(element) == ControlKind.Label;
	}

	public static bool IsDotNetEventTrigger(this IElement element)
	{
		if (element.ElementTypeInfo.ElementType != ElementType.Event)
		{
			return false;
		}
		if (!element.TryGetStringProperty(PropertyType.EventSource, out var eventSource) || string.IsNullOrEmpty(eventSource))
		{
			return false;
		}
		return element.GetContainingApplicationObjectElement().GetSubElements(ElementType.Variable).Any((IElement e) => e.Name == eventSource && e.IsOfType("DotNet"));
	}

	public static Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo GetPropertyInfo(this IElement element, PropertyType property)
	{
		return element.ElementTypeInfo.Properties.Values.Select((ElementProperty v) => v.Property).FirstOrDefault((Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo p) => p.PropertyType == property);
	}

	internal static IElement GetContainingApplicationObjectElement(this IElement element)
	{
		IElement parent = element.Parent;
		while (parent.Parent != null)
		{
			parent = parent.Parent;
		}
		return parent;
	}

	public static bool IsOfUnknownType(this IElement element)
	{
		return element.GetDataType() == null;
	}

	public static bool IsOfType(this IElement element, string alType)
	{
		return string.Equals(element.GetDataType(), alType, StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsOfAnyType(this IElement element, params string[] alTypes)
	{
		string dataType = element.GetDataType();
		foreach (string text in alTypes)
		{
			if (string.Equals(dataType, text, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if ((string.Equals(text, "text", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "code", StringComparison.OrdinalIgnoreCase)) && dataType != null && dataType.StartsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static string GetDataType(this IElement element)
	{
		if (element.TryGetStringProperty(PropertyType.DataType, out var value))
		{
			return value;
		}
		if (element.TryGetStringProperty(PropertyType.ImplicitType, out value))
		{
			return value;
		}
		return null;
	}
}
