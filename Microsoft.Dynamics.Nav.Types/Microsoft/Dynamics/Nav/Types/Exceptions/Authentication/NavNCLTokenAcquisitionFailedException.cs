using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Authentication;

[Serializable]
public class NavNCLTokenAcquisitionFailedException : NavNCLException
{
	public NavNCLTokenAcquisitionFailedException()
	{
	}

	public NavNCLTokenAcquisitionFailedException(string message)
		: base(message)
	{
	}

	public NavNCLTokenAcquisitionFailedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTokenAcquisitionFailedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLTokenAcquisitionFailedException CreateForResource(string resourceName)
	{
		return new NavNCLTokenAcquisitionFailedException(string.Format(CultureInfo.CurrentCulture, Lang.TokenAcquisitionFailedForResource, resourceName));
	}
}
