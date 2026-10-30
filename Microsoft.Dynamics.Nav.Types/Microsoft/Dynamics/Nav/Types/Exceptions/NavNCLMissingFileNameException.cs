using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMissingFileNameException : NavNCLException
{
	public NavNCLMissingFileNameException()
		: base(Lang._00095_50043)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLMissingFileNameException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLMissingFileNameException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLMissingFileNameException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
