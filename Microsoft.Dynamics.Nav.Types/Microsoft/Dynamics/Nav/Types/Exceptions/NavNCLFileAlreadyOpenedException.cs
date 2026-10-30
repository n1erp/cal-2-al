using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFileAlreadyOpenedException : NavNCLException
{
	public NavNCLFileAlreadyOpenedException()
		: base(Lang._00095_50041)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFileAlreadyOpenedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFileAlreadyOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLFileAlreadyOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
