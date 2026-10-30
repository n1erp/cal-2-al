using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLBinarySizeExceededException : NavNCLException
{
	public NavNCLBinarySizeExceededException()
		: base(Lang._00095_50085)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLBinarySizeExceededException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLBinarySizeExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLBinarySizeExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
