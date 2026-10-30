using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLRenameArgumentException : NavNCLException
{
	public NavNCLRenameArgumentException()
		: base(Lang._00095_50048)
	{
	}

	public NavNCLRenameArgumentException(string message)
		: base(message)
	{
	}

	public NavNCLRenameArgumentException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLRenameArgumentException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
