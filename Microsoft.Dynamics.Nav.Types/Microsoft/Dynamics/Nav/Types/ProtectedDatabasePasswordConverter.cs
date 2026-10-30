using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public class ProtectedDatabasePasswordConverter : ConfigurationConverterBase
{
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		return ((ProtectedDatabasePassword)value).Value;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (!(value is string value2))
		{
			return null;
		}
		return ProtectedDatabasePassword.Create(SecureStringExtension.CreateFromUnsecureString(value2));
	}
}
