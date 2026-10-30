using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavValidationRuleException : NavBaseException
{
	public NavValidationRuleException()
	{
	}

	public NavValidationRuleException(string message)
		: base(message)
	{
	}

	public NavValidationRuleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavValidationRuleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
