using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMetaObjectNotFoundException : NavBuilderException
{
	public NavMetaObjectNotFoundException()
	{
	}

	public NavMetaObjectNotFoundException(string message)
		: base(message)
	{
	}

	public NavMetaObjectNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavMetaObjectNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
