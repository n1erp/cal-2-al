using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidJsonDataException : NavNCLException
{
	public NavNCLInvalidJsonDataException()
		: base(string.Format(CultureInfo.CurrentCulture, Lang.NavJsonInvalidJsonData))
	{
	}

	public NavNCLInvalidJsonDataException(string message)
		: base(message)
	{
	}

	protected NavNCLInvalidJsonDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLInvalidJsonDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
