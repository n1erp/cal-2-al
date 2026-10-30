using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFormNotOpenedException : NavNCLException
{
	public NavNCLFormNotOpenedException()
		: base(Lang._00095_50070)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFormNotOpenedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFormNotOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLFormNotOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
