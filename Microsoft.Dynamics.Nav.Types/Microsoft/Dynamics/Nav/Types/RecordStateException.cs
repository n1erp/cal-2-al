using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class RecordStateException : NavBaseException
{
	public RecordStateException()
		: base("An internal error has occurred. The record state is already backed up. Record state does not support nested backup operations.")
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public RecordStateException(string message)
		: base(message)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected RecordStateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	public RecordStateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
