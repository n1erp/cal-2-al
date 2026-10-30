using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

[Serializable]
public class ScaffoldingPathNotFoundException : NavBaseException
{
	public ScaffoldingPathNotFoundException()
	{
	}

	public ScaffoldingPathNotFoundException(string message)
		: base(message)
	{
	}

	public ScaffoldingPathNotFoundException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ScaffoldingPathNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
