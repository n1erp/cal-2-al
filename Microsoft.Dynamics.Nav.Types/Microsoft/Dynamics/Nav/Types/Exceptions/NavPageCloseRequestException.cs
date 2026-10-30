using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavPageCloseRequestException : NavException
{
	public NavPageCloseRequestException()
	{
	}

	public NavPageCloseRequestException(string message)
		: base(message)
	{
	}

	public NavPageCloseRequestException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavPageCloseRequestException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavPageCloseRequestException Create()
	{
		return new NavPageCloseRequestException(string.Empty, null)
		{
			SuppressMessage = true
		};
	}
}
