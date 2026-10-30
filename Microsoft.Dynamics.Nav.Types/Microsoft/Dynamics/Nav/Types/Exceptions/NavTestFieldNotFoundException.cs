using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestFieldNotFoundException : NavTestBaseException
{
	public NavTestFieldNotFoundException()
	{
	}

	public NavTestFieldNotFoundException(string message)
		: base(message)
	{
	}

	public NavTestFieldNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestFieldNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestFieldNotFoundException Create(CultureInfo culture, int fieldId)
	{
		return new NavTestFieldNotFoundException(string.Format(culture, Lang.TestFieldNotFound, fieldId));
	}
}
