using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavDatabasePasswordException : NavBaseException
{
	public const int UnknownPasswordErrorCode = 85132501;

	public const int InvalidPasswordErrorCode = 18023638;

	public const int AuthenticationFailedErrorCode = 22925079;

	public NavDatabasePasswordException(string message)
		: base(message)
	{
	}

	public NavDatabasePasswordException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavDatabasePasswordException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavDatabasePasswordException()
	{
	}
}
