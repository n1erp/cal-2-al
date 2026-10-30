using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public static class LinqToXmlExtensions
{
	public static T GetElementValue<T>(this XContainer element, Enum elementName, bool ignoreCase = false)
	{
		if (!element.Elements(elementName.ToString()).Any())
		{
			return default;
		}
		return ParseValue<T>(element.Element(elementName.ToString()).Value, default, ignoreCase);
	}

	public static T GetAttribute<T>(this XElement element, string attributeName, T defaultValue)
	{
		if (!element.Attributes(attributeName).Any())
		{
			return defaultValue;
		}
		return ParseValue(element.Attribute(attributeName).Value, defaultValue);
	}

	private static T ParseValue<T>(object value, T defaultValue, bool ignoreCase = false)
	{
		try
		{
			if (typeof(T).GetTypeInfo().IsEnum)
			{
				return (T)Enum.Parse(typeof(T), value.ToString(), ignoreCase);
			}
			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.CurrentCulture);
		}
		catch (ArgumentException)
		{
			return defaultValue;
		}
		catch (InvalidCastException)
		{
			return defaultValue;
		}
	}
}
