using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

[Serializable]
public class NavAppExternalDeployException : NavAppException
{
	public NavAppExternalDeployException()
	{
	}

	public NavAppExternalDeployException(string message)
		: base(message)
	{
	}

	public NavAppExternalDeployException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppExternalDeployException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
