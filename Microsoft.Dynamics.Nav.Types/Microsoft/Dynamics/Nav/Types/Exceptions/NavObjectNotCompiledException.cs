using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavObjectNotCompiledException : NavCSideException
{
	public const int TableNotCompiledErrorCode = 18023732;

	public NavObjectNotCompiledException()
	{
	}

	public NavObjectNotCompiledException(string message)
		: base(message)
	{
	}

	public NavObjectNotCompiledException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavObjectNotCompiledException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavObjectNotCompiledException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
	}

	protected NavObjectNotCompiledException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
