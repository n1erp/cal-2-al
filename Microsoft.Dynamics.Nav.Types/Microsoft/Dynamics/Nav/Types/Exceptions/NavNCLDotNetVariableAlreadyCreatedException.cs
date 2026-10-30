using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDotNetVariableAlreadyCreatedException : NavNCLException
{
	public NavNCLDotNetVariableAlreadyCreatedException()
		: base(Lang.DotNetVariableAlreadyCreated)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLDotNetVariableAlreadyCreatedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLDotNetVariableAlreadyCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDotNetVariableAlreadyCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
