using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidChartDefinitionException : NavBaseException
{
	public NavInvalidChartDefinitionException()
	{
	}

	public NavInvalidChartDefinitionException(string message)
		: base(message)
	{
	}

	public NavInvalidChartDefinitionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidChartDefinitionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
