using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLWrongParentRecordException : NavNCLException
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLWrongParentRecordException()
		: base("When creating a record with shared table handle, the parent must be a record and using the same table handle.")
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLWrongParentRecordException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLWrongParentRecordException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLWrongParentRecordException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
