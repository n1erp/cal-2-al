using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclTryFunctionWriteException : NavBaseException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNclTryFunctionWriteException()
	{
	}

	public NavNclTryFunctionWriteException(string message)
		: base(message)
	{
	}

	public NavNclTryFunctionWriteException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclTryFunctionWriteException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNclTryFunctionWriteException Create(string writeMethodName, string tryFunctionName)
	{
		return new NavNclTryFunctionWriteException(string.Format(CultureInfo.CurrentCulture, Lang.MethodWithWriteOperationCalledInsideTry, writeMethodName, tryFunctionName));
	}
}
