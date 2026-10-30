using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSValidateControlTableRelationException : NavNSValidateFieldException
{
	public NavNSValidateControlTableRelationException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidateControlTableRelationException(string message)
		: base(message)
	{
	}

	public NavNSValidateControlTableRelationException(NavBaseException originalException)
		: base(originalException)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidateControlTableRelationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNSValidateControlTableRelationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
