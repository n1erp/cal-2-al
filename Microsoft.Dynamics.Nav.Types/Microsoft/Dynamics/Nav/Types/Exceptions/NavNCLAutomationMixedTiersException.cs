using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAutomationMixedTiersException : NavNCLException
{
	public NavNCLAutomationMixedTiersException()
		: base(Lang._00095_50171)
	{
	}

	public NavNCLAutomationMixedTiersException(string message)
		: base(message)
	{
	}

	public NavNCLAutomationMixedTiersException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAutomationMixedTiersException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
