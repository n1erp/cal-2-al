using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientInvalidPersonalizationMetadataException : NavClientWinFormsException
{
	public NavClientInvalidPersonalizationMetadataException()
		: base(string.Format(CultureInfo.InvariantCulture, Lang.InvalidPersonalizationMetadataFormat))
	{
	}

	public NavClientInvalidPersonalizationMetadataException(string message)
		: base(message)
	{
	}

	public NavClientInvalidPersonalizationMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavClientInvalidPersonalizationMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
