using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLEnumerationNotSequencedException : NavNCLException
{
	public NavNCLEnumerationNotSequencedException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLEnumerationNotSequencedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLEnumerationNotSequencedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLEnumerationNotSequencedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
