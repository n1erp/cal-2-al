using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAutomationVariableNotCreatedException : NavNCLException
{
	public NavNCLAutomationVariableNotCreatedException()
		: base(Lang._00095_50120)
	{
	}

	public NavNCLAutomationVariableNotCreatedException(string message)
		: base(message)
	{
	}

	public NavNCLAutomationVariableNotCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAutomationVariableNotCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
