using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSCRegionalSettingsException : NavException
{
	public NavSCRegionalSettingsException()
	{
	}

	public NavSCRegionalSettingsException(string message)
		: base(message)
	{
	}

	public NavSCRegionalSettingsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSCRegionalSettingsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
