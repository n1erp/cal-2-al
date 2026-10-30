using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSFormNotOpenedException : NavNCLException
{
	public NavNSFormNotOpenedException()
		: base(Lang._00095_50070)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSFormNotOpenedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSFormNotOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNSFormNotOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
