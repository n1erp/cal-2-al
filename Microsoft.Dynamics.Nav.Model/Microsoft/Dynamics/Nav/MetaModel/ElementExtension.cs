using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.MetaModel;

public static class ElementExtension
{
	public static int? GetInt32Property(this IElement element, PropertyType property)
	{
		if (!element.TryGetInt32Property(property, out var value))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Element {0} does not support property {1} or the value is non numeric", element.ElementTypeInfo.Name, property.ToString()));
		}
		return value;
	}

	public static int GetInt32Property(this IElement element, PropertyType property, int defaultValue)
	{
		return element.GetInt32Property(property) ?? defaultValue;
	}

	public static string GetStringProperty(this IElement element, PropertyType property)
	{
		if (!element.TryGetStringProperty(property, out var value))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Element {0} does not support property {1}", element.ElementTypeInfo.Name, property.ToString()));
		}
		return value;
	}

	public static string GetStringProperty(this IElement element, PropertyType property, string defaultValue)
	{
		return element.GetStringProperty(property) ?? defaultValue;
	}

	public static IEnumerable<IElement> GetElements(this IElement element)
	{
		yield return element;
		foreach (IElement subElement in element.GetSubElements())
		{
			foreach (IElement element2 in subElement.GetElements())
			{
				yield return element2;
			}
		}
	}

	public static IEnumerable<IElement> GetElements(this IEnumerable<IElement> elements)
	{
		return elements.SelectMany((IElement e) => e.GetElements());
	}

	public static IEnumerable<IElement> GetElements(this IEnumerable<ApplicationObject> applicationObjects)
	{
		return applicationObjects.SelectMany((ApplicationObject ao) => ao.GetElements());
	}

	public static void ChangePropertyType(this IElement element, PropertyType sourceProperty, PropertyType destinationProperty)
	{
		if (element.GetPropertySetState(sourceProperty))
		{
			element.SetStringProperty(destinationProperty, element.GetStringProperty(sourceProperty));
			element.ClearProperty(sourceProperty);
		}
	}

	public static bool SupportsPropertyType(this IElement element, PropertyType propertyType)
	{
		ElementProperty value;
		return element.ElementTypeInfo.Properties.TryGetValue(propertyType, out value);
	}

	internal static ApplicationObjectId GetApplicationObjectId(this IElement element)
	{
		if (element.ElementTypeInfo.ElementType != ElementType.Modification)
		{
			return new ApplicationObjectId(element.ElementTypeInfo.ElementType, element.Id);
		}
		IElementKeySegment elementKeySegment = ElementKeyHelper.DisplayProvider.GetElementKeySegments(element.GetStringProperty(PropertyType.Target)).Single();
		return new ApplicationObjectId(elementKeySegment.ElementType, (!string.IsNullOrEmpty(elementKeySegment.Id)) ? int.Parse(elementKeySegment.Id, CultureInfo.InvariantCulture) : 0);
	}

	public static void ForEach<T>(this IEnumerable<T> elements, Action<T> action)
	{
		foreach (T element in elements)
		{
			action(element);
		}
	}
}
