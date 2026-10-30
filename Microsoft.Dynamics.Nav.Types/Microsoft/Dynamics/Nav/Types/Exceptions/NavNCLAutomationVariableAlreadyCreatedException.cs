using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAutomationVariableAlreadyCreatedException : NavNCLException
{
	public NavNCLAutomationVariableAlreadyCreatedException()
		: base(Lang._00095_50119)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLAutomationVariableAlreadyCreatedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLAutomationVariableAlreadyCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAutomationVariableAlreadyCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
