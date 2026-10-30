using System;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ExternalDataItemConverter
{
	public static NavValue GetNavValueFromValue(PropertyInfo propertyInfo, object value)
	{
		if (value == null)
		{
			return GetNavValueFromType(propertyInfo.PropertyType, value);
		}
		return GetNavValueFromValue(value);
	}

	public static NavValue GetNavValueFromType(Type type, object value)
	{
		if (type == typeof(string) || type == typeof(string))
		{
			return NavText.CreateFromObject(value);
		}
		if (type == typeof(long))
		{
			return NavBigInteger.CreateFromObject(value);
		}
		if (type == typeof(int) || type == typeof(int))
		{
			return NavInteger.CreateFromObject(value);
		}
		if (type == typeof(DateTime) || type == typeof(DateTime?))
		{
			return NavDateTime.CreateFromObject(value);
		}
		if (type == typeof(double))
		{
			return NavDecimal.CreateFromObject(value);
		}
		if (type == typeof(byte[]))
		{
			return NavValue.CreateNavValueFromBytes(null, (byte[])value);
		}
		if (type == typeof(bool))
		{
			return NavBoolean.CreateFromObject(value);
		}
		return NavText.CreateFromObject(value?.ToString());
	}

	public static NavValue GetNavValueFromValue(object value)
	{
		byte[] array = value as byte[];
		if (value is string)
		{
			return NavText.CreateFromObject(value);
		}
		if (value is DateTime)
		{
			return NavDateTime.CreateFromObject(value);
		}
		if (value is int)
		{
			return NavInteger.CreateFromObject((int)value);
		}
		if (value is long)
		{
			return NavBigInteger.CreateFromObject((long)value);
		}
		if (value is double)
		{
			return NavDecimal.CreateFromObject((double)value);
		}
		if (array != null)
		{
			return NavValue.CreateNavValueFromBytes(null, array);
		}
		if (value is bool)
		{
			return NavBoolean.CreateFromObject((bool)value);
		}
		return NavText.CreateFromObject(value.ToString());
	}
}
