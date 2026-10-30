using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFileNotOpenedException : NavNCLException
{
	public NavNCLFileNotOpenedException()
		: base(Lang._00095_50040)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFileNotOpenedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFileNotOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLFileNotOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
