using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public class EncryptionProviderConverter : ConfigurationConverterBase
{
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		return ((EncryptionProvider)value/*cast due to constrained. prefix*/).ToString();
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string value2)
		{
			return Enum.Parse(typeof(EncryptionProvider), value2, ignoreCase: true);
		}
		return EncryptionProvider.LocalKeyFile;
	}
}
