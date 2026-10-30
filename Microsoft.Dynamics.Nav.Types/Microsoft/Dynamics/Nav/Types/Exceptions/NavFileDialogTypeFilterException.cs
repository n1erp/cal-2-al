using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavFileDialogTypeFilterException : NavException
{
	public NavFileDialogTypeFilterException()
	{
	}

	public NavFileDialogTypeFilterException(string message)
		: base(message)
	{
	}

	public NavFileDialogTypeFilterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavFileDialogTypeFilterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
