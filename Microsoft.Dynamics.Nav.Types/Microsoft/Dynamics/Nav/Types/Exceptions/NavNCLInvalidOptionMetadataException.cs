using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidOptionMetadataException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLInvalidOptionMetadataException()
	{
	}

	public NavNCLInvalidOptionMetadataException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLInvalidOptionMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidOptionMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidOptionMetadataException Create(CultureInfo culture, string options, string optionValue)
	{
		return new NavNCLInvalidOptionMetadataException(string.Format(culture, Lang.InvalidOptionMetadata, options, optionValue));
	}
}
