using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidStreamDataException : NavNCLException
{
	public NavNCLInvalidStreamDataException()
		: base(Lang.InvalidDataInStream)
	{
	}

	public NavNCLInvalidStreamDataException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidStreamDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidStreamDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
