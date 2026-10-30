using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class NavCmdletException : ModelToolException
{
	internal NavCmdletException(string message, params object[] arguments)
		: base(message, arguments)
	{
	}

	public NavCmdletException()
	{
	}

	public NavCmdletException(string message)
		: base(message)
	{
	}

	public NavCmdletException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCmdletException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
