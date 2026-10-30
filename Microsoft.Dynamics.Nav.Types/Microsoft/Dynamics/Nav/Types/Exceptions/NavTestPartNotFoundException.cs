using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestPartNotFoundException : NavTestBaseException
{
	public NavTestPartNotFoundException()
	{
	}

	public NavTestPartNotFoundException(string message)
		: base(message)
	{
	}

	public NavTestPartNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestPartNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestPartNotFoundException Create(CultureInfo culture, int partId)
	{
		return new NavTestPartNotFoundException(string.Format(culture, Lang.TestPartNotFound, partId));
	}
}
