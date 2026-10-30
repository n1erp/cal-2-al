using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace Microsoft.Dynamics.Nav.Types;

public class ReadOnlyCollectionConverter : ConfigurationConverterBase
{
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value == null)
		{
			return null;
		}
		IEnumerable<string> enumerable = value as IEnumerable<string>;
		if (enumerable != null)
		{
			return string.Join(", ", enumerable);
		}
		if (value is IEnumerable source)
		{
			enumerable = from object o in source
				select o.ToString();
		}
		if (enumerable != null)
		{
			return string.Join(", ", enumerable);
		}
		return null;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new ReadOnlyCollection<string>(new string[0]);
			}
			string[] array = text.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Trim();
			}
			return new ReadOnlyCollection<string>(array);
		}
		if (value is string[] list)
		{
			return new ReadOnlyCollection<string>(list);
		}
		try
		{
			object[] array2 = ((dynamic)value).ToArray();
			string[] array3 = new string[array2.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				object obj = array2[j];
				array3[j] = obj.ToString();
			}
			return new ReadOnlyCollection<string>(array3);
		}
		catch (RuntimeBinderException)
		{
			return value;
		}
	}
}
