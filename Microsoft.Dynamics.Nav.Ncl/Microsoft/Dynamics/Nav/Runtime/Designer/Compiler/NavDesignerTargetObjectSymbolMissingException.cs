using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Compiler;

[Serializable]
internal class NavDesignerTargetObjectSymbolMissingException : NavBaseException
{
	public NavDesignerTargetObjectSymbolMissingException()
	{
	}

	public NavDesignerTargetObjectSymbolMissingException(string message)
		: base(message)
	{
	}

	public NavDesignerTargetObjectSymbolMissingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDesignerTargetObjectSymbolMissingException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
