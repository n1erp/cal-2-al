using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using Microsoft.CSharp.RuntimeBinder;

namespace Microsoft.Dynamics.Nav.Types;

public class TimeZoneInfoConverter : ConfigurationConverterBase
{
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		return ((TimeZoneInfo)value).Id;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			return null;
		}
		string text = value as string;
		if (text != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				return TimeZoneInfoResolver.ResolveFromId(text);
			}
			return null;
		}
		if (value is TimeZoneInfo timeZoneInfo)
		{
			text = timeZoneInfo.Id;
		}
		try
		{
			text = ((dynamic)value).Id;
		}
		catch (RuntimeBinderException)
		{
		}
		if (!string.IsNullOrEmpty(text))
		{
			return TimeZoneInfoResolver.ResolveFromId(text);
		}
		return null;
	}
}
