using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace CommandLine.Parsing;

internal sealed class PropertyWriter
{
	private readonly CultureInfo _parsingCulture;

	public PropertyInfo Property { get; private set; }

	public PropertyWriter(PropertyInfo property, CultureInfo parsingCulture)
	{
		_parsingCulture = parsingCulture;
		Property = property;
	}

	public bool WriteScalar(string value, object target)
	{
		try
		{
			object value2 = ((!Property.PropertyType.IsEnum) ? Convert.ChangeType(value, Property.PropertyType, _parsingCulture) : Enum.Parse(Property.PropertyType, value, ignoreCase: true));
			Property.SetValue(target, value2, null);
		}
		catch (InvalidCastException)
		{
			return false;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (OverflowException)
		{
			return false;
		}
		return true;
	}

	public bool WriteNullable(string value, object target)
	{
		NullableConverter nullableConverter = new NullableConverter(Property.PropertyType);
		try
		{
			Property.SetValue(target, nullableConverter.ConvertFromString(null, _parsingCulture, value), null);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
}
