using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLRecordAlreadyExistsException : NavException
{
	public NavNCLRecordAlreadyExistsException()
		: base(Lang.RecordAlreadyExists)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLRecordAlreadyExistsException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLRecordAlreadyExistsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLRecordAlreadyExistsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
