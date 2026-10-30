using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidCompanyNameException : NavNCLException
{
	public NavNCLInvalidCompanyNameException()
		: base(Lang._00095_50024)
	{
	}

	public NavNCLInvalidCompanyNameException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidCompanyNameException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidCompanyNameException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
