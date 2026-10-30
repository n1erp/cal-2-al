using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidTextualFilterGroupException : NavNCLException
{
	public NavNCLInvalidTextualFilterGroupException()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidTextualFilterGroupException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidTextualFilterGroupException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidTextualFilterGroupException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
